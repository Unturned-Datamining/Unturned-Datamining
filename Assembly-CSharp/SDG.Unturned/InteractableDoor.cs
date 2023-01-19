using System;
using System.Collections;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableDoor : Interactable
{
    public static Collider[] checkColliders = new Collider[1];

    private CSteamID _owner;

    private CSteamID _group;

    private bool _isOpen;

    private bool isLocked;

    private float opened;

    private Transform barrierTransform;

    private List<Collider> doorColliders;

    private BoxCollider placeholderCollider;

    protected Coroutine animCoroutine;

    internal static readonly ClientInstanceMethod<bool> SendOpen = ClientInstanceMethod<bool>.Get(typeof(InteractableDoor), "ReceiveOpen");

    private static readonly ServerInstanceMethod<bool> SendToggleRequest = ServerInstanceMethod<bool>.Get(typeof(InteractableDoor), "ReceiveToggleRequest");

    public CSteamID owner => _owner;

    public CSteamID group => _group;

    public bool isOpen => _isOpen;

    public bool isOpenable => Time.realtimeSinceStartup - opened > 0.75f;

    public static event Action<InteractableDoor> OnDoorChanged_Global;

    public bool checkToggle(CSteamID enemyPlayer, CSteamID enemyGroup)
    {
        if (Provider.isServer && placeholderCollider != null && overlapBox(placeholderCollider) > 0)
        {
            return false;
        }
        if (Provider.isServer && !Dedicator.IsDedicatedServer)
        {
            return true;
        }
        if (isLocked && !(enemyPlayer == owner))
        {
            if (group != CSteamID.Nil)
            {
                return enemyGroup == group;
            }
            return false;
        }
        return true;
    }

    public void updateToggle(bool newOpen)
    {
        opened = Time.realtimeSinceStartup;
        _isOpen = newOpen;
        Animation component = GetComponent<Animation>();
        if (component != null)
        {
            playAnimation(component, applyInstantly: false);
        }
        if (!Dedicator.IsDedicatedServer)
        {
            GetComponent<AudioSource>().Play();
        }
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
        if (barrierTransform != null)
        {
            barrierTransform.gameObject.SetActive(!isOpen);
        }
        InteractableDoor.OnDoorChanged_Global.TryInvoke("OnDoorChanged_Global", this);
    }

    public override void updateState(Asset asset, byte[] state)
    {
        isLocked = ((ItemBarricadeAsset)asset).isLocked;
        _owner = new CSteamID(BitConverter.ToUInt64(state, 0));
        _group = new CSteamID(BitConverter.ToUInt64(state, 8));
        _isOpen = state[16] == 1;
        Animation component = GetComponent<Animation>();
        if (component != null)
        {
            playAnimation(component, applyInstantly: true);
        }
        Transform transform = base.transform.Find("Placeholder");
        if (transform != null)
        {
            placeholderCollider = transform.GetComponent<BoxCollider>();
        }
        else
        {
            placeholderCollider = null;
        }
        if (barrierTransform != null)
        {
            barrierTransform.gameObject.SetActive(!isOpen);
        }
        if (((ItemBarricadeAsset)asset).allowCollisionWhileAnimating)
        {
            doorColliders = null;
        }
        else if (doorColliders == null)
        {
            doorColliders = new List<Collider>();
        }
    }

    protected virtual void Start()
    {
        if (placeholderCollider != null && (base.transform.parent == null || !base.transform.parent.CompareTag("Vehicle")))
        {
            barrierTransform = UnityEngine.Object.Instantiate(placeholderCollider.gameObject).transform;
            barrierTransform.position = placeholderCollider.transform.position;
            barrierTransform.rotation = placeholderCollider.transform.rotation;
            barrierTransform.tag = "Barricade";
            barrierTransform.name = "ExpandedBarrier";
            barrierTransform.gameObject.layer = 27;
            barrierTransform.parent = base.transform;
            Rigidbody component = barrierTransform.GetComponent<Rigidbody>();
            if (component != null)
            {
                UnityEngine.Object.Destroy(component);
            }
            BoxCollider component2 = barrierTransform.GetComponent<BoxCollider>();
            if (component2 != null)
            {
                component2.size = new Vector3(component2.size.x + 0.25f, component2.size.y + 0.25f, 0.1f);
            }
            barrierTransform.gameObject.SetActive(!isOpen);
        }
        if (doorColliders == null)
        {
            return;
        }
        GetComponentsInChildren(doorColliders);
        for (int num = doorColliders.Count - 1; num >= 0; num--)
        {
            Collider collider = doorColliders[num];
            if (collider == placeholderCollider || collider.transform == barrierTransform)
            {
                doorColliders.RemoveAtFast(num);
            }
        }
    }

    protected void playAnimation(Animation animationComponent, bool applyInstantly)
    {
        string animation = (isOpen ? "Open" : "Close");
        if (animationComponent.GetClip(animation) == null)
        {
            return;
        }
        animationComponent.Play(animation);
        if (applyInstantly)
        {
            animationComponent[animation].normalizedTime = 1f;
        }
        else if (doorColliders != null && doorColliders.Count > 0)
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
            }
            float length = animationComponent[animation].length;
            animCoroutine = StartCoroutine(disableAnimatedColliders(length));
        }
    }

    protected int overlapBox(BoxCollider boxCollider)
    {
        int mask = ((base.transform.parent != null && base.transform.parent.CompareTag("Vehicle")) ? RayMasks.BLOCK_CHAR_HINGE_OVERLAP_ON_VEHICLE : RayMasks.BLOCK_CHAR_HINGE_OVERLAP);
        return CollisionUtil.OverlapBoxColliderNonAlloc(boxCollider, checkColliders, mask, QueryTriggerInteraction.Collide);
    }

    protected bool areAnimatedCollidersOverlapping()
    {
        foreach (Collider doorCollider in doorColliders)
        {
            BoxCollider boxCollider = doorCollider as BoxCollider;
            if (boxCollider != null && overlapBox(boxCollider) > 0)
            {
                return true;
            }
        }
        return false;
    }

    protected IEnumerator disableAnimatedColliders(float delay)
    {
        foreach (Collider doorCollider in doorColliders)
        {
            doorCollider.enabled = false;
        }
        yield return new WaitForSeconds(delay);
        while (areAnimatedCollidersOverlapping())
        {
            yield return new WaitForSeconds(0.1f);
        }
        foreach (Collider doorCollider2 in doorColliders)
        {
            doorCollider2.enabled = true;
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveOpen(bool newOpen)
    {
        updateToggle(newOpen);
    }

    public void ClientToggle()
    {
        SendToggleRequest.Invoke(GetNetId(), ENetReliability.Unreliable, !isOpen);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveToggleRequest(in ServerInvocationContext context, bool desiredOpen)
    {
        if (isOpen != desiredOpen && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            Player player = context.GetPlayer();
            if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f) && isOpenable && checkToggle(player.channel.owner.playerID.steamID, player.quests.groupID))
            {
                BarricadeManager.ServerSetDoorOpenInternal(this, x, y, plant, region, !isOpen);
            }
        }
    }
}
