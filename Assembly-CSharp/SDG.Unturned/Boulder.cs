using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Boulder : MonoBehaviour
{
    private static readonly float DAMAGE_PLAYER = 3f;

    private static readonly float DAMAGE_BARRICADE = 15f;

    private static readonly float DAMAGE_STRUCTURE = 15f;

    private static readonly float DAMAGE_OBJECT = 25f;

    private static readonly float DAMAGE_VEHICLE = 10f;

    private static readonly float DAMAGE_RESOURCE = 25f;

    private bool isExploded;

    private Vector3 lastPos;

    internal static AssetReference<EffectAsset> Metal_2_Ref = new AssetReference<EffectAsset>("b7d53965bc6545c28e029175af35de30");

    private void OnTriggerEnter(Collider other)
    {
        if (isExploded || other.isTrigger || other.transform.CompareTag("Agent"))
        {
            return;
        }
        isExploded = true;
        Vector3 normalized = (base.transform.position - lastPos).normalized;
        if (Provider.isServer)
        {
            float num = Mathf.Clamp(base.transform.parent.GetComponent<Rigidbody>().velocity.magnitude, 0f, 20f);
            if (num < 3f)
            {
                return;
            }
            if (other.transform.CompareTag("Player"))
            {
                Player player = DamageTool.getPlayer(other.transform);
                if (player != null)
                {
                    DamageTool.damage(player, EDeathCause.BOULDER, ELimb.SPINE, CSteamID.Nil, normalized, DAMAGE_PLAYER, num, out var _);
                }
            }
            else if (other.transform.CompareTag("Vehicle"))
            {
                if (Provider.modeConfigData.Zombies.Can_Target_Vehicles)
                {
                    InteractableVehicle component = other.transform.GetComponent<InteractableVehicle>();
                    if (component != null && component.asset != null && component.asset.isVulnerableToEnvironment)
                    {
                        VehicleManager.damage(component, DAMAGE_VEHICLE, num, canRepair: true, default(CSteamID), EDamageOrigin.Mega_Zombie_Boulder);
                    }
                }
            }
            else if (other.transform.CompareTag("Barricade"))
            {
                if (Provider.modeConfigData.Zombies.Can_Target_Barricades)
                {
                    Transform barricadeRootTransform = DamageTool.getBarricadeRootTransform(other.transform);
                    if (barricadeRootTransform != null)
                    {
                        BarricadeManager.damage(barricadeRootTransform, DAMAGE_BARRICADE, num, armor: true, default(CSteamID), EDamageOrigin.Mega_Zombie_Boulder);
                    }
                }
            }
            else if (other.transform.CompareTag("Structure"))
            {
                if (Provider.modeConfigData.Zombies.Can_Target_Structures)
                {
                    Transform structureRootTransform = DamageTool.getStructureRootTransform(other.transform);
                    if (structureRootTransform != null)
                    {
                        StructureManager.damage(structureRootTransform, normalized, DAMAGE_STRUCTURE, num, armor: true, default(CSteamID), EDamageOrigin.Mega_Zombie_Boulder);
                    }
                }
            }
            else if (other.transform.CompareTag("Resource"))
            {
                Transform resourceRootTransform = DamageTool.getResourceRootTransform(other.transform);
                if (resourceRootTransform != null)
                {
                    ResourceManager.damage(resourceRootTransform, normalized, DAMAGE_RESOURCE, num, 1f, out var _, out var _, default(CSteamID), EDamageOrigin.Mega_Zombie_Boulder);
                }
            }
            else
            {
                InteractableObjectRubble componentInParent = other.transform.GetComponentInParent<InteractableObjectRubble>();
                if (componentInParent != null)
                {
                    DamageTool.damage(componentInParent.transform, normalized, componentInParent.getSection(other.transform), DAMAGE_OBJECT, num, out var _, out var _, default(CSteamID), EDamageOrigin.Mega_Zombie_Boulder);
                }
            }
        }
        if (!Dedicator.IsDedicatedServer)
        {
            EffectAsset effectAsset = Assets.find(Metal_2_Ref);
            if (effectAsset != null)
            {
                EffectManager.effect(effectAsset, base.transform.position, -normalized);
            }
        }
    }

    private void FixedUpdate()
    {
        lastPos = base.transform.position;
    }

    private void Awake()
    {
        lastPos = base.transform.position;
    }
}
