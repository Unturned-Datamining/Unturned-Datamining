using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableDetonator : Useable
{
    private float startedUse;

    private float useTime;

    private uint lastExplosion;

    private bool isUsing;

    private bool isDetonating;

    private Vector3 chargePoint;

    private List<InteractableCharge> foundInRadius;

    private List<InteractableCharge> chargesInRadius;

    private List<InteractableCharge> charges;

    private InteractableCharge target;

    private static readonly ClientInstanceMethod SendPlayPlunge = ClientInstanceMethod.Get(typeof(UseableDetonator), "ReceivePlayPlunge");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isDetonatable => Time.realtimeSinceStartup - startedUse > useTime * 0.33f;

    private void plunge()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        base.player.animator.play("Use", smooth: false);
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemDetonatorAsset)base.player.equipment.asset).use);
        }
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askPlunge(CSteamID steamID)
    {
        ReceivePlayPlunge();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askPlunge")]
    public void ReceivePlayPlunge()
    {
        if (base.player.equipment.isEquipped)
        {
            plunge();
        }
    }

    public override void startPrimary()
    {
        if (base.player.equipment.isBusy || !isUseable)
        {
            return;
        }
        if (base.channel.isOwner)
        {
            for (int i = 0; i < charges.Count; i++)
            {
                InteractableCharge interactableCharge = charges[i];
                if (!(interactableCharge == null))
                {
                    RaycastInfo info = new RaycastInfo(interactableCharge.transform);
                    base.player.input.sendRaycast(info, ERaycastInfoUsage.Detonator);
                }
            }
            charges.Clear();
        }
        if (Provider.isServer)
        {
            charges.Clear();
            if (base.player.input.hasInputs())
            {
                int inputCount = base.player.input.getInputCount();
                for (int j = 0; j < inputCount; j++)
                {
                    InputInfo input = base.player.input.getInput(doOcclusionCheck: false, ERaycastInfoUsage.Detonator);
                    if (input != null && input.type == ERaycastInfoType.BARRICADE && !(input.transform == null) && input.transform.CompareTag("Barricade"))
                    {
                        InteractableCharge component = input.transform.GetComponent<InteractableCharge>();
                        if (!(component == null) && !(Dedicator.IsDedicatedServer ? (!OwnershipTool.checkToggle(base.channel.owner.playerID.steamID, component.owner, base.player.quests.groupID, component.group)) : (!component.hasOwnership)))
                        {
                            charges.Add(component);
                        }
                    }
                }
            }
        }
        base.player.equipment.isBusy = true;
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        isDetonating = true;
        plunge();
        if (Provider.isServer)
        {
            base.player.life.markAggressive(force: false);
            SendPlayPlunge.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
        }
    }

    public override void startSecondary()
    {
        if (base.player.equipment.isBusy || !base.channel.isOwner || isUsing || !(target != null))
        {
            return;
        }
        if (target.isSelected)
        {
            target.deselect();
            charges.Remove(target);
            return;
        }
        target.select();
        charges.Add(target);
        if (charges.Count > 8)
        {
            if (charges[0] != null)
            {
                charges[0].deselect();
            }
            charges.RemoveAt(0);
        }
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.getAnimationLength("Use");
        if (base.channel.isOwner)
        {
            chargePoint = Vector3.zero;
            foundInRadius = new List<InteractableCharge>();
            chargesInRadius = new List<InteractableCharge>();
        }
        charges = new List<InteractableCharge>();
    }

    public override void dequip()
    {
        if (base.channel.isOwner)
        {
            for (int i = 0; i < chargesInRadius.Count; i++)
            {
                chargesInRadius[i].unhighlight();
            }
        }
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (isDetonating && isDetonatable)
        {
            if (charges.Count > 0)
            {
                if (simulation - lastExplosion > 1)
                {
                    lastExplosion = simulation;
                    InteractableCharge interactableCharge = charges[0];
                    charges.RemoveAt(0);
                    if (interactableCharge != null)
                    {
                        interactableCharge.Detonate(base.player);
                    }
                }
            }
            else
            {
                isDetonating = false;
            }
        }
        if (isUsing && isUseable && charges.Count == 0)
        {
            base.player.equipment.isBusy = false;
            isUsing = false;
        }
    }

    public override void tick()
    {
        if (!base.channel.isOwner)
        {
            return;
        }
        if ((base.transform.position - chargePoint).sqrMagnitude > 1f)
        {
            chargePoint = base.transform.position;
            foundInRadius.Clear();
            PowerTool.checkInteractables(chargePoint, 64f, foundInRadius);
            for (int num = chargesInRadius.Count - 1; num >= 0; num--)
            {
                InteractableCharge interactableCharge = chargesInRadius[num];
                if (interactableCharge == null)
                {
                    chargesInRadius.RemoveAtFast(num);
                }
                else if (!foundInRadius.Contains(interactableCharge))
                {
                    interactableCharge.unhighlight();
                    chargesInRadius.RemoveAtFast(num);
                }
            }
            for (int i = 0; i < foundInRadius.Count; i++)
            {
                InteractableCharge interactableCharge2 = foundInRadius[i];
                if (!(interactableCharge2 == null) && interactableCharge2.hasOwnership && !chargesInRadius.Contains(interactableCharge2))
                {
                    interactableCharge2.highlight();
                    chargesInRadius.Add(interactableCharge2);
                }
            }
        }
        InteractableCharge interactableCharge3 = null;
        float num2 = 0.98f;
        for (int j = 0; j < chargesInRadius.Count; j++)
        {
            InteractableCharge interactableCharge4 = chargesInRadius[j];
            if (!(interactableCharge4 == null))
            {
                float num3 = Vector3.Dot((interactableCharge4.transform.position - MainCamera.instance.transform.position).normalized, MainCamera.instance.transform.forward);
                if (num3 > num2)
                {
                    interactableCharge3 = interactableCharge4;
                    num2 = num3;
                }
            }
        }
        if (interactableCharge3 != target)
        {
            if (target != null)
            {
                target.untarget();
            }
            target = interactableCharge3;
            if (target != null)
            {
                target.target();
            }
        }
    }
}
