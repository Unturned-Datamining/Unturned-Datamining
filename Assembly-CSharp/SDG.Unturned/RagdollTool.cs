using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class RagdollTool
{
    private static List<Renderer> tempRenderers = new List<Renderer>();

    private static List<Rigidbody> tempRigidbodies = new List<Rigidbody>();

    private static List<CharacterJoint> tempJoints = new List<CharacterJoint>();

    private static List<Material> tempMaterials = new List<Material>();

    private static Material bronzeMaterial;

    private static Material silverMaterial;

    private static Material goldMaterial;

    private static Material zeroKelvinMaterial;

    private static Material jadedMaterial;

    private static Material soulCrystalGreenMaterial;

    private static Material soulCrystalMagentaMaterial;

    private static Material soulCrystalRedMaterial;

    private static Material soulCrystalYellowMaterial;

    private static Material getRagdollEffectMaterial(ERagdollEffect effect)
    {
        switch (effect)
        {
        case ERagdollEffect.BRONZE:
            if (bronzeMaterial == null)
            {
                bronzeMaterial = Resources.Load<Material>("Characters/RagdollMaterials/Bronze");
            }
            return bronzeMaterial;
        case ERagdollEffect.SILVER:
            if (silverMaterial == null)
            {
                silverMaterial = Resources.Load<Material>("Characters/RagdollMaterials/Silver");
            }
            return silverMaterial;
        case ERagdollEffect.GOLD:
            if (goldMaterial == null)
            {
                goldMaterial = Resources.Load<Material>("Characters/RagdollMaterials/Gold");
            }
            return goldMaterial;
        case ERagdollEffect.ZERO_KELVIN:
            if (zeroKelvinMaterial == null)
            {
                zeroKelvinMaterial = Resources.Load<Material>("Characters/RagdollMaterials/Zero_Kelvin");
            }
            return zeroKelvinMaterial;
        case ERagdollEffect.JADED:
            if (jadedMaterial == null)
            {
                jadedMaterial = Resources.Load<Material>("Characters/RagdollMaterials/Jaded");
            }
            return jadedMaterial;
        case ERagdollEffect.SOUL_CRYSTAL_GREEN:
            if (soulCrystalGreenMaterial == null)
            {
                soulCrystalGreenMaterial = Resources.Load<Material>("Characters/RagdollMaterials/SoulCrystal_Green");
            }
            return soulCrystalGreenMaterial;
        case ERagdollEffect.SOUL_CRYSTAL_MAGENTA:
            if (soulCrystalMagentaMaterial == null)
            {
                soulCrystalMagentaMaterial = Resources.Load<Material>("Characters/RagdollMaterials/SoulCrystal_Magenta");
            }
            return soulCrystalMagentaMaterial;
        case ERagdollEffect.SOUL_CRYSTAL_RED:
            if (soulCrystalRedMaterial == null)
            {
                soulCrystalRedMaterial = Resources.Load<Material>("Characters/RagdollMaterials/SoulCrystal_Red");
            }
            return soulCrystalRedMaterial;
        case ERagdollEffect.SOUL_CRYSTAL_YELLOW:
            if (soulCrystalYellowMaterial == null)
            {
                soulCrystalYellowMaterial = Resources.Load<Material>("Characters/RagdollMaterials/SoulCrystal_Yellow");
            }
            return soulCrystalYellowMaterial;
        default:
            return null;
        }
    }

    /// <summary>
    /// Find materials in finished ragdoll and replace them with the appropriate effect.
    /// </summary>
    private static void applyRagdollEffect(Transform root, ERagdollEffect effect)
    {
        if (effect == ERagdollEffect.NONE)
        {
            return;
        }
        Material ragdollEffectMaterial = getRagdollEffectMaterial(effect);
        if (ragdollEffectMaterial == null)
        {
            UnturnedLog.warn("Unable to load ragdoll effect material " + effect);
            return;
        }
        tempRenderers.Clear();
        root.GetComponentsInChildren(tempRenderers);
        foreach (Renderer tempRenderer in tempRenderers)
        {
            tempMaterials.Clear();
            tempRenderer.GetSharedMaterials(tempMaterials);
            if (tempMaterials.Count > 1)
            {
                for (int i = 0; i < tempMaterials.Count; i++)
                {
                    tempMaterials[i] = ragdollEffectMaterial;
                }
                tempRenderer.sharedMaterials = tempMaterials.ToArray();
            }
            else
            {
                tempRenderer.sharedMaterial = ragdollEffectMaterial;
            }
        }
        Rigidbody componentInChildren = root.GetComponentInChildren<Rigidbody>();
        tempJoints.Clear();
        root.GetComponentsInChildren(tempJoints);
        foreach (CharacterJoint tempJoint in tempJoints)
        {
            Object.Destroy(tempJoint);
        }
        tempRigidbodies.Clear();
        root.GetComponentsInChildren(tempRigidbodies);
        foreach (Rigidbody tempRigidbody in tempRigidbodies)
        {
            if (tempRigidbody != componentInChildren)
            {
                Object.Destroy(tempRigidbody);
            }
        }
    }

    private static void applySkeleton(Transform skeleton_0, Transform skeleton_1)
    {
        if (skeleton_0 == null || skeleton_1 == null)
        {
            return;
        }
        for (int i = 0; i < skeleton_1.childCount; i++)
        {
            Transform child = skeleton_1.GetChild(i);
            Transform transform = skeleton_0.Find(child.name);
            if (transform != null)
            {
                child.localPosition = transform.localPosition;
                child.localRotation = transform.localRotation;
                if (transform.childCount > 0 && child.childCount > 0)
                {
                    applySkeleton(transform, child);
                }
            }
        }
    }

    public static void ragdollPlayer(Vector3 point, Quaternion rotation, Transform skeleton, Vector3 ragdoll, PlayerClothing clothes, ERagdollEffect effect)
    {
        if (GraphicsSettings.ragdolls)
        {
            ragdoll.y += 8f;
            ragdoll.x += Random.Range(-16f, 16f);
            ragdoll.z += Random.Range(-16f, 16f);
            ragdoll *= (float)((Player.player != null && Player.player.skills.boost == EPlayerBoost.FLIGHT) ? 256 : 32);
            Transform transform = ((GameObject)Object.Instantiate(Resources.Load("Characters/Ragdoll_Player"), point + Vector3.up * 0.1f, rotation * Quaternion.Euler(90f, 0f, 0f))).transform;
            transform.name = "Ragdoll";
            EffectManager.RegisterDebris(transform.gameObject);
            if (skeleton != null)
            {
                applySkeleton(skeleton, transform.Find("Skeleton"));
            }
            transform.Find("Skeleton")?.Find("Spine")?.GetComponent<Rigidbody>()?.AddForce(ragdoll);
            Object.Destroy(transform.gameObject, GraphicsSettings.effect);
            if (clothes != null && clothes.thirdClothes != null)
            {
                HumanClothes component = transform.GetComponent<HumanClothes>();
                component.isRagdoll = true;
                component.skin = clothes.skin;
                component.color = clothes.color;
                component.face = clothes.face;
                component.hair = clothes.hair;
                component.beard = clothes.beard;
                component.shirtAsset = clothes.shirtAsset;
                component.pantsAsset = clothes.pantsAsset;
                component.hatAsset = clothes.hatAsset;
                component.backpackAsset = clothes.backpackAsset;
                component.vestAsset = clothes.vestAsset;
                component.maskAsset = clothes.maskAsset;
                component.glassesAsset = clothes.glassesAsset;
                component.visualShirt = clothes.visualShirt;
                component.visualPants = clothes.visualPants;
                component.visualHat = clothes.visualHat;
                component.visualBackpack = clothes.visualBackpack;
                component.visualVest = clothes.visualVest;
                component.visualMask = clothes.visualMask;
                component.visualGlasses = clothes.visualGlasses;
                component.isVisual = clothes.isVisual;
                component.apply();
            }
            applyRagdollEffect(transform, effect);
        }
    }

    public static Transform ragdollZombie(Vector3 point, Quaternion rotation, Transform skeleton, Vector3 ragdoll, byte type, byte shirt, byte pants, byte hat, byte gear, ushort hatID, ushort gearID, bool isMega, ERagdollEffect effect)
    {
        if (!GraphicsSettings.ragdolls)
        {
            return null;
        }
        ragdoll.y += 8f;
        ragdoll.x += Random.Range(-16f, 16f);
        ragdoll.z += Random.Range(-16f, 16f);
        ragdoll *= (float)((Player.player != null && Player.player.skills.boost == EPlayerBoost.FLIGHT) ? 256 : 32);
        Transform transform = ((GameObject)Object.Instantiate(Resources.Load("Characters/Ragdoll_Zombie"), point + Vector3.up * 0.1f, rotation * Quaternion.Euler(90f, 0f, 0f))).transform;
        transform.name = "Ragdoll";
        EffectManager.RegisterDebris(transform.gameObject);
        if (isMega)
        {
            transform.localScale = Vector3.one * 1.5f;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
        if (skeleton != null)
        {
            applySkeleton(skeleton, transform.Find("Skeleton"));
        }
        transform.Find("Skeleton")?.Find("Spine")?.GetComponent<Rigidbody>()?.AddForce(ragdoll);
        Object.Destroy(transform.gameObject, GraphicsSettings.effect);
        ZombieClothing.EApplyFlags eApplyFlags = ZombieClothing.EApplyFlags.Ragdoll;
        if (isMega)
        {
            eApplyFlags |= ZombieClothing.EApplyFlags.Mega;
        }
        ZombieClothing.apply(transform, eApplyFlags, null, transform.Find("Model_1").GetComponent<SkinnedMeshRenderer>(), type, shirt, pants, hat, gear, hatID, gearID, out var _, out var _);
        applyRagdollEffect(transform, effect);
        return transform;
    }

    public static void ragdollAnimal(Vector3 point, Quaternion rotation, Transform skeleton, Vector3 ragdoll, ushort id, ERagdollEffect effect)
    {
        if (!GraphicsSettings.ragdolls)
        {
            return;
        }
        ragdoll.y += 8f;
        ragdoll.x += Random.Range(-16f, 16f);
        ragdoll.z += Random.Range(-16f, 16f);
        ragdoll *= (float)((Player.player != null && Player.player.skills.boost == EPlayerBoost.FLIGHT) ? 256 : 32);
        if (Assets.find(EAssetType.ANIMAL, id) is AnimalAsset animalAsset)
        {
            Transform transform = Object.Instantiate(animalAsset.ragdoll, point + Vector3.up * 0.1f, rotation * Quaternion.Euler(0f, 90f, 0f)).transform;
            transform.name = "Ragdoll";
            EffectManager.RegisterDebris(transform.gameObject);
            if (skeleton != null)
            {
                applySkeleton(skeleton, transform.Find("Skeleton"));
            }
            transform.Find("Skeleton")?.Find("Spine")?.GetComponent<Rigidbody>()?.AddForce(ragdoll);
            Object.Destroy(transform.gameObject, GraphicsSettings.effect);
            applyRagdollEffect(transform, effect);
        }
    }
}
