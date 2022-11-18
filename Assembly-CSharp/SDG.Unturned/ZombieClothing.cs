using System;
using UnityEngine;

namespace SDG.Unturned;

public class ZombieClothing
{
    [Flags]
    public enum EApplyFlags
    {
        None = 1,
        Mega = 2,
        Ragdoll = 4
    }

    private static Mesh megaMesh_0;

    private static Mesh megaMesh_1;

    private static Mesh zombieMesh_0;

    private static Mesh zombieMesh_1;

    private static Texture2D faceTexture;

    private static Shader clothingShader;

    private static Material[][,] clothes;

    public static Material ghostMaterial { get; private set; }

    public static Material ghostSpiritMaterial { get; private set; }

    public static Material paint(ushort shirt, ushort pants, bool isMega)
    {
        Material material = new Material(clothingShader);
        material.name = "Zombie_" + (isMega ? "Mega" : "Normal") + "_" + shirt + "_" + pants;
        material.hideFlags = HideFlags.HideAndDontSave;
        material.SetColor(HumanClothes.skinColorPropertyID, isMega ? new Color32(89, 99, 89, byte.MaxValue) : new Color32(99, 124, 99, byte.MaxValue));
        material.SetTexture(HumanClothes.faceAlbedoTexturePropertyID, faceTexture);
        if (shirt != 0 && Assets.find(EAssetType.ITEM, shirt) is ItemShirtAsset itemShirtAsset)
        {
            material.SetTexture(HumanClothes.shirtAlbedoTexturePropertyID, itemShirtAsset.shirt);
            material.SetTexture(HumanClothes.shirtEmissionTexturePropertyID, itemShirtAsset.emission);
            material.SetTexture(HumanClothes.shirtMetallicTexturePropertyID, itemShirtAsset.metallic);
        }
        if (pants != 0 && Assets.find(EAssetType.ITEM, pants) is ItemPantsAsset itemPantsAsset)
        {
            material.SetTexture(HumanClothes.pantsAlbedoTexturePropertyID, itemPantsAsset.pants);
            material.SetTexture(HumanClothes.pantsEmissionTexturePropertyID, itemPantsAsset.emission);
            material.SetTexture(HumanClothes.pantsMetallicTexturePropertyID, itemPantsAsset.metallic);
        }
        return material;
    }

    public static void apply(Transform zombie, EApplyFlags flags, SkinnedMeshRenderer renderer_0, SkinnedMeshRenderer renderer_1, byte type, byte shirt, byte pants, byte hat, byte gear, ushort hatID, ushort gearID, out Transform attachmentModel_0, out Transform attachmentModel_1)
    {
        bool flag = flags.HasFlag(EApplyFlags.Mega);
        bool isRagdoll = flags.HasFlag(EApplyFlags.Ragdoll);
        attachmentModel_0 = null;
        attachmentModel_1 = null;
        Transform transform = zombie.Find("Skeleton").Find("Spine");
        Transform transform2 = transform.Find("Skull");
        if (type >= LevelZombies.tables.Count)
        {
            UnturnedLog.warn("Zombie clothes unknown type index {0}, defaulting to zero", type);
            type = 0;
        }
        if (type >= LevelZombies.tables.Count)
        {
            UnturnedLog.warn("No valid zombie tables, should not have been spawned");
            return;
        }
        ZombieTable zombieTable = LevelZombies.tables[type];
        if (shirt == byte.MaxValue)
        {
            shirt = (byte)zombieTable.slots[0].table.Count;
        }
        else if (shirt > zombieTable.slots[0].table.Count)
        {
            byte b = (byte)zombieTable.slots[0].table.Count;
            UnturnedLog.warn("Zombie clothes unknown shirt index {0}, defaulting to {1}", shirt, b);
            shirt = b;
        }
        if (pants == byte.MaxValue)
        {
            pants = (byte)zombieTable.slots[1].table.Count;
        }
        else if (pants > zombieTable.slots[1].table.Count)
        {
            byte b2 = (byte)zombieTable.slots[1].table.Count;
            UnturnedLog.warn("Zombie clothes unknown pants index {0}, defaulting to {1}", pants, b2);
            pants = b2;
        }
        Material material;
        if (shirt <= zombieTable.slots[0].table.Count && pants <= zombieTable.slots[1].table.Count)
        {
            material = clothes[type][shirt, pants];
        }
        else
        {
            material = null;
            UnturnedLog.warn("Zombies clothes type {0} no valid shirt or pants", type);
        }
        if (material != null)
        {
            if (renderer_0 != null)
            {
                renderer_0.sharedMesh = (flag ? megaMesh_0 : zombieMesh_0);
                renderer_0.sharedMaterial = material;
            }
            if (renderer_1 != null)
            {
                renderer_1.sharedMesh = (flag ? megaMesh_1 : zombieMesh_1);
                renderer_1.sharedMaterial = material;
            }
        }
        Transform transform3 = transform2.Find("Hat");
        if (transform3 != null)
        {
            UnityEngine.Object.Destroy(transform3.gameObject);
        }
        Transform transform4 = transform.Find("Backpack");
        if (transform4 != null)
        {
            UnityEngine.Object.Destroy(transform4.gameObject);
        }
        Transform transform5 = transform.Find("Vest");
        if (transform5 != null)
        {
            UnityEngine.Object.Destroy(transform5.gameObject);
        }
        Transform transform6 = transform2.Find("Mask");
        if (transform6 != null)
        {
            UnityEngine.Object.Destroy(transform6.gameObject);
        }
        Transform transform7 = transform2.Find("Glasses");
        if (transform7 != null)
        {
            UnityEngine.Object.Destroy(transform7.gameObject);
        }
        if (hatID == 0 && hat != byte.MaxValue && hat < zombieTable.slots[2].table.Count)
        {
            hatID = zombieTable.slots[2].table[hat].item;
        }
        if (hatID != 0 && Assets.find(EAssetType.ITEM, hatID) is ItemClothingAsset itemClothingAsset && itemClothingAsset.shouldBeVisible(isRagdoll))
        {
            if (itemClothingAsset.type == EItemType.HAT)
            {
                transform3 = UnityEngine.Object.Instantiate(((ItemHatAsset)itemClothingAsset).hat).transform;
                transform3.name = "Hat";
                transform3.transform.parent = transform2;
                transform3.transform.localPosition = Vector3.zero;
                transform3.transform.localRotation = Quaternion.identity;
                transform3.transform.localScale = Vector3.one;
                UnityEngine.Object.Destroy(transform3.GetComponent<Collider>());
                transform3.DestroyRigidbody();
                attachmentModel_0 = transform3.transform;
            }
            else if (itemClothingAsset.type == EItemType.BACKPACK)
            {
                transform4 = UnityEngine.Object.Instantiate(((ItemBackpackAsset)itemClothingAsset).backpack).transform;
                transform4.name = "Backpack";
                transform4.transform.parent = transform;
                transform4.transform.localPosition = Vector3.zero;
                transform4.transform.localRotation = Quaternion.identity;
                transform4.transform.localScale = (flag ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one);
                UnityEngine.Object.Destroy(transform4.GetComponent<Collider>());
                transform4.DestroyRigidbody();
                attachmentModel_0 = transform4.transform;
            }
            else if (itemClothingAsset.type == EItemType.VEST)
            {
                transform5 = UnityEngine.Object.Instantiate(((ItemVestAsset)itemClothingAsset).vest).transform;
                transform5.name = "Vest";
                transform5.transform.parent = transform;
                transform5.transform.localPosition = Vector3.zero;
                transform5.transform.localRotation = Quaternion.identity;
                transform5.transform.localScale = (flag ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one);
                UnityEngine.Object.Destroy(transform5.GetComponent<Collider>());
                transform5.DestroyRigidbody();
                attachmentModel_0 = transform5.transform;
            }
            else if (itemClothingAsset.type == EItemType.MASK)
            {
                transform6 = UnityEngine.Object.Instantiate(((ItemMaskAsset)itemClothingAsset).mask).transform;
                transform6.name = "Mask";
                transform6.transform.parent = transform2;
                transform6.transform.localPosition = Vector3.zero;
                transform6.transform.localRotation = Quaternion.identity;
                transform6.transform.localScale = Vector3.one;
                UnityEngine.Object.Destroy(transform6.GetComponent<Collider>());
                transform6.DestroyRigidbody();
                attachmentModel_0 = transform6.transform;
            }
            else if (itemClothingAsset.type == EItemType.GLASSES)
            {
                transform7 = UnityEngine.Object.Instantiate(((ItemGlassesAsset)itemClothingAsset).glasses).transform;
                transform7.name = "Glasses";
                transform7.transform.parent = transform2;
                transform7.transform.localPosition = Vector3.zero;
                transform7.transform.localRotation = Quaternion.identity;
                transform7.transform.localScale = Vector3.one;
                UnityEngine.Object.Destroy(transform7.GetComponent<Collider>());
                transform7.DestroyRigidbody();
                attachmentModel_0 = transform7.transform;
            }
        }
        if (gearID == 0 && gear != byte.MaxValue && gear < zombieTable.slots[3].table.Count)
        {
            gearID = zombieTable.slots[3].table[gear].item;
        }
        if (gearID != 0 && Assets.find(EAssetType.ITEM, gearID) is ItemClothingAsset itemClothingAsset2 && itemClothingAsset2.shouldBeVisible(isRagdoll))
        {
            if (itemClothingAsset2.type == EItemType.HAT)
            {
                transform3 = UnityEngine.Object.Instantiate(((ItemHatAsset)itemClothingAsset2).hat).transform;
                transform3.name = "Hat";
                transform3.transform.parent = transform2;
                transform3.transform.localPosition = Vector3.zero;
                transform3.transform.localRotation = Quaternion.identity;
                transform3.transform.localScale = Vector3.one;
                UnityEngine.Object.Destroy(transform3.GetComponent<Collider>());
                transform3.DestroyRigidbody();
                attachmentModel_1 = transform3.transform;
            }
            else if (itemClothingAsset2.type == EItemType.BACKPACK)
            {
                transform4 = UnityEngine.Object.Instantiate(((ItemBackpackAsset)itemClothingAsset2).backpack).transform;
                transform4.name = "Backpack";
                transform4.transform.parent = transform;
                transform4.transform.localPosition = Vector3.zero;
                transform4.transform.localRotation = Quaternion.identity;
                transform4.transform.localScale = (flag ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one);
                UnityEngine.Object.Destroy(transform4.GetComponent<Collider>());
                transform4.DestroyRigidbody();
                attachmentModel_1 = transform4.transform;
            }
            else if (itemClothingAsset2.type == EItemType.VEST)
            {
                transform5 = UnityEngine.Object.Instantiate(((ItemVestAsset)itemClothingAsset2).vest).transform;
                transform5.name = "Vest";
                transform5.transform.parent = transform;
                transform5.transform.localPosition = Vector3.zero;
                transform5.transform.localRotation = Quaternion.identity;
                transform5.transform.localScale = (flag ? new Vector3(1.05f, 1f, 1.1f) : Vector3.one);
                UnityEngine.Object.Destroy(transform5.GetComponent<Collider>());
                transform5.DestroyRigidbody();
                attachmentModel_1 = transform5.transform;
            }
            else if (itemClothingAsset2.type == EItemType.MASK)
            {
                transform6 = UnityEngine.Object.Instantiate(((ItemMaskAsset)itemClothingAsset2).mask).transform;
                transform6.name = "Mask";
                transform6.transform.parent = transform2;
                transform6.transform.localPosition = Vector3.zero;
                transform6.transform.localRotation = Quaternion.identity;
                transform6.transform.localScale = Vector3.one;
                UnityEngine.Object.Destroy(transform6.GetComponent<Collider>());
                transform6.DestroyRigidbody();
                attachmentModel_1 = transform6.transform;
            }
            else if (itemClothingAsset2.type == EItemType.GLASSES)
            {
                transform7 = UnityEngine.Object.Instantiate(((ItemGlassesAsset)itemClothingAsset2).glasses).transform;
                transform7.name = "Glasses";
                transform7.transform.parent = transform2;
                transform7.transform.localPosition = Vector3.zero;
                transform7.transform.localRotation = Quaternion.identity;
                transform7.transform.localScale = Vector3.one;
                UnityEngine.Object.Destroy(transform7.GetComponent<Collider>());
                transform7.DestroyRigidbody();
                attachmentModel_1 = transform7.transform;
            }
        }
    }

    public static void build()
    {
        if (ghostMaterial == null)
        {
            ghostMaterial = (Material)Resources.Load("Characters/Ghost");
        }
        if (ghostSpiritMaterial == null)
        {
            ghostSpiritMaterial = (Material)Resources.Load("Characters/Ghost_Spirit");
        }
        if (megaMesh_0 == null)
        {
            megaMesh_0 = ((GameObject)Resources.Load("Characters/Mega_0")).GetComponent<MeshFilter>().sharedMesh;
        }
        if (megaMesh_1 == null)
        {
            megaMesh_1 = ((GameObject)Resources.Load("Characters/Mega_1")).GetComponent<MeshFilter>().sharedMesh;
        }
        if (zombieMesh_0 == null)
        {
            zombieMesh_0 = ((GameObject)Resources.Load("Characters/Zombie_0")).GetComponent<MeshFilter>().sharedMesh;
        }
        if (zombieMesh_1 == null)
        {
            zombieMesh_1 = ((GameObject)Resources.Load("Characters/Zombie_1")).GetComponent<MeshFilter>().sharedMesh;
        }
        if (faceTexture == null)
        {
            faceTexture = Resources.Load<Texture2D>("Faces/19/Texture");
        }
        if (clothingShader == null)
        {
            clothingShader = Shader.Find("Standard/Clothes");
        }
        if (clothes != null)
        {
            for (int i = 0; i < clothes.GetLength(0); i++)
            {
                for (int j = 0; j < clothes[i].GetLength(0); j++)
                {
                    for (int k = 0; k < clothes[i].GetLength(1); k++)
                    {
                        if (clothes[i][j, k] != null)
                        {
                            UnityEngine.Object.DestroyImmediate(clothes[i][j, k]);
                            clothes[i][j, k] = null;
                        }
                    }
                }
            }
        }
        if (LevelZombies.tables == null)
        {
            clothes = null;
            return;
        }
        clothes = new Material[LevelZombies.tables.Count][,];
        for (byte b = 0; b < LevelZombies.tables.Count; b = (byte)(b + 1))
        {
            ZombieTable zombieTable = LevelZombies.tables[b];
            clothes[b] = new Material[zombieTable.slots[0].table.Count + 1, zombieTable.slots[1].table.Count + 1];
            for (byte b2 = 0; b2 < zombieTable.slots[0].table.Count + 1; b2 = (byte)(b2 + 1))
            {
                ushort shirt = 0;
                if (b2 < zombieTable.slots[0].table.Count)
                {
                    shirt = zombieTable.slots[0].table[b2].item;
                }
                for (byte b3 = 0; b3 < zombieTable.slots[1].table.Count + 1; b3 = (byte)(b3 + 1))
                {
                    ushort pants = 0;
                    if (b3 < zombieTable.slots[1].table.Count)
                    {
                        pants = zombieTable.slots[1].table[b3].item;
                    }
                    clothes[b][b2, b3] = paint(shirt, pants, zombieTable.isMega);
                }
            }
        }
    }
}
