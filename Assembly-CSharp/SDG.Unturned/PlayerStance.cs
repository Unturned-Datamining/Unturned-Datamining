using System;
using System.Collections.Generic;
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerStance : PlayerCaller
{
    public static readonly float COOLDOWN = 0.5f;

    public static readonly float RADIUS = 0.4f;

    public static readonly float DETECT_MOVE = 1.1f;

    public static readonly float DETECT_FORWARD = 48f;

    public static readonly float DETECT_BACKWARD = 24f;

    public static readonly float DETECT_SPRINT = 20f;

    public static readonly float DETECT_STAND = 12f;

    public static readonly float DETECT_CROUCH = 6f;

    public static readonly float DETECT_PRONE = 3f;

    public StanceUpdated onStanceUpdated;

    private EPlayerStance _stance;

    /// <summary>
    /// Stance to fit available space when loading in.
    /// </summary>
    public EPlayerStance initialStance = EPlayerStance.STAND;

    private float lastStance;

    private float lastSubmergeSound;

    private float lastDetect;

    private float lastHold;

    private bool isHolding;

    private bool _localWantsToCrouch;

    private bool _localWantsToProne;

    private bool _localWantsToSprint;

    internal bool localWantsToSteadyAim;

    private bool _isSubmerged;

    private bool _inShallows;

    private RaycastHit ladder;

    /// <summary>
    /// Regular interact ray still hits the ladder, but we only allow climbing within a smaller range to make its
    /// teleport less powerful.
    /// </summary>
    internal const float LADDER_INTERACT_RANGE = 4f;

    /// <summary>
    /// Ladder forward ray is 0.75m, so we move slightly less than that away from the ladder.
    /// </summary>
    internal const float LADDER_INTERACT_TELEPORT_OFFSET = 0.65f;

    internal static readonly ServerInstanceMethod<Vector3> SendClimbRequest = ServerInstanceMethod<Vector3>.Get(typeof(PlayerStance), "ReceiveClimbRequest");

    private static readonly ClientInstanceMethod<EPlayerStance> SendStance = ClientInstanceMethod<EPlayerStance>.Get(typeof(PlayerStance), "ReceiveStance");

    public EPlayerStance stance
    {
        get
        {
            return _stance;
        }
        set
        {
            checkStance(value, all: true);
        }
    }

    [Obsolete("Renamed to GetStealthDetectionRadius.")]
    public float radius => GetStealthDetectionRadius();

    public bool crouch => _localWantsToCrouch;

    public bool prone => _localWantsToProne;

    public bool sprint => _localWantsToSprint;

    /// <summary>
    /// Older, cached version of areEyesUnderwater.
    /// </summary>
    public bool isSubmerged => _isSubmerged;

    internal bool canCurrentStanceTransitionToClimbing
    {
        get
        {
            if (stance != EPlayerStance.STAND && stance != EPlayerStance.SPRINT)
            {
                return stance == EPlayerStance.SWIM;
            }
            return true;
        }
    }

    /// <summary>
    /// Return false if there are any external restrictions (e.g. reloading, handcuffed) preventing climbing.
    /// </summary>
    internal bool isAllowedToStartClimbing
    {
        get
        {
            if (!base.player.equipment.isBusy)
            {
                return base.player.animator.gesture != EPlayerGesture.ARREST_START;
            }
            return false;
        }
    }

    /// <summary>
    /// Test whether bottom of controller is currently inside a water volume.
    /// </summary>
    public bool areFeetUnderwater => WaterUtility.isPointUnderwater(base.transform.position);

    /// <summary>
    /// Test whether viewpoint is currently inside a water volume.
    /// </summary>
    public bool areEyesUnderwater => WaterUtility.isPointUnderwater(base.player.look.aim.position);

    /// <summary>
    /// Test whether body is currently inside a water volume.
    /// Enters the swimming stance while true.
    /// </summary>
    public bool isBodyUnderwater => WaterUtility.isPointUnderwater(base.transform.position + new Vector3(0f, 1.25f, 0f));

    /// <summary>
    /// Invoked after any player's stance changes (not including loading).
    /// </summary>
    public static event Action<PlayerStance> OnStanceChanged_Global;

    /// <returns>Distance zombies can detect this player within.</returns>
    public float GetStealthDetectionRadius()
    {
        if (base.player.movement.nav != byte.MaxValue && ZombieManager.regions[base.player.movement.nav].isHyper)
        {
            return 24f;
        }
        if (stance == EPlayerStance.DRIVING)
        {
            if (base.player.movement.getVehicle().sirensOn)
            {
                return DETECT_FORWARD;
            }
            return DETECT_FORWARD * base.player.movement.getVehicle().GetReplicatedForwardSpeedPercentageOfTargetSpeed();
        }
        if (stance == EPlayerStance.SITTING)
        {
            return 0f;
        }
        if (stance == EPlayerStance.SPRINT)
        {
            return DETECT_SPRINT * (base.player.movement.isMoving ? DETECT_MOVE : 1f);
        }
        if (stance == EPlayerStance.STAND || stance == EPlayerStance.SWIM)
        {
            float num = 1f - base.player.skills.mastery(1, 0) * 0.5f;
            return DETECT_STAND * (base.player.movement.isMoving ? DETECT_MOVE : 1f) * num;
        }
        float num2 = 1f - base.player.skills.mastery(1, 0) * 0.75f;
        if (stance == EPlayerStance.CROUCH || stance == EPlayerStance.CLIMB)
        {
            return DETECT_CROUCH * (base.player.movement.isMoving ? DETECT_MOVE : 1f) * num2;
        }
        if (stance == EPlayerStance.PRONE)
        {
            return DETECT_PRONE * (base.player.movement.isMoving ? DETECT_MOVE : 1f) * num2;
        }
        return 0f;
    }

    /// <summary>
    /// Draw debug capsule matching the player size.
    /// </summary>
    public static void drawCapsule(Vector3 position, float height, Color color, float lifespan = 0f)
    {
        Vector3 begin = position + new Vector3(0f, RADIUS, 0f);
        Vector3 end = position + new Vector3(0f, height - RADIUS, 0f);
        RuntimeGizmos.Get().Capsule(begin, end, RADIUS, color, lifespan);
    }

    /// <summary>
    /// Draw standing-height debug capsule matching the player size.
    /// </summary>
    public static void drawStandingCapsule(Vector3 position, Color color, float lifespan = 0f)
    {
        Vector3 begin = position + new Vector3(0f, RADIUS, 0f);
        Vector3 end = position + new Vector3(0f, PlayerMovement.HEIGHT_STAND - RADIUS, 0f);
        RuntimeGizmos.Get().Capsule(begin, end, RADIUS, color, lifespan);
    }

    /// <summary>
    /// Is there enough height for our capsule at a position?
    /// </summary>
    public static bool hasHeightClearanceAtPosition(Vector3 position, float height)
    {
        Vector3 start = position + new Vector3(0f, RADIUS + 0.01f, 0f);
        Vector3 end = position + new Vector3(0f, height - RADIUS, 0f);
        if (Physics.CheckCapsule(start, end, RADIUS, RayMasks.BLOCK_STANCE & -21, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        if (Physics.Linecast(position + new Vector3(0f, height, 0f), position + new Vector3(0f, 0.01f, 0f), 1048576))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Could a standing player capsule fit at the given position?
    /// </summary>
    public static bool hasStandingHeightClearanceAtPosition(Vector3 position)
    {
        return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_STAND);
    }

    /// <summary>
    /// Could a crouching player capsule fit at the given position?
    /// </summary>
    public static bool hasCrouchingHeightClearanceAtPosition(Vector3 position)
    {
        return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_CROUCH);
    }

    /// <summary>
    /// Could a prone player capsule fit at the given position?
    /// </summary>
    public static bool hasProneHeightClearanceAtPosition(Vector3 position)
    {
        return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_PRONE);
    }

    /// <summary>
    /// Could a standing player capsule teleport to the given position?
    /// </summary>
    public static bool hasTeleportClearanceAtPosition(Vector3 position)
    {
        return hasHeightClearanceAtPosition(position, PlayerMovement.HEIGHT_STAND + 0.5f);
    }

    /// <summary>
    /// Is there any compatible stance that can fit at position?
    /// </summary>
    public static bool getStanceForPosition(Vector3 position, ref EPlayerStance stance)
    {
        if (hasStandingHeightClearanceAtPosition(position))
        {
            stance = EPlayerStance.STAND;
            return true;
        }
        if (hasCrouchingHeightClearanceAtPosition(position))
        {
            stance = EPlayerStance.CROUCH;
            return true;
        }
        if (hasProneHeightClearanceAtPosition(position))
        {
            stance = EPlayerStance.PRONE;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Using our capsule's current height would there be enough space at a given position?
    /// </summary>
    public bool wouldHaveHeightClearanceAtPosition(Vector3 position, float padding = 0f)
    {
        CharacterController controller = base.player.movement.controller;
        float num = ((controller != null) ? controller.height : PlayerMovement.HEIGHT_STAND);
        return hasHeightClearanceAtPosition(position, num + padding);
    }

    /// <summary>
    /// Does capsule have appropriate clearance for a pending height change?
    /// </summary>
    public bool hasHeightClearance(float height)
    {
        return hasHeightClearanceAtPosition(base.transform.position, height);
    }

    private EPlayerHeight getHeightForStance(EPlayerStance testStance)
    {
        return testStance switch
        {
            EPlayerStance.PRONE => EPlayerHeight.PRONE, 
            EPlayerStance.CROUCH => EPlayerHeight.CROUCH, 
            _ => EPlayerHeight.STAND, 
        };
    }

    internal void internalSetStance(EPlayerStance newStance)
    {
        if (_stance != newStance)
        {
            _stance = newStance;
            EPlayerHeight heightForStance = getHeightForStance(newStance);
            base.player.movement.setSize(heightForStance);
            onStanceUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Replicate stance to clients.
    /// </summary>
    private void replicateStance(bool notifyOwner)
    {
        if (notifyOwner)
        {
            SendStance.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), stance);
        }
        else
        {
            SendStance.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GatherRemoteClientConnectionsExcludingOwner(), stance);
        }
    }

    public void checkStance(EPlayerStance newStance)
    {
        checkStance(newStance, all: false);
    }

    public void checkStance(EPlayerStance newStance, bool all)
    {
        if ((base.player.movement.getVehicle() != null && newStance != EPlayerStance.DRIVING && newStance != EPlayerStance.SITTING) || newStance == stance || ((newStance == EPlayerStance.PRONE || newStance == EPlayerStance.CROUCH) && _inShallows))
        {
            return;
        }
        if ((stance == EPlayerStance.CROUCH && newStance == EPlayerStance.STAND) || (stance == EPlayerStance.PRONE && (newStance == EPlayerStance.CROUCH || newStance == EPlayerStance.STAND)))
        {
            if (!(Time.realtimeSinceStartup - lastStance > COOLDOWN))
            {
                return;
            }
            lastStance = Time.realtimeSinceStartup;
        }
        if (newStance != EPlayerStance.DRIVING && newStance != EPlayerStance.SITTING)
        {
            EPlayerHeight height = base.player.movement.height;
            EPlayerHeight heightForStance = getHeightForStance(newStance);
            if (heightForStance != height)
            {
                switch (heightForStance)
                {
                case EPlayerHeight.STAND:
                    if (!hasHeightClearance(PlayerMovement.HEIGHT_STAND))
                    {
                        return;
                    }
                    break;
                case EPlayerHeight.CROUCH:
                    if (height == EPlayerHeight.PRONE && !hasHeightClearance(PlayerMovement.HEIGHT_CROUCH))
                    {
                        return;
                    }
                    break;
                }
            }
        }
        if (Provider.isServer)
        {
            if (base.player.animator.gesture == EPlayerGesture.INVENTORY_START)
            {
                if (newStance != EPlayerStance.STAND && newStance != EPlayerStance.SPRINT && newStance != EPlayerStance.CROUCH)
                {
                    base.player.animator.sendGesture(EPlayerGesture.INVENTORY_STOP, all: false);
                }
            }
            else if (base.player.animator.gesture == EPlayerGesture.SURRENDER_START)
            {
                base.player.animator.sendGesture(EPlayerGesture.SURRENDER_STOP, all: true);
            }
            else if (base.player.animator.gesture == EPlayerGesture.REST_START)
            {
                base.player.animator.sendGesture(EPlayerGesture.REST_STOP, all: true);
            }
        }
        internalSetStance(newStance);
        if (Provider.isServer)
        {
            replicateStance(all);
        }
    }

    public bool adjustStanceOrTeleportIfStuck()
    {
        EPlayerStance newStance = stance;
        if (getStanceForPosition(base.transform.position, ref newStance))
        {
            internalSetStance(newStance);
            replicateStance(Dedicator.IsDedicatedServer);
            return true;
        }
        return base.player.teleportToRandomSpawnPoint();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2)]
    public void ReceiveClimbRequest(in ServerInvocationContext context, Vector3 direction)
    {
        if (!canCurrentStanceTransitionToClimbing || !isAllowedToStartClimbing || !Physics.SphereCast(new Ray(base.player.look.aim.position, direction), RADIUS, out var hitInfo, 4f, RayMasks.LADDER_INTERACT) || hitInfo.collider == null || !hitInfo.collider.CompareTag("Ladder") || !Physics.Raycast(new Ray(base.player.look.aim.position, direction), out var hitInfo2, 4f, RayMasks.LADDER_INTERACT) || hitInfo2.collider == null || !hitInfo2.collider.CompareTag("Ladder"))
        {
            return;
        }
        float num = Vector3.Dot(hitInfo2.normal, hitInfo2.collider.transform.up);
        if (Mathf.Abs(num) <= 0.9f || Mathf.Abs(Vector3.Dot(Vector3.up, hitInfo2.collider.transform.up)) > 0.1f)
        {
            return;
        }
        Vector3 vector = new Vector3(hitInfo2.collider.transform.position.x, hitInfo2.point.y - 0.5f - 0.5f - 0.1f, hitInfo2.collider.transform.position.z) + hitInfo2.normal * 0.65f;
        float num2 = PlayerMovement.HEIGHT_STAND + 0.1f + 0.5f;
        Vector3 end = vector + new Vector3(0f, num2 * 0.5f, 0f);
        if (!Physics.Linecast(hitInfo2.point, end, out var _, RayMasks.BLOCK_STANCE, QueryTriggerInteraction.Collide) && hasHeightClearanceAtPosition(vector, num2))
        {
            float num3 = hitInfo2.collider.transform.rotation.eulerAngles.y;
            if (num < 0f)
            {
                num3 += 180f;
            }
            base.player.teleportToLocation(vector, num3);
        }
    }

    [Obsolete]
    public void tellStance(CSteamID steamID, byte newStance)
    {
        ReceiveStance((EPlayerStance)newStance);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellStance")]
    public void ReceiveStance(EPlayerStance newStance)
    {
        internalSetStance(newStance);
        if (stance == EPlayerStance.CROUCH)
        {
            if (ControlsSettings.crouching == EControlMode.TOGGLE)
            {
                _localWantsToCrouch = true;
                _localWantsToProne = false;
            }
        }
        else if (stance == EPlayerStance.PRONE && ControlsSettings.proning == EControlMode.TOGGLE)
        {
            _localWantsToCrouch = false;
            _localWantsToProne = true;
        }
        PlayerStance.OnStanceChanged_Global?.Invoke(this);
    }

    [Obsolete]
    public void askStance(CSteamID steamID)
    {
    }

    internal void SendInitialPlayerState(SteamPlayer client)
    {
        SendStance.Invoke(GetNetId(), ENetReliability.Reliable, client.transportConnection, stance);
    }

    internal void SendInitialPlayerState(List<ITransportConnection> transportConnections)
    {
        SendStance.Invoke(GetNetId(), ENetReliability.Reliable, transportConnections, stance);
    }

    public void simulate(uint simulation, bool inputCrouch, bool inputProne, bool inputSprint)
    {
        _isSubmerged = areEyesUnderwater;
        _inShallows = areFeetUnderwater;
        if (stance == EPlayerStance.CLIMB || (canCurrentStanceTransitionToClimbing && isAllowedToStartClimbing))
        {
            Physics.Raycast(new Ray(base.transform.position + Vector3.up * 0.5f, base.transform.forward), out ladder, 0.75f, RayMasks.LADDER_INTERACT);
            if (ladder.collider != null && ladder.collider.transform.CompareTag("Ladder") && Mathf.Abs(Vector3.Dot(ladder.normal, ladder.collider.transform.up)) > 0.9f)
            {
                if (stance != 0)
                {
                    Vector3 vector = new Vector3(ladder.collider.transform.position.x, ladder.point.y - 0.5f, ladder.collider.transform.position.z) + ladder.normal * 0.5f;
                    if (!Physics.CapsuleCast(base.transform.position + new Vector3(0f, RADIUS, 0f), base.transform.position + new Vector3(0f, PlayerMovement.HEIGHT_STAND - RADIUS, 0f), RADIUS, (vector - base.transform.position).normalized, (vector - base.transform.position).magnitude, RayMasks.BLOCK_LADDER, QueryTriggerInteraction.Ignore) && !Physics.CheckCapsule(vector + new Vector3(0f, RADIUS, 0f), vector + new Vector3(0f, PlayerMovement.HEIGHT_STAND - RADIUS, 0f), RADIUS, RayMasks.BLOCK_LADDER, QueryTriggerInteraction.Ignore))
                    {
                        base.transform.position = vector;
                        checkStance(EPlayerStance.CLIMB);
                    }
                }
                if (stance == EPlayerStance.CLIMB)
                {
                    return;
                }
            }
            else if (stance == EPlayerStance.CLIMB)
            {
                checkStance(EPlayerStance.STAND);
            }
        }
        if (isBodyUnderwater)
        {
            if (stance != EPlayerStance.SWIM)
            {
                checkStance(EPlayerStance.SWIM);
                if (stance == EPlayerStance.SWIM && !base.player.input.isResimulating && !Dedicator.IsDedicatedServer && Time.time - lastSubmergeSound > 0.1f)
                {
                    lastSubmergeSound = Time.time;
                    base.player.movement.PlaySwimAudioClip();
                }
            }
            return;
        }
        if (_inShallows)
        {
            if (stance != EPlayerStance.STAND && stance != EPlayerStance.SPRINT)
            {
                checkStance(EPlayerStance.STAND);
            }
        }
        else if (stance == EPlayerStance.SWIM)
        {
            checkStance(EPlayerStance.STAND);
        }
        if (stance == EPlayerStance.CLIMB || stance == EPlayerStance.SITTING || stance == EPlayerStance.DRIVING)
        {
            return;
        }
        if (inputCrouch || (base.player.animator.gesture == EPlayerGesture.REST_START && !inputProne))
        {
            if (stance != EPlayerStance.CROUCH)
            {
                checkStance(EPlayerStance.CROUCH);
            }
        }
        else if (inputProne)
        {
            if (stance != EPlayerStance.PRONE)
            {
                checkStance(EPlayerStance.PRONE);
            }
        }
        else if (stance == EPlayerStance.CROUCH || stance == EPlayerStance.PRONE)
        {
            checkStance(EPlayerStance.STAND);
        }
        bool flag = true;
        if (base.player.equipment.useable is UseableGun useableGun && base.player.equipment.asset is ItemGunAsset itemGunAsset)
        {
            flag = itemGunAsset.canAimDuringSprint || !useableGun.isAiming;
        }
        if (inputSprint && !base.player.life.isBroken && base.player.life.stamina > 0 && flag && base.player.movement.isMoving)
        {
            if (stance == EPlayerStance.STAND)
            {
                checkStance(EPlayerStance.SPRINT);
            }
        }
        else if (stance == EPlayerStance.SPRINT)
        {
            checkStance(EPlayerStance.STAND);
        }
    }

    private void onLifeUpdated(bool isDead)
    {
        if (!isDead)
        {
            checkStance(EPlayerStance.STAND);
        }
    }

    private void Update()
    {
        if (base.channel.IsLocalPlayer && !PlayerUI.window.showCursor)
        {
            if (!base.player.look.isOrbiting)
            {
                if (InputEx.GetKey(ControlsSettings.stance))
                {
                    if (isHolding)
                    {
                        if (Time.realtimeSinceStartup - lastHold > 0.33f)
                        {
                            _localWantsToCrouch = false;
                            _localWantsToProne = true;
                        }
                    }
                    else
                    {
                        isHolding = true;
                        lastHold = Time.realtimeSinceStartup;
                    }
                }
                else if (isHolding)
                {
                    if (Time.realtimeSinceStartup - lastHold < 0.33f)
                    {
                        if (crouch)
                        {
                            _localWantsToCrouch = false;
                            _localWantsToProne = false;
                        }
                        else
                        {
                            _localWantsToCrouch = true;
                            _localWantsToProne = false;
                        }
                    }
                    isHolding = false;
                }
                if (base.player.animator.gesture == EPlayerGesture.REST_START)
                {
                    if (InputEx.GetKeyDown(ControlsSettings.crouch))
                    {
                        base.player.animator.sendGesture(EPlayerGesture.REST_STOP, all: true);
                        _localWantsToCrouch = true;
                        if (_localWantsToProne)
                        {
                            _localWantsToProne = false;
                        }
                    }
                }
                else if (ControlsSettings.crouching == EControlMode.TOGGLE)
                {
                    if (InputEx.GetKeyDown(ControlsSettings.crouch))
                    {
                        _localWantsToCrouch = !crouch;
                        if (_localWantsToCrouch)
                        {
                            _localWantsToProne = false;
                        }
                    }
                }
                else
                {
                    _localWantsToCrouch = InputEx.GetKey(ControlsSettings.crouch);
                    if (_localWantsToCrouch)
                    {
                        _localWantsToProne = false;
                    }
                }
                if (ControlsSettings.proning == EControlMode.TOGGLE)
                {
                    if (InputEx.GetKeyDown(ControlsSettings.prone))
                    {
                        _localWantsToProne = !prone;
                        if (_localWantsToProne)
                        {
                            _localWantsToCrouch = false;
                        }
                    }
                }
                else
                {
                    _localWantsToProne = InputEx.GetKey(ControlsSettings.prone);
                    if (_localWantsToProne)
                    {
                        _localWantsToCrouch = false;
                    }
                }
                if (ControlsSettings.sprinting == EControlMode.TOGGLE)
                {
                    if (InputEx.GetKeyDown(ControlsSettings.sprint))
                    {
                        _localWantsToSprint = !sprint;
                    }
                }
                else
                {
                    _localWantsToSprint = InputEx.GetKey(ControlsSettings.sprint);
                }
                localWantsToSteadyAim = InputEx.GetKey(ControlsSettings.sprint);
            }
            if ((stance == EPlayerStance.PRONE || stance == EPlayerStance.CROUCH) && InputEx.GetKey(ControlsSettings.jump))
            {
                _localWantsToCrouch = false;
                _localWantsToProne = false;
            }
            if (_inShallows || stance == EPlayerStance.SWIM || stance == EPlayerStance.CLIMB || stance == EPlayerStance.SITTING || stance == EPlayerStance.DRIVING)
            {
                _localWantsToCrouch = false;
                _localWantsToProne = false;
            }
            if (ControlsSettings.sprinting == EControlMode.TOGGLE && base.player.movement.input_x == 0 && base.player.movement.input_y == 0)
            {
                _localWantsToSprint = false;
            }
            if (PlayerUI.window.showCursor)
            {
                _localWantsToSprint = false;
                localWantsToSteadyAim = false;
            }
        }
        if (Provider.isServer && (double)(Time.realtimeSinceStartup - lastDetect) > 0.1)
        {
            lastDetect = Time.realtimeSinceStartup;
            if (base.player.life.IsAlive)
            {
                AlertTool.alert(base.player, base.transform.position, GetStealthDetectionRadius(), stance != EPlayerStance.SPRINT && stance != EPlayerStance.DRIVING, base.player.look.aim.forward, base.player.isSpotOn);
            }
        }
    }

    internal void InitializePlayer()
    {
        _stance = EPlayerStance.STAND;
        if (base.channel.IsLocalPlayer || Provider.isServer)
        {
            lastStance = 0f;
            lastSubmergeSound = 0f;
            PlayerLife life = base.player.life;
            life.onLifeUpdated = (LifeUpdated)Delegate.Combine(life.onLifeUpdated, new LifeUpdated(onLifeUpdated));
        }
        if (Provider.isServer)
        {
            internalSetStance(initialStance);
        }
    }
}
