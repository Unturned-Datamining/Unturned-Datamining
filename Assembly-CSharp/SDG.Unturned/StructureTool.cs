using UnityEngine;

namespace SDG.Unturned;

public class StructureTool : MonoBehaviour
{
    public static Transform getStructure(ushort id, byte hp)
    {
        ItemStructureAsset asset = Assets.find(EAssetType.ITEM, id) as ItemStructureAsset;
        return getStructure(id, hp, 0uL, 0uL, asset);
    }

    private static Transform getEmptyStructure(ushort id)
    {
        Transform obj = new GameObject().transform;
        obj.name = id.ToString();
        obj.tag = "Structure";
        obj.gameObject.layer = 28;
        return obj;
    }

    public static Transform getStructure(ushort id, byte hp, ulong owner, ulong group, ItemStructureAsset asset)
    {
        if (asset != null)
        {
            Transform transform = ((!(asset.structure != null)) ? null : Object.Instantiate(asset.structure).transform);
            if (transform == null)
            {
                transform = getEmptyStructure(id);
            }
            transform.name = id.ToString();
            if (Provider.isServer && asset.nav != null)
            {
                Transform obj = Object.Instantiate(asset.nav).transform;
                obj.name = "Nav";
                obj.parent = transform;
                obj.localPosition = Vector3.zero;
                obj.localRotation = Quaternion.identity;
            }
            if (!asset.isUnpickupable)
            {
                Interactable2HP interactable2HP = transform.gameObject.AddComponent<Interactable2HP>();
                interactable2HP.hp = hp;
                Interactable2SalvageStructure interactable2SalvageStructure = transform.gameObject.AddComponent<Interactable2SalvageStructure>();
                interactable2SalvageStructure.hp = interactable2HP;
                interactable2SalvageStructure.owner = owner;
                interactable2SalvageStructure.group = group;
                interactable2SalvageStructure.salvageDurationMultiplier = asset.salvageDurationMultiplier;
            }
            return transform;
        }
        return getEmptyStructure(id);
    }
}
