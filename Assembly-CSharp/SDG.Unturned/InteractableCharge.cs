using System;
using HighlightingSystem;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableCharge : Interactable
{
    public ulong owner;

    public ulong group;

    private float range2;

    private float playerDamage;

    private float zombieDamage;

    private float animalDamage;

    private float barricadeDamage;

    private float structureDamage;

    private float vehicleDamage;

    private float resourceDamage;

    private float objectDamage;

    public Guid detonationEffectGuid;

    private ushort explosion2;

    private float explosionLaunchSpeed;

    private Highlighter highlighter;

    public bool hasOwnership => OwnershipTool.checkToggle(owner, group);

    public bool isSelected { get; private set; }

    public bool isTargeted { get; private set; }

    public override void updateState(Asset asset, byte[] state)
    {
        range2 = ((ItemChargeAsset)asset).range2;
        playerDamage = ((ItemChargeAsset)asset).playerDamage;
        zombieDamage = ((ItemChargeAsset)asset).zombieDamage;
        animalDamage = ((ItemChargeAsset)asset).animalDamage;
        barricadeDamage = ((ItemChargeAsset)asset).barricadeDamage;
        structureDamage = ((ItemChargeAsset)asset).structureDamage;
        vehicleDamage = ((ItemChargeAsset)asset).vehicleDamage;
        resourceDamage = ((ItemChargeAsset)asset).resourceDamage;
        objectDamage = ((ItemChargeAsset)asset).objectDamage;
        detonationEffectGuid = ((ItemChargeAsset)asset).DetonationEffectGuid;
        explosion2 = ((ItemChargeAsset)asset).explosion2;
        explosionLaunchSpeed = ((ItemChargeAsset)asset).explosionLaunchSpeed;
        isSelected = false;
        isTargeted = false;
        unhighlight();
    }

    public override bool checkInteractable()
    {
        return false;
    }

    public void detonate(CSteamID killer)
    {
        EffectAsset effectAsset = Assets.FindEffectAssetByGuidOrLegacyId(detonationEffectGuid, explosion2);
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters = new TriggerEffectParameters(effectAsset);
            parameters.relevantDistance = EffectManager.LARGE;
            parameters.position = base.transform.position;
            EffectManager.triggerEffect(parameters);
        }
        ExplosionParameters parameters2 = new ExplosionParameters(base.transform.position, range2, EDeathCause.CHARGE, killer);
        parameters2.playerDamage = playerDamage;
        parameters2.zombieDamage = zombieDamage;
        parameters2.animalDamage = animalDamage;
        parameters2.barricadeDamage = barricadeDamage;
        parameters2.structureDamage = structureDamage;
        parameters2.vehicleDamage = vehicleDamage;
        parameters2.resourceDamage = resourceDamage;
        parameters2.objectDamage = objectDamage;
        parameters2.damageOrigin = EDamageOrigin.Charge_Explosion;
        parameters2.launchSpeed = explosionLaunchSpeed;
        DamageTool.explode(parameters2, out var _);
        BarricadeManager.damage(base.transform, 5f, 1f, armor: false, killer, EDamageOrigin.Charge_Self_Destruct);
    }

    public void select()
    {
        if (!isSelected)
        {
            isSelected = true;
            updateHighlight();
        }
    }

    public void deselect()
    {
        if (isSelected)
        {
            isSelected = false;
            updateHighlight();
        }
    }

    public void target()
    {
        if (!isTargeted)
        {
            isTargeted = true;
            updateHighlight();
        }
    }

    public void untarget()
    {
        if (isTargeted)
        {
            isTargeted = false;
            updateHighlight();
        }
    }

    public void highlight()
    {
        if (!(highlighter != null))
        {
            highlighter = base.gameObject.AddComponent<Highlighter>();
            updateHighlight();
        }
    }

    public void unhighlight()
    {
        if (!(highlighter == null))
        {
            UnityEngine.Object.DestroyImmediate(highlighter);
            highlighter = null;
            isSelected = false;
            isTargeted = false;
        }
    }

    private void updateHighlight()
    {
        if (highlighter == null)
        {
            return;
        }
        if (isSelected)
        {
            if (isTargeted)
            {
                highlighter.ConstantOn(Color.cyan);
            }
            else
            {
                highlighter.ConstantOn(Color.green);
            }
        }
        else if (isTargeted)
        {
            highlighter.ConstantOn(Color.yellow);
        }
        else
        {
            highlighter.ConstantOn(Color.red);
        }
    }
}
