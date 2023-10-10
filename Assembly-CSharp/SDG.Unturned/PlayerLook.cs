using System;
using SDG.Framework.Foliage;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SDG.Unturned;

public class PlayerLook : PlayerCaller
{
    private static readonly float HEIGHT_LOOK_SIT = 1.6f;

    private static readonly float HEIGHT_LOOK_STAND = 1.75f;

    private static readonly float HEIGHT_LOOK_CROUCH = 1.2f;

    private static readonly float HEIGHT_LOOK_PRONE = 0.35f;

    private static readonly float HEIGHT_CAMERA_SIT = 0.7f;

    private static readonly float HEIGHT_CAMERA_STAND = 1.05f;

    private static readonly float HEIGHT_CAMERA_CROUCH = 0.95f;

    private static readonly float HEIGHT_CAMERA_PRONE = 0.3f;

    private static readonly float MIN_ANGLE_SIT = 60f;

    private static readonly float MAX_ANGLE_SIT = 120f;

    private static readonly float MIN_ANGLE_CLIMB = 45f;

    private static readonly float MAX_ANGLE_CLIMB = 100f;

    private static readonly float MIN_ANGLE_SWIM = 45f;

    private static readonly float MAX_ANGLE_SWIM = 135f;

    private static readonly float MIN_ANGLE_STAND = 0f;

    private static readonly float MAX_ANGLE_STAND = 180f;

    private static readonly float MIN_ANGLE_CROUCH = 20f;

    private static readonly float MAX_ANGLE_CROUCH = 160f;

    private static readonly float MIN_ANGLE_PRONE = 60f;

    private static readonly float MAX_ANGLE_PRONE = 120f;

    public PerspectiveUpdated onPerspectiveUpdated;

    private Camera _characterCamera;

    private Camera _scopeCamera;

    private bool _isScopeActive;

    private bool isOverlayActive;

    private ELightingVision scopeVision;

    private Color scopeNightvisionColor;

    private float scopeNightvisionFogIntensity;

    private ELightingVision tempVision;

    private Color tempNightvisionColor;

    private float tempNightvisionFogIntensity;

    private Transform _aim;

    private static float characterHeight;

    private static float _characterYaw;

    public static float characterYaw;

    private static float killcam;

    private float yawInputMultiplier;

    private float pitchInputMultiplier;

    private float _pitch;

    private float _yaw;

    private float _look_x;

    private float _look_y;

    private float _orbitPitch;

    private float _orbitYaw;

    public float orbitSpeed = 16f;

    public Vector3 lockPosition;

    public Vector3 orbitPosition;

    public bool isOrbiting;

    public bool isTracking;

    public bool isLocking;

    public bool isFocusing;

    public bool isSmoothing;

    public bool isIgnoringInput;

    private Vector3 smoothPosition;

    private Quaternion smoothRotation;

    public byte angle;

    public byte rot;

    private float recoil_x;

    private float recoil_y;

    public byte lastAngle;

    public byte lastRot;

    private Quaternion flinchLocalRotation;

    public Rk4SpringQ targetExplosionLocalRotation;

    private Quaternion smoothedExplosionLocalRotation = Quaternion.identity;

    public float explosionSmoothingSpeed;

    internal float mainCameraZoomFactor;

    private float scopeCameraZoomFactor;

    private float eyes;

    private float thirdPersonEyeHeight;

    public bool shouldUseZoomFactorForSensitivity;

    private EPlayerPerspective _perspective;

    private RenderTexture scopeRenderTexture;

    protected bool isZoomed;

    protected bool allowFreecamWithoutAdmin;

    protected bool allowWorkzoneWithoutAdmin;

    protected bool allowSpecStatsWithoutAdmin;

    private static readonly ClientInstanceMethod<bool> SendFreecamAllowed = ClientInstanceMethod<bool>.Get(typeof(PlayerLook), "ReceiveFreecamAllowed");

    private static readonly ClientInstanceMethod<bool> SendWorkzoneAllowed = ClientInstanceMethod<bool>.Get(typeof(PlayerLook), "ReceiveWorkzoneAllowed");

    private static readonly ClientInstanceMethod<bool> SendSpecStatsAllowed = ClientInstanceMethod<bool>.Get(typeof(PlayerLook), "ReceiveSpecStatsAllowed");

    private static RaycastHit[] sweepHits = new RaycastHit[8];

    private const float NEAR_CLIP_SWEEP_RADIUS = 0.39f;

    public float heightLook
    {
        get
        {
            if (base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING)
            {
                return HEIGHT_LOOK_SIT;
            }
            if (base.player.stance.stance == EPlayerStance.STAND || base.player.stance.stance == EPlayerStance.SPRINT || base.player.stance.stance == EPlayerStance.CLIMB || base.player.stance.stance == EPlayerStance.SWIM || base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING)
            {
                return HEIGHT_LOOK_STAND;
            }
            if (base.player.stance.stance == EPlayerStance.CROUCH)
            {
                return HEIGHT_LOOK_CROUCH;
            }
            if (base.player.stance.stance == EPlayerStance.PRONE)
            {
                return HEIGHT_LOOK_PRONE;
            }
            return 0f;
        }
    }

    private float heightCamera
    {
        get
        {
            if (base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING)
            {
                return HEIGHT_CAMERA_SIT;
            }
            if (base.player.stance.stance == EPlayerStance.STAND || base.player.stance.stance == EPlayerStance.SPRINT || base.player.stance.stance == EPlayerStance.CLIMB || base.player.stance.stance == EPlayerStance.SWIM || base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING)
            {
                return HEIGHT_CAMERA_STAND;
            }
            if (base.player.stance.stance == EPlayerStance.CROUCH)
            {
                return HEIGHT_CAMERA_CROUCH;
            }
            if (base.player.stance.stance == EPlayerStance.PRONE)
            {
                return HEIGHT_CAMERA_PRONE;
            }
            return 0f;
        }
    }

    public Camera characterCamera => _characterCamera;

    public Camera scopeCamera => _scopeCamera;

    public Material scopeMaterial { get; private set; }

    public bool isScopeActive => _isScopeActive;

    public Transform aim => _aim;

    public float pitch => _pitch;

    public float yaw => _yaw;

    public float look_x => _look_x;

    public float look_y => _look_y;

    public float orbitPitch => _orbitPitch;

    public float orbitYaw => _orbitYaw;

    public bool areSpecStatsVisible { get; protected set; }

    public bool isCam
    {
        get
        {
            if (!isOrbiting && !isTracking && !isLocking)
            {
                return isFocusing;
            }
            return true;
        }
    }

    public EPlayerPerspective perspective => _perspective;

    public bool canUseFreecam
    {
        get
        {
            if (allowFreecamWithoutAdmin)
            {
                return true;
            }
            return base.channel.owner.isAdmin;
        }
    }

    public bool canUseWorkzone
    {
        get
        {
            if (allowWorkzoneWithoutAdmin)
            {
                return true;
            }
            return base.channel.owner.isAdmin;
        }
    }

    public bool canUseSpecStats
    {
        get
        {
            if (allowSpecStatsWithoutAdmin)
            {
                return true;
            }
            return base.channel.owner.isAdmin;
        }
    }

    internal void TeleportYaw(float newYaw)
    {
        _yaw = newYaw;
        clampYaw();
        base.transform.localRotation = Quaternion.Euler(0f, _yaw, 0f);
    }

    public Vector3 getEyesPosition()
    {
        return aim.position;
    }

    public Vector3 GetEyesPositionWithoutLeaning()
    {
        return base.transform.TransformPoint(aim.localPosition);
    }

    public void updateScope(EGraphicQuality quality)
    {
        bool flag = false;
        int num = 0;
        switch (quality)
        {
        case EGraphicQuality.LOW:
            flag = true;
            num = 256;
            break;
        case EGraphicQuality.MEDIUM:
            flag = true;
            num = 512;
            break;
        case EGraphicQuality.HIGH:
            flag = true;
            num = 1024;
            break;
        case EGraphicQuality.ULTRA:
            flag = true;
            num = 2048;
            break;
        }
        if (flag)
        {
            if (scopeRenderTexture != null && scopeRenderTexture.width != num)
            {
                UnityEngine.Object.Destroy(scopeRenderTexture);
                scopeRenderTexture = null;
            }
            if (scopeRenderTexture == null)
            {
                GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB;
                GraphicsFormat depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
                scopeRenderTexture = new RenderTexture(num, num, colorFormat, depthStencilFormat);
                scopeRenderTexture.name = "Dual-Render Scope";
                scopeRenderTexture.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        else if (scopeRenderTexture != null)
        {
            UnityEngine.Object.Destroy(scopeRenderTexture);
            scopeRenderTexture = null;
        }
        scopeCamera.targetTexture = scopeRenderTexture;
        if (quality != 0)
        {
            if (scopeMaterial == null)
            {
                scopeMaterial = UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/Scope"));
            }
            scopeMaterial.SetTexture("_MainTex", scopeCamera.targetTexture);
            scopeMaterial.SetTexture("_EmissionMap", scopeCamera.targetTexture);
        }
        scopeCamera.enabled = isScopeActive && scopeCamera.targetTexture != null && scopeVision == ELightingVision.NONE;
        if (base.player.equipment.asset != null && base.player.equipment.asset.type == EItemType.GUN)
        {
            base.player.equipment.useable.updateState(base.player.equipment.state);
        }
    }

    public void enableScope(float zoom, ItemSightAsset sightAsset)
    {
        scopeCameraZoomFactor = zoom;
        _isScopeActive = true;
        scopeVision = sightAsset.vision;
        scopeNightvisionColor = sightAsset.nightvisionColor;
        scopeNightvisionFogIntensity = sightAsset.nightvisionFogIntensity;
        scopeCamera.enabled = scopeCamera.targetTexture != null && scopeVision == ELightingVision.NONE;
        scopeCamera.GetComponent<GrayscaleEffect>().blend = ((scopeVision == ELightingVision.CIVILIAN) ? 1f : 0f);
    }

    public void disableScope()
    {
        scopeCamera.enabled = false;
        _isScopeActive = false;
        scopeVision = ELightingVision.NONE;
    }

    public void enableOverlay()
    {
        if (scopeVision != 0 && !(scopeCamera.targetTexture != null))
        {
            enableVision();
            isOverlayActive = true;
        }
    }

    [Obsolete("this was never supported server-side")]
    public void setPerspective(EPlayerPerspective newPerspective)
    {
        throw new NotSupportedException("this was never supported server-side");
    }

    private void setActivePerspective(EPlayerPerspective newPerspective)
    {
        _perspective = newPerspective;
        if (perspective == EPlayerPerspective.FIRST)
        {
            MainCamera.instance.transform.parent = base.player.first;
            MainCamera.instance.transform.localPosition = Vector3.up * eyes;
            isOrbiting = false;
            isTracking = false;
            isLocking = false;
            isFocusing = false;
            if (PlayerWorkzoneUI.active)
            {
                PlayerWorkzoneUI.close();
                PlayerLifeUI.open();
            }
        }
        else
        {
            MainCamera.instance.transform.parent = base.player.transform;
        }
        onPerspectiveUpdated?.Invoke(perspective);
        UnturnedPostProcess.instance.notifyPerspectiveChanged();
    }

    private void enableVision()
    {
        tempVision = LevelLighting.vision;
        tempNightvisionColor = LevelLighting.nightvisionColor;
        tempNightvisionFogIntensity = LevelLighting.nightvisionFogIntensity;
        LevelLighting.vision = scopeVision;
        LevelLighting.nightvisionColor = scopeNightvisionColor;
        LevelLighting.nightvisionFogIntensity = scopeNightvisionFogIntensity;
        LevelLighting.updateLighting();
        LevelLighting.updateLocal();
        PlayerLifeUI.updateGrayscale();
    }

    public void disableOverlay()
    {
        if (perspective != 0)
        {
            tempVision = ELightingVision.NONE;
        }
        if (isOverlayActive)
        {
            disableVision();
            isOverlayActive = false;
        }
    }

    private void disableVision()
    {
        LevelLighting.vision = tempVision;
        LevelLighting.nightvisionColor = tempNightvisionColor;
        LevelLighting.nightvisionFogIntensity = tempNightvisionFogIntensity;
        LevelLighting.updateLighting();
        LevelLighting.updateLocal();
        PlayerLifeUI.updateGrayscale();
        tempVision = ELightingVision.NONE;
    }

    public void enableZoom(float zoom)
    {
        mainCameraZoomFactor = zoom;
        isZoomed = true;
    }

    public void disableZoom()
    {
        mainCameraZoomFactor = 0f;
        isZoomed = false;
    }

    public void updateRot()
    {
        if (pitch < 0f)
        {
            angle = 0;
        }
        else if (pitch > 180f)
        {
            angle = 180;
        }
        else
        {
            angle = (byte)pitch;
        }
        rot = MeasurementTool.angleToByte(yaw);
    }

    public void updateLook()
    {
        _pitch = 90f;
        _yaw = base.transform.localRotation.eulerAngles.y;
        updateRot();
        if (base.channel.IsLocalPlayer && perspective == EPlayerPerspective.FIRST)
        {
            MainCamera.instance.transform.localRotation = Quaternion.Euler(pitch - 90f, 0f, 0f);
            MainCamera.instance.transform.localPosition = Vector3.up * eyes;
        }
    }

    public void recoil(float x, float y, float h, float v)
    {
        _yaw += x;
        _pitch -= y;
        recoil_x += x * h;
        recoil_y += y * v;
    }

    public void simulate(float look_x, float look_y, float delta)
    {
        _pitch = look_y;
        _yaw = look_x;
        clampPitch();
        clampYaw();
        updateRot();
        if (base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING)
        {
            base.transform.localRotation = Quaternion.identity;
        }
        else
        {
            base.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
        if (base.player.movement.getVehicle() != null && base.player.movement.getVehicle().passengers[base.player.movement.getSeat()].turret != null)
        {
            Passenger passenger = base.player.movement.getVehicle().passengers[base.player.movement.getSeat()];
            if (passenger.turretYaw != null)
            {
                passenger.turretYaw.localRotation = passenger.rotationYaw * Quaternion.Euler(0f, yaw, 0f);
            }
            if (passenger.turretPitch != null)
            {
                passenger.turretPitch.localRotation = passenger.rotationPitch * Quaternion.Euler(pitch - 90f, 0f, 0f);
            }
        }
        updateAim(delta);
    }

    private void clampPitch()
    {
        Passenger vehicleSeat = base.player.movement.getVehicleSeat();
        float min;
        float max;
        if (vehicleSeat != null)
        {
            if (vehicleSeat.turret != null)
            {
                min = vehicleSeat.turret.pitchMin;
                max = vehicleSeat.turret.pitchMax;
            }
            else
            {
                min = MIN_ANGLE_SIT;
                max = MAX_ANGLE_SIT;
            }
        }
        else if (base.player.stance.stance == EPlayerStance.STAND || base.player.stance.stance == EPlayerStance.SPRINT)
        {
            min = MIN_ANGLE_STAND;
            max = MAX_ANGLE_STAND;
        }
        else if (base.player.stance.stance == EPlayerStance.CLIMB)
        {
            min = MIN_ANGLE_CLIMB;
            max = MAX_ANGLE_CLIMB;
        }
        else if (base.player.stance.stance == EPlayerStance.SWIM)
        {
            min = MIN_ANGLE_SWIM;
            max = MAX_ANGLE_SWIM;
        }
        else if (base.player.stance.stance == EPlayerStance.CROUCH)
        {
            min = MIN_ANGLE_CROUCH;
            max = MAX_ANGLE_CROUCH;
        }
        else if (base.player.stance.stance == EPlayerStance.PRONE)
        {
            min = MIN_ANGLE_PRONE;
            max = MAX_ANGLE_PRONE;
        }
        else
        {
            min = 0f;
            max = 180f;
        }
        _pitch = Mathf.Clamp(_pitch, min, max);
    }

    private void clampYaw()
    {
        _yaw %= 360f;
        Passenger vehicleSeat = base.player.movement.getVehicleSeat();
        if (vehicleSeat != null)
        {
            float min;
            float max;
            if (vehicleSeat.turret != null)
            {
                min = vehicleSeat.turret.yawMin;
                max = vehicleSeat.turret.yawMax;
            }
            else if (base.player.stance.stance == EPlayerStance.DRIVING)
            {
                min = -160f;
                max = 160f;
            }
            else
            {
                min = -90f;
                max = 90f;
            }
            _yaw = Mathf.Clamp(_yaw, min, max);
        }
    }

    public void updateAim(float delta)
    {
        if (base.player.movement.getVehicle() != null && base.player.movement.getVehicle().passengers[base.player.movement.getSeat()].turret != null && base.player.movement.getVehicle().passengers[base.player.movement.getSeat()].turret.useAimCamera)
        {
            Passenger passenger = base.player.movement.getVehicle().passengers[base.player.movement.getSeat()];
            if (passenger.turretAim != null)
            {
                aim.position = passenger.turretAim.position;
                aim.rotation = passenger.turretAim.rotation;
            }
            return;
        }
        aim.localPosition = Vector3.Lerp(aim.localPosition, Vector3.up * heightLook, 4f * delta);
        if (base.player.stance.stance == EPlayerStance.SITTING || base.player.stance.stance == EPlayerStance.DRIVING)
        {
            aim.parent.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
        else if (base.player.animator.leanObstructed)
        {
            aim.parent.localRotation = Quaternion.identity;
        }
        else
        {
            aim.parent.localRotation = Quaternion.Lerp(aim.parent.localRotation, Quaternion.Euler(0f, 0f, (float)base.player.animator.lean * HumanAnimator.LEAN), 4f * delta);
        }
        aim.localRotation = Quaternion.Euler(pitch - 90f + base.player.animator.scopeSway.x, base.player.animator.scopeSway.y, 0f);
    }

    internal void FlinchFromDamage(byte damageAmount, Vector3 worldDirection)
    {
        Camera instance = MainCamera.instance;
        if (!(instance == null))
        {
            Vector3 normalized = Vector3.Cross(Vector3.up, worldDirection).normalized;
            Vector3 axis = instance.transform.InverseTransformDirection(normalized);
            float num = (float)Mathf.Min(damageAmount, 25) * 0.5f;
            float num2 = 1f - base.player.skills.mastery(1, 3) * 0.75f;
            flinchLocalRotation *= Quaternion.AngleAxis(num * num2, axis);
        }
    }

    internal void FlinchFromExplosion(Vector3 position, float radius, float magnitudeDegrees)
    {
        Camera instance = MainCamera.instance;
        if (!(instance == null))
        {
            Vector3 vector = instance.transform.position - position;
            float magnitude = vector.magnitude;
            if (!(magnitude <= 0f) && !(magnitude >= radius))
            {
                Vector3 vector2 = vector / magnitude;
                Vector3 normalized = Vector3.Cross(Vector3.up, vector2).normalized;
                Vector3 axis = instance.transform.InverseTransformDirection(normalized);
                float num = 1f - base.player.skills.mastery(1, 3) * 0.5f;
                float num2 = 1f - MathfEx.Square(magnitude / radius);
                magnitudeDegrees *= num * num2;
                targetExplosionLocalRotation.currentRotation *= Quaternion.AngleAxis(magnitudeDegrees, axis);
                base.player.animator.FlinchFromExplosion(vector2, magnitudeDegrees);
            }
        }
    }

    private void onVisionUpdated(bool isViewing)
    {
        if (isViewing)
        {
            yawInputMultiplier = (((double)UnityEngine.Random.value < 0.25) ? (-1f) : 1f);
            pitchInputMultiplier = (((double)UnityEngine.Random.value < 0.25) ? (-1f) : 1f);
        }
        else
        {
            yawInputMultiplier = 1f;
            pitchInputMultiplier = 1f;
        }
    }

    private void onLifeUpdated(bool isDead)
    {
        if (isDead)
        {
            killcam = base.transform.rotation.eulerAngles.y;
        }
    }

    private void onSeated(bool isDriver, bool inVehicle, bool wasVehicle, InteractableVehicle oldVehicle, InteractableVehicle newVehicle)
    {
        if (!wasVehicle)
        {
            _orbitPitch = 22.5f;
            _orbitYaw = 0f;
        }
        if (Provider.cameraMode == ECameraMode.VEHICLE && perspective == EPlayerPerspective.THIRD && !isDriver)
        {
            setActivePerspective(EPlayerPerspective.FIRST);
        }
    }

    [Obsolete]
    public void tellFreecamAllowed(CSteamID senderId, bool isAllowed)
    {
        ReceiveFreecamAllowed(isAllowed);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellFreecamAllowed")]
    public void ReceiveFreecamAllowed(bool isAllowed)
    {
        allowFreecamWithoutAdmin = isAllowed;
        if (!canUseFreecam && isCam)
        {
            isOrbiting = false;
            isTracking = false;
            isLocking = false;
            isFocusing = false;
        }
    }

    public void sendFreecamAllowed(bool isAllowed)
    {
        allowFreecamWithoutAdmin = isAllowed;
        SendFreecamAllowed.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), isAllowed);
    }

    [Obsolete]
    public void tellWorkzoneAllowed(CSteamID senderId, bool isAllowed)
    {
        ReceiveWorkzoneAllowed(isAllowed);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellWorkzoneAllowed")]
    public void ReceiveWorkzoneAllowed(bool isAllowed)
    {
        allowWorkzoneWithoutAdmin = isAllowed;
        if (!canUseWorkzone && PlayerWorkzoneUI.active)
        {
            PlayerWorkzoneUI.close();
            PlayerLifeUI.open();
        }
    }

    public void sendWorkzoneAllowed(bool isAllowed)
    {
        allowWorkzoneWithoutAdmin = isAllowed;
        SendWorkzoneAllowed.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), isAllowed);
    }

    [Obsolete]
    public void tellSpecStatsAllowed(CSteamID senderId, bool isAllowed)
    {
        ReceiveSpecStatsAllowed(isAllowed);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSpecStatsAllowed")]
    public void ReceiveSpecStatsAllowed(bool isAllowed)
    {
        allowSpecStatsWithoutAdmin = isAllowed;
        if (!canUseSpecStats)
        {
            areSpecStatsVisible = false;
        }
    }

    public void sendSpecStatsAllowed(bool isAllowed)
    {
        allowSpecStatsWithoutAdmin = isAllowed;
        SendSpecStatsAllowed.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), isAllowed);
    }

    private Vector3 sphereCastCamera(Vector3 origin, Vector3 direction, float length, int layerMask)
    {
        int num = Physics.SphereCastNonAlloc(new Ray(origin, direction), 0.39f, sweepHits, length, layerMask, QueryTriggerInteraction.Ignore);
        float num2 = length;
        for (int i = 0; i < num; i++)
        {
            num2 = Mathf.Min(num2, sweepHits[i].distance);
        }
        return origin + direction * num2;
    }

    private void Update()
    {
        if (base.channel.IsLocalPlayer)
        {
            if (InputEx.GetKey(KeyCode.LeftShift))
            {
                if (canUseFreecam)
                {
                    if (InputEx.GetKeyDown(KeyCode.F1))
                    {
                        isOrbiting = !isOrbiting;
                        if (isOrbiting && !isTracking && !isLocking && !isFocusing)
                        {
                            isTracking = true;
                        }
                    }
                    if (InputEx.GetKeyDown(KeyCode.F2))
                    {
                        isTracking = !isTracking;
                        if (isTracking)
                        {
                            isLocking = false;
                            isFocusing = false;
                        }
                    }
                    if (InputEx.GetKeyDown(KeyCode.F3))
                    {
                        isLocking = !isLocking;
                        if (isLocking)
                        {
                            isTracking = false;
                            isFocusing = false;
                            lockPosition = base.player.first.position;
                        }
                    }
                    if (InputEx.GetKeyDown(KeyCode.F4))
                    {
                        isFocusing = !isFocusing;
                        if (isFocusing)
                        {
                            isTracking = false;
                            isLocking = false;
                            lockPosition = base.player.first.position;
                        }
                    }
                    if (InputEx.GetKeyDown(KeyCode.F5))
                    {
                        isSmoothing = !isSmoothing;
                    }
                }
                if (InputEx.GetKeyDown(KeyCode.F6))
                {
                    if (PlayerWorkzoneUI.active)
                    {
                        PlayerWorkzoneUI.close();
                        PlayerLifeUI.open();
                    }
                    else if (canUseWorkzone && perspective == EPlayerPerspective.THIRD)
                    {
                        PlayerWorkzoneUI.open();
                        PlayerLifeUI.close();
                    }
                }
                if (InputEx.GetKeyDown(KeyCode.F7))
                {
                    if (areSpecStatsVisible)
                    {
                        areSpecStatsVisible = false;
                    }
                    else if (canUseSpecStats)
                    {
                        areSpecStatsVisible = true;
                    }
                }
            }
            float num = heightLook;
            eyes = Mathf.Lerp(eyes, num, 4f * Time.deltaTime);
            if (base.player.movement.controller != null)
            {
                float min = 0.39499998f;
                float max = base.player.movement.controller.height - 0.39f - 0.005f;
                thirdPersonEyeHeight = Mathf.Lerp(thirdPersonEyeHeight, Mathf.Clamp(num, min, max), 4f * Time.deltaTime);
            }
            Camera instance = MainCamera.instance;
            if (base.player.life.IsAlive && !PlayerUI.window.showCursor)
            {
                if (InputEx.GetKeyDown(ControlsSettings.perspective) && (Provider.cameraMode == ECameraMode.BOTH || (Provider.cameraMode == ECameraMode.VEHICLE && base.player.stance.stance == EPlayerStance.DRIVING)))
                {
                    EPlayerPerspective activePerspective = ((perspective == EPlayerPerspective.FIRST) ? EPlayerPerspective.THIRD : EPlayerPerspective.FIRST);
                    setActivePerspective(activePerspective);
                }
                if (isCam)
                {
                    if (perspective != EPlayerPerspective.THIRD)
                    {
                        setActivePerspective(EPlayerPerspective.THIRD);
                    }
                }
                else if ((Provider.cameraMode == ECameraMode.FIRST || (Provider.cameraMode == ECameraMode.VEHICLE && base.player.stance.stance != EPlayerStance.DRIVING)) && perspective != 0)
                {
                    setActivePerspective(EPlayerPerspective.FIRST);
                }
            }
            float zoomBaseFieldOfView = OptionsSettings.GetZoomBaseFieldOfView();
            if (isCam)
            {
                instance.fieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
            }
            else
            {
                instance.fieldOfView = Mathf.Lerp(instance.fieldOfView, (mainCameraZoomFactor > 0f) ? (zoomBaseFieldOfView / mainCameraZoomFactor) : (OptionsSettings.DesiredVerticalFieldOfView + (float)((base.player.stance.stance == EPlayerStance.SPRINT) ? 10 : 0)), 8f * Time.deltaTime);
            }
            if (isScopeActive && scopeCamera != null && scopeCameraZoomFactor > 0f)
            {
                scopeCamera.fieldOfView = zoomBaseFieldOfView / scopeCameraZoomFactor;
            }
            _look_x = 0f;
            _look_y = 0f;
            if (PlayerUI.window.isCursorLocked && !isIgnoringInput)
            {
                if (isOrbiting)
                {
                    if (!base.player.workzone.isBuilding || InputEx.GetKey(ControlsSettings.secondary))
                    {
                        _orbitYaw += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_x") * yawInputMultiplier;
                        if (ControlsSettings.invert)
                        {
                            _orbitPitch += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
                        }
                        else
                        {
                            _orbitPitch -= ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
                        }
                    }
                }
                else
                {
                    if (perspective == EPlayerPerspective.FIRST || isTracking || isLocking || isFocusing)
                    {
                        _look_x = ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_x") * yawInputMultiplier;
                        _look_y = ControlsSettings.mouseAimSensitivity * (0f - Input.GetAxis("mouse_y")) * pitchInputMultiplier;
                    }
                    if (InputEx.GetKey(ControlsSettings.rollLeft))
                    {
                        _look_x = ((base.player.movement.getVehicle() != null) ? (0f - base.player.movement.getVehicle().asset.airTurnResponsiveness) : (-1f));
                    }
                    else if (InputEx.GetKey(ControlsSettings.rollRight))
                    {
                        _look_x = ((base.player.movement.getVehicle() != null) ? base.player.movement.getVehicle().asset.airTurnResponsiveness : 1f);
                    }
                    if (InputEx.GetKey(ControlsSettings.pitchUp))
                    {
                        _look_y = ((base.player.movement.getVehicle() != null) ? (0f - base.player.movement.getVehicle().asset.airTurnResponsiveness) : (-1f));
                    }
                    else if (InputEx.GetKey(ControlsSettings.pitchDown))
                    {
                        _look_y = ((base.player.movement.getVehicle() != null) ? base.player.movement.getVehicle().asset.airTurnResponsiveness : 1f);
                    }
                    if (ControlsSettings.invertFlight)
                    {
                        _look_y *= -1f;
                    }
                    float num2 = 1f;
                    switch (ControlsSettings.sensitivityScalingMode)
                    {
                    case ESensitivityScalingMode.ProjectionRatio:
                    {
                        float num3 = ((shouldUseZoomFactorForSensitivity && isScopeActive && perspective == EPlayerPerspective.FIRST && scopeCameraZoomFactor > 0f) ? scopeCamera.fieldOfView : instance.fieldOfView);
                        float f = MathF.PI / 180f * num3 * 0.5f;
                        float f2 = MathF.PI / 180f * OptionsSettings.DesiredVerticalFieldOfView * 0.5f;
                        float projectionRatioCoefficient = ControlsSettings.projectionRatioCoefficient;
                        num2 = Mathf.Atan(projectionRatioCoefficient * Mathf.Tan(f)) / Mathf.Atan(projectionRatioCoefficient * Mathf.Tan(f2));
                        break;
                    }
                    case ESensitivityScalingMode.ZoomFactor:
                    case ESensitivityScalingMode.Legacy:
                        if (shouldUseZoomFactorForSensitivity)
                        {
                            if (isScopeActive && perspective == EPlayerPerspective.FIRST && scopeCameraZoomFactor > 0f)
                            {
                                num2 = 1f / scopeCameraZoomFactor;
                            }
                            else if (mainCameraZoomFactor > 0f)
                            {
                                num2 = 1f / mainCameraZoomFactor;
                            }
                        }
                        break;
                    }
                    if (base.player.movement.getVehicle() != null && perspective == EPlayerPerspective.THIRD)
                    {
                        _orbitYaw += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_x") * yawInputMultiplier;
                        _orbitYaw = orbitYaw % 360f;
                    }
                    else if (base.player.movement.getVehicle() == null || !base.player.movement.getVehicle().asset.hasLockMouse || !base.player.movement.getVehicle().isDriver)
                    {
                        _yaw += ControlsSettings.mouseAimSensitivity * num2 * Input.GetAxis("mouse_x") * yawInputMultiplier;
                    }
                    if (base.player.movement.getVehicle() != null && perspective == EPlayerPerspective.THIRD)
                    {
                        if (ControlsSettings.invert)
                        {
                            _orbitPitch += ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
                        }
                        else
                        {
                            _orbitPitch -= ControlsSettings.mouseAimSensitivity * Input.GetAxis("mouse_y") * pitchInputMultiplier;
                        }
                    }
                    else if (base.player.movement.getVehicle() == null || !base.player.movement.getVehicle().asset.hasLockMouse || !base.player.movement.getVehicle().isDriver)
                    {
                        if (ControlsSettings.invert)
                        {
                            _pitch += ControlsSettings.mouseAimSensitivity * num2 * Input.GetAxis("mouse_y") * pitchInputMultiplier;
                        }
                        else
                        {
                            _pitch -= ControlsSettings.mouseAimSensitivity * num2 * Input.GetAxis("mouse_y") * pitchInputMultiplier;
                        }
                    }
                }
            }
            if (float.IsInfinity(yaw) || float.IsNaN(yaw))
            {
                _yaw = 0f;
            }
            if (float.IsInfinity(pitch) || float.IsNaN(pitch))
            {
                _pitch = 0f;
            }
            if (float.IsInfinity(orbitYaw) || float.IsNaN(orbitYaw))
            {
                _orbitYaw = 0f;
            }
            if (float.IsInfinity(orbitPitch) || float.IsNaN(orbitPitch))
            {
                _orbitPitch = 0f;
            }
            float num4 = Mathf.Lerp(recoil_x, 0f, 4f * Time.deltaTime);
            float num5 = num4 - recoil_x;
            recoil_x = num4;
            float num6 = Mathf.Lerp(recoil_y, 0f, 4f * Time.deltaTime);
            float num7 = num6 - recoil_y;
            recoil_y = num6;
            _yaw += num5;
            _pitch -= num7;
            flinchLocalRotation = Quaternion.Lerp(flinchLocalRotation, Quaternion.identity, 4f * Time.deltaTime);
            smoothedExplosionLocalRotation = Quaternion.Lerp(smoothedExplosionLocalRotation, targetExplosionLocalRotation.currentRotation, explosionSmoothingSpeed * Time.deltaTime);
            targetExplosionLocalRotation.Update(Time.deltaTime);
            clampPitch();
            clampYaw();
            if (orbitPitch > 90f)
            {
                _orbitPitch = 90f;
            }
            else if (orbitPitch < -90f)
            {
                _orbitPitch = -90f;
            }
            _characterYaw = Mathf.Lerp(_characterYaw, characterYaw + 180f, 4f * Time.deltaTime);
            characterCamera.transform.rotation = Quaternion.Euler(20f, _characterYaw, 0f);
            characterCamera.transform.position = base.player.character.position - characterCamera.transform.forward * 3.5f + Vector3.up * characterHeight;
            if (base.player.life.isDead)
            {
                killcam += -16f * Time.deltaTime;
                instance.transform.rotation = Quaternion.Lerp(instance.transform.rotation, Quaternion.Euler(32f, killcam, 0f), 2f * Time.deltaTime);
            }
            else
            {
                if ((base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING) && perspective == EPlayerPerspective.THIRD)
                {
                    instance.transform.localRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
                }
                else if (base.player.stance.stance == EPlayerStance.DRIVING)
                {
                    instance.transform.localRotation = Quaternion.Euler(pitch - 90f, yaw / 10f, 0f);
                    instance.transform.Rotate(base.transform.up, yaw, Space.World);
                }
                else if (base.player.stance.stance == EPlayerStance.SITTING)
                {
                    instance.transform.localRotation = Quaternion.Euler(pitch - 90f + base.player.animator.scopeSway.x, base.player.animator.scopeSway.y, 0f);
                    instance.transform.Rotate(base.transform.up, yaw, Space.World);
                }
                else
                {
                    if (perspective == EPlayerPerspective.FIRST)
                    {
                        instance.transform.localRotation = smoothedExplosionLocalRotation * flinchLocalRotation * Quaternion.Euler(pitch - 90f + base.player.animator.scopeSway.x, base.player.animator.scopeSway.y, 0f);
                    }
                    else
                    {
                        instance.transform.localRotation = smoothedExplosionLocalRotation * flinchLocalRotation * Quaternion.Euler(pitch - 90f + base.player.animator.scopeSway.x, base.player.animator.shoulder * -5f + base.player.animator.scopeSway.y, 0f);
                    }
                    base.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                }
                if (isCam)
                {
                    if (isFocusing)
                    {
                        if (isSmoothing)
                        {
                            smoothRotation = Quaternion.Lerp(smoothRotation, Quaternion.LookRotation(base.player.first.position + Vector3.up - (lockPosition + orbitPosition)), 4f * Time.deltaTime);
                            instance.transform.rotation = smoothRotation;
                        }
                        else
                        {
                            instance.transform.rotation = Quaternion.LookRotation(base.player.first.position + Vector3.up - (lockPosition + orbitPosition));
                        }
                    }
                    else if (isSmoothing)
                    {
                        smoothRotation = Quaternion.Lerp(smoothRotation, Quaternion.Euler(orbitPitch, orbitYaw, 0f), 4f * Time.deltaTime);
                        instance.transform.rotation = smoothRotation;
                    }
                    else
                    {
                        instance.transform.rotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
                    }
                }
            }
            if (base.player.life.isDead)
            {
                Vector3 origin = base.player.first.position + Vector3.up;
                Vector3 direction = -instance.transform.forward;
                float length = 4f;
                instance.transform.position = sphereCastCamera(origin, direction, length, RayMasks.BLOCK_KILLCAM);
            }
            else
            {
                if (isCam)
                {
                    if (isLocking || isFocusing)
                    {
                        instance.transform.position = lockPosition + orbitPosition;
                    }
                    else if (isOrbiting || isTracking)
                    {
                        if (isSmoothing)
                        {
                            smoothPosition = Vector3.Lerp(smoothPosition, orbitPosition, 4f * Time.deltaTime);
                            instance.transform.position = base.player.first.position + smoothPosition;
                        }
                        else
                        {
                            instance.transform.position = base.player.first.position + orbitPosition;
                        }
                    }
                }
                else if ((base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING) && perspective == EPlayerPerspective.THIRD)
                {
                    Vector3 origin2 = base.player.first.transform.position + Vector3.up * eyes;
                    Transform transform = base.player.movement.getVehicle().transform.Find("Camera_Focus");
                    if (transform != null)
                    {
                        origin2 = transform.position;
                    }
                    float length2 = base.player.movement.getVehicle().asset.camFollowDistance + Mathf.Abs(base.player.movement.getVehicle().spedometer) * 0.1f;
                    Vector3 direction2 = -instance.transform.forward;
                    instance.transform.position = sphereCastCamera(origin2, direction2, length2, RayMasks.BLOCK_VEHICLECAM);
                }
                else if (base.player.stance.stance == EPlayerStance.DRIVING)
                {
                    float num8 = base.player.movement.getVehicle().asset.camDriverOffset + base.player.movement.getVehicle().asset.camPassengerOffset;
                    if (yaw > 0f)
                    {
                        instance.transform.localPosition = Vector3.Lerp(instance.transform.localPosition, Vector3.up * (heightLook + num8) - Vector3.left * yaw / 360f, 4f * Time.deltaTime);
                    }
                    else
                    {
                        instance.transform.localPosition = Vector3.Lerp(instance.transform.localPosition, Vector3.up * (heightLook + num8) - Vector3.left * yaw / 240f, 4f * Time.deltaTime);
                    }
                }
                else if (perspective == EPlayerPerspective.FIRST)
                {
                    float num9;
                    if (base.player.stance.stance == EPlayerStance.SITTING && base.player.movement.getVehicle() != null)
                    {
                        num9 = base.player.movement.getVehicle().asset.camPassengerOffset;
                    }
                    else
                    {
                        num9 = 0f;
                        Vector3 origin3 = base.player.first.position + new Vector3(0f, HEIGHT_LOOK_PRONE - 0.25f, 0f);
                        Vector3 up = Vector3.up;
                        float maxDistance = PlayerMovement.HEIGHT_STAND - HEIGHT_LOOK_PRONE - 0.25f;
                        if (Physics.SphereCast(origin3, 0.25f, up, out var hitInfo, maxDistance, RayMasks.BLOCK_PLAYERCAM_1P, QueryTriggerInteraction.Ignore))
                        {
                            float b = hitInfo.point.y - base.player.first.position.y - 0.25f;
                            eyes = Mathf.Min(eyes, b);
                        }
                    }
                    instance.transform.localPosition = new Vector3(0f, eyes + num9, 0f);
                }
                else
                {
                    Vector3 direction3 = ((!Provider.modeConfigData.Gameplay.Allow_Shoulder_Camera) ? (instance.transform.forward * -1.5f + instance.transform.up * 0.5f + instance.transform.right * base.player.animator.shoulder2 * 0.5f) : (instance.transform.forward * -1.5f + instance.transform.up * 0.25f + instance.transform.right * base.player.animator.shoulder * 1f));
                    direction3.Normalize();
                    Vector3 origin4 = base.player.first.position + new Vector3(0f, thirdPersonEyeHeight, 0f);
                    float length3 = 2f;
                    instance.transform.position = sphereCastCamera(origin4, direction3, length3, RayMasks.BLOCK_PLAYERCAM);
                }
                characterHeight = Mathf.Lerp(characterHeight, heightCamera, 4f * Time.deltaTime);
            }
            if (base.player.movement.getVehicle() != null && base.player.movement.getVehicle().asset.engine == EEngine.PLANE && base.player.movement.getVehicle().spedometer > 16f)
            {
                LevelLighting.updateLocal(instance.transform.position, Mathf.Lerp(0f, 1f, (base.player.movement.getVehicle().spedometer - 16f) / 8f), base.player.movement.effectNode);
            }
            else if (base.player.movement.getVehicle() != null && (base.player.movement.getVehicle().asset.engine == EEngine.HELICOPTER || base.player.movement.getVehicle().asset.engine == EEngine.BLIMP) && base.player.movement.getVehicle().spedometer > 4f)
            {
                LevelLighting.updateLocal(instance.transform.position, Mathf.Lerp(0f, 1f, (base.player.movement.getVehicle().spedometer - 8f) / 8f), base.player.movement.effectNode);
            }
            else
            {
                LevelLighting.updateLocal(instance.transform.position, 0f, base.player.movement.effectNode);
            }
            base.player.animator.viewmodelParentTransform.rotation = instance.transform.rotation;
            if (isScopeActive && scopeCamera.targetTexture != null && scopeVision != 0)
            {
                enableVision();
                scopeCamera.Render();
                disableVision();
            }
            if (base.player.movement.getVehicle() != null && base.player.movement.getVehicle().passengers[base.player.movement.getSeat()].turret != null)
            {
                Passenger passenger = base.player.movement.getVehicle().passengers[base.player.movement.getSeat()];
                if (passenger.turretYaw != null)
                {
                    passenger.turretYaw.localRotation = passenger.rotationYaw * Quaternion.Euler(0f, yaw, 0f);
                }
                if (passenger.turretPitch != null)
                {
                    passenger.turretPitch.localRotation = passenger.rotationPitch * Quaternion.Euler(pitch - 90f, 0f, 0f);
                }
                if (perspective == EPlayerPerspective.FIRST && base.player.movement.getVehicle().passengers[base.player.movement.getSeat()].turret.useAimCamera)
                {
                    instance.transform.position = passenger.turretAim.position;
                    instance.transform.rotation = passenger.turretAim.rotation;
                }
            }
            if (FoliageSettings.drawFocus)
            {
                if (isZoomed || (isScopeActive && scopeCamera.targetTexture != null))
                {
                    FoliageSystem.isFocused = true;
                    if (Physics.Raycast(MainCamera.instance.transform.position, MainCamera.instance.transform.forward, out var hitInfo2, FoliageSettings.focusDistance, RayMasks.FOLIAGE_FOCUS))
                    {
                        FoliageSystem.focusPosition = hitInfo2.point;
                        if (isScopeActive && scopeCamera.targetTexture != null)
                        {
                            FoliageSystem.focusCamera = scopeCamera;
                        }
                        else
                        {
                            FoliageSystem.focusCamera = MainCamera.instance;
                        }
                    }
                }
                else
                {
                    FoliageSystem.isFocused = false;
                }
            }
        }
        else if (!Provider.isServer)
        {
            if (base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING)
            {
                base.transform.localRotation = Quaternion.identity;
            }
            else
            {
                _pitch = base.player.movement.snapshot.pitch;
                _yaw = base.player.movement.snapshot.yaw;
                base.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }
            if (base.player.movement.getVehicle() != null && base.player.movement.getVehicle().passengers[base.player.movement.getSeat()].turret != null)
            {
                Passenger passenger2 = base.player.movement.getVehicle().passengers[base.player.movement.getSeat()];
                if (passenger2.turretYaw != null)
                {
                    passenger2.turretYaw.localRotation = passenger2.rotationYaw * Quaternion.Euler(0f, base.player.movement.snapshot.yaw, 0f);
                }
                if (passenger2.turretPitch != null)
                {
                    passenger2.turretPitch.localRotation = passenger2.rotationPitch * Quaternion.Euler(base.player.movement.snapshot.pitch - 90f, 0f, 0f);
                }
            }
        }
        if (!Dedicator.IsDedicatedServer)
        {
            updateAim(Time.deltaTime);
        }
    }

    internal void InitializePlayer()
    {
        _aim = base.transform.Find("Aim").Find("Fire");
        updateLook();
        yawInputMultiplier = 1f;
        pitchInputMultiplier = 1f;
        if (base.channel.IsLocalPlayer)
        {
            if (Provider.cameraMode == ECameraMode.THIRD)
            {
                _perspective = EPlayerPerspective.THIRD;
                MainCamera.instance.transform.parent = base.player.transform;
            }
            else
            {
                _perspective = EPlayerPerspective.FIRST;
            }
            MainCamera.instance.fieldOfView = OptionsSettings.DesiredVerticalFieldOfView;
            targetExplosionLocalRotation.currentRotation = Quaternion.identity;
            targetExplosionLocalRotation.targetRotation = Quaternion.identity;
            characterHeight = 0f;
            _characterYaw = 180f;
            characterYaw = 0f;
            if (base.player.character != null)
            {
                _characterCamera = base.player.character.Find("Camera").GetComponent<Camera>();
                _characterCamera.eventMask = 0;
            }
            _scopeCamera = MainCamera.instance.transform.Find("Scope").GetComponent<Camera>();
            scopeCamera.layerCullDistances = MainCamera.instance.layerCullDistances;
            scopeCamera.layerCullSpherical = MainCamera.instance.layerCullSpherical;
            scopeCamera.fieldOfView = 10f;
            scopeCamera.eventMask = 0;
            UnturnedPostProcess.instance.setScopeCamera(scopeCamera);
            LevelLighting.updateLighting();
            PlayerLife life = base.player.life;
            life.onVisionUpdated = (VisionUpdated)Delegate.Combine(life.onVisionUpdated, new VisionUpdated(onVisionUpdated));
            PlayerLife life2 = base.player.life;
            life2.onLifeUpdated = (LifeUpdated)Delegate.Combine(life2.onLifeUpdated, new LifeUpdated(onLifeUpdated));
            PlayerMovement movement = base.player.movement;
            movement.onSeated = (Seated)Delegate.Combine(movement.onSeated, new Seated(onSeated));
        }
    }

    private void OnDestroy()
    {
        if (scopeRenderTexture != null)
        {
            UnityEngine.Object.Destroy(scopeRenderTexture);
            scopeRenderTexture = null;
        }
        if (scopeMaterial != null)
        {
            UnityEngine.Object.Destroy(scopeMaterial);
            scopeMaterial = null;
        }
    }
}
