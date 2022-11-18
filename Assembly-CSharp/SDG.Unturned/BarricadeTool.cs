using UnityEngine;

namespace SDG.Unturned;

public class BarricadeTool : MonoBehaviour
{
    public static Transform getBarricade(Transform parent, byte hp, Vector3 pos, Quaternion rot, ushort id, byte[] state)
    {
        ItemBarricadeAsset asset = Assets.find(EAssetType.ITEM, id) as ItemBarricadeAsset;
        return getBarricade(parent, hp, 0uL, 0uL, pos, rot, id, state, asset);
    }

    private static Transform getEmptyBarricade(ushort id)
    {
        Transform obj = new GameObject().transform;
        obj.name = id.ToString();
        obj.tag = "Barricade";
        obj.gameObject.layer = 27;
        return obj;
    }

    public static Transform getBarricade(Transform parent, byte hp, ulong owner, ulong group, Vector3 pos, Quaternion rot, ushort id, byte[] state, ItemBarricadeAsset asset)
    {
        if (asset != null)
        {
            Transform transform = ((!(asset.barricade != null)) ? null : Object.Instantiate(asset.barricade).transform);
            if (transform == null)
            {
                transform = getEmptyBarricade(id);
            }
            _ = asset.useWaterHeightTransparentSort;
            transform.name = id.ToString();
            transform.parent = parent;
            transform.localPosition = pos;
            transform.localRotation = rot;
            if (Provider.isServer && asset.nav != null)
            {
                Transform transform2 = Object.Instantiate(asset.nav).transform;
                transform2.name = "Nav";
                if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
                {
                    Transform transform3 = transform.Find("Skeleton").Find("Hinge");
                    if (transform3 != null)
                    {
                        transform2.parent = transform3;
                    }
                    else
                    {
                        transform2.parent = transform;
                    }
                }
                else
                {
                    transform2.parent = transform;
                }
                transform2.localPosition = Vector3.zero;
                transform2.localRotation = Quaternion.identity;
            }
            Transform transform4 = transform.FindChildRecursive("Burning");
            if (transform4 != null)
            {
                transform4.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.BURNING;
            }
            Transform transform5 = transform.FindChildRecursive("Warm");
            if (transform5 != null)
            {
                transform5.gameObject.AddComponent<TemperatureTrigger>().temperature = EPlayerTemperature.WARM;
            }
            if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
            {
                InteractableDoor interactableDoor = transform.gameObject.AddComponent<InteractableDoor>();
                interactableDoor.updateState(asset, state);
                Transform transform6 = transform.Find("Skeleton").Find("Hinge");
                if (transform6 != null)
                {
                    transform6.gameObject.AddComponent<InteractableDoorHinge>().door = interactableDoor;
                }
                Transform transform7 = transform.Find("Skeleton").Find("Left_Hinge");
                if (transform7 != null)
                {
                    transform7.gameObject.AddComponent<InteractableDoorHinge>().door = interactableDoor;
                }
                Transform transform8 = transform.Find("Skeleton").Find("Right_Hinge");
                if (transform8 != null)
                {
                    transform8.gameObject.AddComponent<InteractableDoorHinge>().door = interactableDoor;
                }
            }
            else if (asset.build == EBuild.BED)
            {
                transform.gameObject.AddComponent<InteractableBed>().updateState(asset, state);
            }
            else if (asset.build == EBuild.STORAGE || asset.build == EBuild.STORAGE_WALL)
            {
                transform.gameObject.AddComponent<InteractableStorage>().updateState(asset, state);
            }
            else if (asset.build == EBuild.FARM)
            {
                transform.gameObject.AddComponent<InteractableFarm>().updateState(asset, state);
            }
            else if (asset.build == EBuild.TORCH || asset.build == EBuild.CAMPFIRE)
            {
                transform.gameObject.AddComponent<InteractableFire>().updateState(asset, state);
            }
            else if (asset.build == EBuild.OVEN)
            {
                transform.gameObject.AddComponent<InteractableOven>().updateState(asset, state);
            }
            else if (asset.build == EBuild.SPIKE || asset.build == EBuild.WIRE)
            {
                transform.Find("Trap").gameObject.AddComponent<InteractableTrap>().updateState(asset, state);
            }
            else if (asset.build == EBuild.CHARGE)
            {
                InteractableCharge interactableCharge = transform.gameObject.AddComponent<InteractableCharge>();
                interactableCharge.updateState(asset, state);
                interactableCharge.owner = owner;
                interactableCharge.group = group;
            }
            else if (asset.build == EBuild.GENERATOR)
            {
                transform.gameObject.AddComponent<InteractableGenerator>().updateState(asset, state);
            }
            else if (asset.build == EBuild.SPOT || asset.build == EBuild.CAGE)
            {
                transform.gameObject.AddComponent<InteractableSpot>().updateState(asset, state);
            }
            else if (asset.build == EBuild.SAFEZONE)
            {
                transform.gameObject.AddComponent<InteractableSafezone>().updateState(asset, state);
            }
            else if (asset.build == EBuild.OXYGENATOR)
            {
                transform.gameObject.AddComponent<InteractableOxygenator>().updateState(asset, state);
            }
            else if (asset.build == EBuild.SIGN || asset.build == EBuild.SIGN_WALL || asset.build == EBuild.NOTE)
            {
                transform.gameObject.AddComponent<InteractableSign>().updateState(asset, state);
            }
            else if (asset.build == EBuild.CLAIM)
            {
                InteractableClaim interactableClaim = transform.gameObject.AddComponent<InteractableClaim>();
                interactableClaim.owner = owner;
                interactableClaim.group = group;
                interactableClaim.updateState(asset);
            }
            else if (asset.build == EBuild.BEACON)
            {
                transform.gameObject.AddComponent<InteractableBeacon>().updateState(asset);
            }
            else if (asset.build == EBuild.BARREL_RAIN)
            {
                transform.gameObject.AddComponent<InteractableRainBarrel>().updateState(asset, state);
            }
            else if (asset.build == EBuild.OIL)
            {
                transform.gameObject.AddComponent<InteractableOil>().updateState(asset, state);
            }
            else if (asset.build == EBuild.TANK)
            {
                transform.gameObject.AddComponent<InteractableTank>().updateState(asset, state);
            }
            else if (asset.build == EBuild.SENTRY || asset.build == EBuild.SENTRY_FREEFORM)
            {
                InteractableSentry interactableSentry = transform.gameObject.AddComponent<InteractableSentry>();
                InteractablePower interactablePower = (interactableSentry.power = transform.gameObject.AddComponent<InteractablePower>());
                interactableSentry.updateState(asset, state);
            }
            else if (asset.build == EBuild.LIBRARY)
            {
                transform.gameObject.AddComponent<InteractableLibrary>().updateState(asset, state);
            }
            else if (asset.build == EBuild.MANNEQUIN)
            {
                transform.gameObject.AddComponent<InteractableMannequin>().updateState(asset, state);
            }
            else if (asset.build == EBuild.STEREO)
            {
                transform.gameObject.AddComponent<InteractableStereo>().updateState(asset, state);
            }
            else
            {
                _ = asset.build;
            }
            if (!asset.isUnpickupable)
            {
                Interactable2HP interactable2HP = transform.gameObject.AddComponent<Interactable2HP>();
                interactable2HP.hp = hp;
                if (asset.build == EBuild.DOOR || asset.build == EBuild.GATE || asset.build == EBuild.SHUTTER || asset.build == EBuild.HATCH)
                {
                    Transform transform9 = transform.Find("Skeleton").Find("Hinge");
                    if (transform9 != null)
                    {
                        Interactable2SalvageBarricade interactable2SalvageBarricade = transform9.gameObject.AddComponent<Interactable2SalvageBarricade>();
                        interactable2SalvageBarricade.root = transform;
                        interactable2SalvageBarricade.hp = interactable2HP;
                        interactable2SalvageBarricade.owner = owner;
                        interactable2SalvageBarricade.group = group;
                        interactable2SalvageBarricade.salvageDurationMultiplier = asset.salvageDurationMultiplier;
                    }
                    Transform transform10 = transform.Find("Skeleton").Find("Left_Hinge");
                    if (transform10 != null)
                    {
                        Interactable2SalvageBarricade interactable2SalvageBarricade2 = transform10.gameObject.AddComponent<Interactable2SalvageBarricade>();
                        interactable2SalvageBarricade2.root = transform;
                        interactable2SalvageBarricade2.hp = interactable2HP;
                        interactable2SalvageBarricade2.owner = owner;
                        interactable2SalvageBarricade2.group = group;
                        interactable2SalvageBarricade2.salvageDurationMultiplier = asset.salvageDurationMultiplier;
                    }
                    Transform transform11 = transform.Find("Skeleton").Find("Right_Hinge");
                    if (transform11 != null)
                    {
                        Interactable2SalvageBarricade interactable2SalvageBarricade3 = transform11.gameObject.AddComponent<Interactable2SalvageBarricade>();
                        interactable2SalvageBarricade3.root = transform;
                        interactable2SalvageBarricade3.hp = interactable2HP;
                        interactable2SalvageBarricade3.owner = owner;
                        interactable2SalvageBarricade3.group = group;
                        interactable2SalvageBarricade3.salvageDurationMultiplier = asset.salvageDurationMultiplier;
                    }
                }
                else
                {
                    Interactable2SalvageBarricade interactable2SalvageBarricade4 = transform.gameObject.AddComponent<Interactable2SalvageBarricade>();
                    interactable2SalvageBarricade4.root = transform;
                    interactable2SalvageBarricade4.hp = interactable2HP;
                    interactable2SalvageBarricade4.owner = owner;
                    interactable2SalvageBarricade4.group = group;
                    interactable2SalvageBarricade4.salvageDurationMultiplier = asset.salvageDurationMultiplier;
                }
            }
            return transform;
        }
        return getEmptyBarricade(id);
    }
}
