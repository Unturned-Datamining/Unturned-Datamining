using SDG.NetTransport;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class InteractableObjectNPC : InteractableObject
{
    protected ObjectNPCAsset _npcAsset;

    public bool isLookingAtPlayer;

    private bool isInit;

    private Animation anim;

    private HumanAnimator humanAnim;

    private HumanClothes clothes;

    private Transform skull;

    private bool itemHasEquipAnimation;

    private bool itemHasSafetyAnimation;

    private bool itemHasInspectAnimation;

    private bool hasPlayedEquipAnimation;

    private bool isPlayingSafetyAnimation;

    private string stanceIdle;

    private string stanceActive;

    private float lastIdle;

    private float idleDelay;

    private float headBlend;

    private Quaternion headRotation;

    public ObjectNPCAsset npcAsset => _npcAsset;

    public NetId GetNpcNetId()
    {
        return NetIdRegistry.GetTransformNetId(base.transform);
    }

    public static InteractableObjectNPC GetNpcFromObjectNetId(NetId netId)
    {
        return NetIdRegistry.GetTransform(netId, null)?.GetComponent<InteractableObjectNPC>();
    }

    internal void SetFaceOverride(byte? faceOverride)
    {
        byte b = (faceOverride.HasValue ? faceOverride.Value : _npcAsset.face);
        if (clothes.face != b)
        {
            clothes.face = b;
            clothes.apply();
        }
    }

    internal void OnStoppedTalkingWithLocalPlayer()
    {
        isLookingAtPlayer = false;
        SetFaceOverride(null);
    }

    private void updateStance()
    {
        stanceActive = null;
        if (npcAsset.pose == ENPCPose.SIT)
        {
            if (Random.value < 0.5f)
            {
                stanceIdle = "Idle_Sit";
            }
            else
            {
                stanceIdle = "Idle_Drive";
            }
        }
        else if (npcAsset.pose == ENPCPose.CROUCH)
        {
            stanceIdle = "Idle_Crouch";
        }
        else if (npcAsset.pose == ENPCPose.PRONE)
        {
            stanceIdle = "Idle_Prone";
        }
        else if (npcAsset.pose == ENPCPose.UNDER_ARREST)
        {
            stanceIdle = "Gesture_Arrest";
        }
        else if (npcAsset.pose == ENPCPose.REST)
        {
            stanceIdle = "Gesture_Rest";
        }
        else if (npcAsset.pose == ENPCPose.SURRENDER)
        {
            stanceIdle = "Gesture_Surrender";
        }
        else if (itemHasEquipAnimation || npcAsset.pose == ENPCPose.ASLEEP)
        {
            stanceIdle = "Idle_Stand";
        }
        else if (Random.value < 0.5f)
        {
            stanceIdle = "Idle_Stand";
        }
        else
        {
            stanceIdle = "Idle_Hips";
        }
    }

    private void updateIdle()
    {
        lastIdle = Time.time;
        idleDelay = Random.Range(5f, 30f);
    }

    private void updateAnimation()
    {
        hasPlayedEquipAnimation = false;
        isPlayingSafetyAnimation = false;
        updateStance();
        updateIdle();
    }

    public override void updateState(Asset asset, byte[] state)
    {
        base.updateState(asset, state);
        if (isInit)
        {
            return;
        }
        isInit = true;
        _npcAsset = asset as ObjectNPCAsset;
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        Transform transform = base.transform.Find("Root");
        Transform transform2 = transform.Find("Skeleton");
        skull = transform2.Find("Spine").Find("Skull");
        anim = transform.GetComponent<Animation>();
        humanAnim = transform.GetComponent<HumanAnimator>();
        transform.localScale = new Vector3((!npcAsset.IsLeftHanded) ? 1 : (-1), 1f, 1f);
        ItemAsset itemAsset = null;
        Transform parent = transform2.Find("Spine").Find("Primary_Melee");
        Transform parent2 = transform2.Find("Spine").Find("Primary_Large_Gun");
        Transform parent3 = transform2.Find("Spine").Find("Primary_Small_Gun");
        Transform parent4 = transform2.Find("Right_Hip").Find("Right_Leg").Find("Secondary_Melee");
        Transform parent5 = transform2.Find("Right_Hip").Find("Right_Leg").Find("Secondary_Gun");
        Transform transform3 = transform2.Find("Spine");
        Transform transform4 = transform3.Find("Left_Shoulder").Find("Left_Arm").Find("Left_Hand")
            .Find("Left_Hook");
        Transform transform5 = transform3.Find("Right_Shoulder").Find("Right_Arm").Find("Right_Hand")
            .Find("Right_Hook");
        clothes = transform.GetComponent<HumanClothes>();
        clothes.canWearPro = true;
        NPCAssetOutfit currentOutfit = npcAsset.currentOutfit;
        if (!currentOutfit.shirtGuid.IsEmpty())
        {
            clothes.shirtGuid = currentOutfit.shirtGuid;
        }
        else
        {
            clothes.shirt = currentOutfit.shirt;
        }
        if (!currentOutfit.pantsGuid.IsEmpty())
        {
            clothes.pantsGuid = currentOutfit.pantsGuid;
        }
        else
        {
            clothes.pants = currentOutfit.pants;
        }
        if (!currentOutfit.hatGuid.IsEmpty())
        {
            clothes.hatGuid = currentOutfit.hatGuid;
        }
        else
        {
            clothes.hat = currentOutfit.hat;
        }
        if (!currentOutfit.backpackGuid.IsEmpty())
        {
            clothes.backpackGuid = currentOutfit.backpackGuid;
        }
        else
        {
            clothes.backpack = currentOutfit.backpack;
        }
        if (!currentOutfit.vestGuid.IsEmpty())
        {
            clothes.vestGuid = currentOutfit.vestGuid;
        }
        else
        {
            clothes.vest = currentOutfit.vest;
        }
        if (!currentOutfit.maskGuid.IsEmpty())
        {
            clothes.maskGuid = currentOutfit.maskGuid;
        }
        else
        {
            clothes.mask = currentOutfit.mask;
        }
        if (!currentOutfit.glassesGuid.IsEmpty())
        {
            clothes.glassesGuid = currentOutfit.glassesGuid;
        }
        else
        {
            clothes.glasses = currentOutfit.glasses;
        }
        clothes.face = npcAsset.face;
        clothes.hair = npcAsset.hair;
        clothes.beard = npcAsset.beard;
        clothes.skin = npcAsset.skin;
        clothes.color = npcAsset.color;
        clothes.apply();
        ItemAsset itemAsset2 = Assets.FindItemByGuidOrLegacyId<ItemAsset>(npcAsset.primaryWeaponGuid, npcAsset.primary);
        if (itemAsset2 != null)
        {
            GameObject prefabOverride = ((npcAsset.equipped == ESlotType.PRIMARY && itemAsset2.equipablePrefab != null) ? itemAsset2.equipablePrefab : itemAsset2.item);
            Material tempMaterial;
            Transform transform6 = ItemTool.InstantiateItem(100, itemAsset2.getState(), viewmodel: false, itemAsset2, null, shouldDestroyColliders: true, null, out tempMaterial, null, prefabOverride);
            if (npcAsset.equipped == ESlotType.PRIMARY)
            {
                Transform parent6 = itemAsset2.EquipableModelParent switch
                {
                    EEquipableModelParent.LeftHook => transform4, 
                    EEquipableModelParent.Spine => transform3, 
                    _ => transform5, 
                };
                transform6.transform.parent = parent6;
                itemAsset = itemAsset2;
            }
            else if (itemAsset2.type == EItemType.MELEE)
            {
                transform6.transform.parent = parent;
            }
            else if (itemAsset2.slot == ESlotType.PRIMARY)
            {
                transform6.transform.parent = parent2;
            }
            else
            {
                transform6.transform.parent = parent3;
            }
            transform6.localPosition = Vector3.zero;
            transform6.localRotation = Quaternion.Euler(0f, 0f, 90f);
            transform6.localScale = Vector3.one;
            transform6.DestroyRigidbody();
            Layerer.enemy(transform6);
        }
        ItemAsset itemAsset3 = Assets.FindItemByGuidOrLegacyId<ItemAsset>(npcAsset.secondaryWeaponGuid, npcAsset.secondary);
        if (itemAsset3 != null)
        {
            GameObject prefabOverride2 = ((npcAsset.equipped == ESlotType.SECONDARY && itemAsset3.equipablePrefab != null) ? itemAsset3.equipablePrefab : itemAsset3.item);
            Material tempMaterial2;
            Transform transform7 = ItemTool.InstantiateItem(100, itemAsset3.getState(), viewmodel: false, itemAsset3, null, shouldDestroyColliders: true, null, out tempMaterial2, null, prefabOverride2);
            if (npcAsset.equipped == ESlotType.SECONDARY)
            {
                Transform parent7 = itemAsset3.EquipableModelParent switch
                {
                    EEquipableModelParent.LeftHook => transform4, 
                    EEquipableModelParent.Spine => transform3, 
                    _ => transform5, 
                };
                transform7.transform.parent = parent7;
                itemAsset = itemAsset3;
            }
            else if (itemAsset3.type == EItemType.MELEE)
            {
                transform7.transform.parent = parent4;
            }
            else
            {
                transform7.transform.parent = parent5;
            }
            transform7.localPosition = Vector3.zero;
            transform7.localRotation = Quaternion.Euler(0f, 0f, 90f);
            transform7.localScale = Vector3.one;
            transform7.DestroyRigidbody();
            Layerer.enemy(transform7);
        }
        ItemAsset itemAsset4 = Assets.FindItemByGuidOrLegacyId<ItemAsset>(npcAsset.tertiaryWeaponGuid, npcAsset.tertiary);
        if (itemAsset4 != null && npcAsset.equipped == ESlotType.TERTIARY)
        {
            GameObject prefabOverride3 = ((itemAsset4.equipablePrefab != null) ? itemAsset4.equipablePrefab : itemAsset4.item);
            Material tempMaterial3;
            Transform transform8 = ItemTool.InstantiateItem(100, itemAsset4.getState(), viewmodel: false, itemAsset4, null, shouldDestroyColliders: true, null, out tempMaterial3, null, prefabOverride3);
            Transform parent8 = itemAsset4.EquipableModelParent switch
            {
                EEquipableModelParent.LeftHook => transform4, 
                EEquipableModelParent.Spine => transform3, 
                _ => transform5, 
            };
            transform8.transform.parent = parent8;
            itemAsset = itemAsset4;
            transform8.localPosition = Vector3.zero;
            transform8.localRotation = Quaternion.Euler(0f, 0f, 90f);
            transform8.localScale = Vector3.one;
            transform8.DestroyRigidbody();
            Layerer.enemy(transform8);
        }
        if (itemAsset != null && itemAsset.animations != null)
        {
            Transform mix = transform2.Find("Spine").Find("Left_Shoulder");
            Transform mix2 = transform2.Find("Spine").Find("Right_Shoulder");
            for (int i = 0; i < itemAsset.animations.Length; i++)
            {
                AnimationClip animationClip = itemAsset.animations[i];
                if (animationClip.name == "Equip")
                {
                    itemHasEquipAnimation = true;
                }
                else if (animationClip.name == "Sprint_Start" || animationClip.name == "Sprint_Stop")
                {
                    itemHasSafetyAnimation = true;
                }
                else
                {
                    if (!(animationClip.name == "Inspect"))
                    {
                        continue;
                    }
                    itemHasInspectAnimation = true;
                }
                anim.AddClip(animationClip, animationClip.name);
                anim[animationClip.name].AddMixingTransform(mix, recursive: true);
                anim[animationClip.name].AddMixingTransform(mix2, recursive: true);
                anim[animationClip.name].layer = 1;
            }
        }
        anim["Idle_Kick_Left"].AddMixingTransform(transform2.Find("Left_Hip"), recursive: true);
        anim["Idle_Kick_Left"].layer = 2;
        anim["Idle_Kick_Right"].AddMixingTransform(transform2.Find("Right_Hip"), recursive: true);
        anim["Idle_Kick_Right"].layer = 2;
        updateAnimation();
    }

    public override void use()
    {
        if (npcAsset.FindDialogueAsset() == null)
        {
            UnturnedLog.warn("Failed to find NPC dialogue: " + npcAsset.FriendlyName);
        }
        else
        {
            ObjectManager.SendTalkWithNpcRequest.Invoke(ENetReliability.Reliable, GetNpcNetId());
        }
    }

    public override bool checkUseable()
    {
        if (!PlayerUI.window.showCursor)
        {
            return !npcAsset.IsDialogueRefNull();
        }
        return false;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        message = EPlayerMessage.TALK;
        text = "";
        color = Color.white;
        return !PlayerUI.window.showCursor;
    }

    private void Update()
    {
        if (Dedicator.IsDedicatedServer)
        {
            return;
        }
        humanAnim.lean = npcAsset.poseLean;
        humanAnim.pitch = npcAsset.posePitch;
        humanAnim.offset = npcAsset.poseHeadOffset;
        if (!string.IsNullOrEmpty(stanceActive) && Time.time - lastIdle < anim[stanceActive].length)
        {
            anim.CrossFade(stanceActive);
        }
        else
        {
            stanceActive = null;
            anim.CrossFade(stanceIdle);
        }
        if (!hasPlayedEquipAnimation)
        {
            hasPlayedEquipAnimation = true;
            if (itemHasEquipAnimation && (!itemHasSafetyAnimation || npcAsset.pose != ENPCPose.PASSIVE))
            {
                anim.Play("Equip");
            }
        }
        if (itemHasSafetyAnimation)
        {
            if (isLookingAtPlayer || npcAsset.pose == ENPCPose.PASSIVE)
            {
                if (!isPlayingSafetyAnimation)
                {
                    isPlayingSafetyAnimation = true;
                    anim.Play("Sprint_Start");
                }
                return;
            }
            if (isPlayingSafetyAnimation)
            {
                isPlayingSafetyAnimation = false;
                anim.Play("Sprint_Stop");
                updateIdle();
            }
        }
        if (!(Time.time - lastIdle > idleDelay))
        {
            return;
        }
        updateIdle();
        if (itemHasInspectAnimation && (!itemHasSafetyAnimation || npcAsset.pose != ENPCPose.PASSIVE) && Random.value < 0.1f)
        {
            anim.Play("Inspect");
        }
        else if (!itemHasEquipAnimation && Random.value < 0.5f)
        {
            if (Random.value < 0.25f)
            {
                updateStance();
            }
            else if (npcAsset.pose == ENPCPose.STAND)
            {
                stanceActive = "Idle_Hands_" + Random.Range(0, 5);
            }
        }
        else
        {
            if (npcAsset.pose != 0 && npcAsset.pose != ENPCPose.PASSIVE && npcAsset.pose != ENPCPose.UNDER_ARREST && npcAsset.pose != ENPCPose.SURRENDER)
            {
                return;
            }
            if (Random.value < 0.1f)
            {
                if (Random.value < 0.5f)
                {
                    anim.Play("Idle_Kick_Left");
                }
                else
                {
                    anim.Play("Idle_Kick_Right");
                }
            }
            else if (npcAsset.pose != ENPCPose.UNDER_ARREST && npcAsset.pose != ENPCPose.SURRENDER)
            {
                stanceActive = "Idle_Paranoid_" + Random.Range(0, 6);
            }
        }
    }

    private void LateUpdate()
    {
        if (Dedicator.IsDedicatedServer || skull == null || Player.player == null || npcAsset.pose == ENPCPose.ASLEEP)
        {
            return;
        }
        Vector3 vector = Player.player.look.aim.position + new Vector3(0f, -0.45f, 0f) - skull.position;
        if ((isLookingAtPlayer || vector.sqrMagnitude < 4f) && Vector3.Dot(vector, -base.transform.up) > 0.15f)
        {
            headBlend = Mathf.Lerp(headBlend, 1f, 4f * Time.deltaTime);
            if (npcAsset.IsLeftHanded)
            {
                headRotation = Quaternion.Lerp(headRotation, Quaternion.LookRotation(vector, Vector3.up) * Quaternion.Euler(0f, 0f, 90f), 4f * Time.deltaTime);
            }
            else
            {
                headRotation = Quaternion.Lerp(headRotation, Quaternion.LookRotation(vector, Vector3.up) * Quaternion.Euler(0f, 0f, -90f), 4f * Time.deltaTime);
            }
        }
        else
        {
            headBlend = Mathf.Lerp(headBlend, 0f, 4f * Time.deltaTime);
        }
        if (!(headBlend < 0.01f))
        {
            skull.rotation = Quaternion.Lerp(skull.rotation, headRotation, headBlend);
        }
    }

    private void OnEnable()
    {
        if (!Dedicator.IsDedicatedServer)
        {
            updateAnimation();
        }
    }
}
