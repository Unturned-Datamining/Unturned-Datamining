using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Bumper : MonoBehaviour
{
    public bool reverse;

    public bool instakill;

    private static readonly float DAMAGE_PLAYER = 10f;

    private static readonly float DAMAGE_ZOMBIE = 15f;

    private static readonly float DAMAGE_ANIMAL = 15f;

    private static readonly float DAMAGE_OBJECT = 30f;

    private static readonly float DAMAGE_VEHICLE = 8f;

    private static readonly float DAMAGE_RESOURCE = 85f;

    private InteractableVehicle vehicle;

    private float lastDamageImpact;

    public void init(InteractableVehicle newVehicle)
    {
        vehicle = newVehicle;
    }

    protected CSteamID getInstigatorSteamID()
    {
        if ((bool)vehicle && vehicle.isDriven)
        {
            return vehicle.passengers[0].player.playerID.steamID;
        }
        return CSteamID.Nil;
    }

    protected void takeCrashDamage(float damage, bool canRepair = true)
    {
        if (!(vehicle == null) && vehicle.asset != null && vehicle.asset.isVulnerableToBumper)
        {
            DamageTool.damage(vehicle, damageTires: false, base.transform.position, isRepairing: false, damage, 1f, canRepair, out var _, getInstigatorSteamID(), EDamageOrigin.Vehicle_Collision_Self_Damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || !Provider.isServer || vehicle == null || vehicle.asset == null || other.isTrigger || other.transform.IsChildOf(vehicle.transform) || DamageTool.getVehicle(other.transform) != null || other.CompareTag("Debris"))
        {
            return;
        }
        float num = Mathf.Clamp(vehicle.speed * vehicle.asset.bumperMultiplier, -10f, 10f);
        if (reverse)
        {
            num = 0f - num;
        }
        if (num < 3f)
        {
            return;
        }
        if (other.transform.CompareTag("Player"))
        {
            if (vehicle.isDriven)
            {
                Player player = vehicle.passengers[0].player.player;
                Player player2 = DamageTool.getPlayer(other.transform);
                if (player != null && player2 != null && player2.movement.getVehicle() == null && DamageTool.isPlayerAllowedToDamagePlayer(player, player2))
                {
                    DamageTool.damage(player2, EDeathCause.ROADKILL, ELimb.SPINE, vehicle.passengers[0].player.playerID.steamID, base.transform.forward, instakill ? 101f : DAMAGE_PLAYER, num, out var _, applyGlobalArmorMultiplier: true, trackKill: true);
                    DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up, -base.transform.forward, "Flesh", null, Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
                    takeCrashDamage(2f);
                }
            }
            return;
        }
        if (other.transform.CompareTag("Agent"))
        {
            Zombie zombie = DamageTool.getZombie(other.transform);
            if (zombie != null)
            {
                DamageZombieParameters parameters = new DamageZombieParameters(zombie, base.transform.forward, instakill ? 65000f : DAMAGE_ZOMBIE);
                parameters.times = num;
                parameters.instigator = this;
                DamageTool.damageZombie(parameters, out var _, out var _);
                DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up, -base.transform.forward, zombie.isRadioactive ? "Alien" : "Flesh", null, Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
                takeCrashDamage(2f);
                return;
            }
            Animal animal = DamageTool.getAnimal(other.transform);
            if (animal != null)
            {
                DamageAnimalParameters parameters2 = new DamageAnimalParameters(animal, base.transform.forward, instakill ? 65000f : DAMAGE_ANIMAL);
                parameters2.times = num;
                parameters2.instigator = this;
                DamageTool.damageAnimal(parameters2, out var _, out var _);
                DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up, -base.transform.forward, "Flesh", null, Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
                takeCrashDamage(2f);
            }
            return;
        }
        bool flag = false;
        if (other.transform.CompareTag("Barricade"))
        {
            if (instakill)
            {
                Transform barricadeRootTransform = DamageTool.getBarricadeRootTransform(other.transform);
                if (barricadeRootTransform.parent == null || !barricadeRootTransform.parent.CompareTag("Vehicle"))
                {
                    flag = true;
                    BarricadeManager.damage(barricadeRootTransform, 65000f, num, armor: false, getInstigatorSteamID(), EDamageOrigin.Vehicle_Bumper);
                    takeCrashDamage(DAMAGE_VEHICLE * num);
                }
            }
        }
        else if (other.transform.CompareTag("Structure"))
        {
            if (instakill)
            {
                StructureManager.damage(DamageTool.getStructureRootTransform(other.transform), base.transform.forward, 65000f, num, armor: false, getInstigatorSteamID(), EDamageOrigin.Vehicle_Bumper);
                flag = true;
                takeCrashDamage(DAMAGE_VEHICLE * num);
            }
        }
        else if (other.transform.CompareTag("Resource"))
        {
            Transform resourceRootTransform = DamageTool.getResourceRootTransform(other.transform);
            flag = true;
            ResourceManager.damage(resourceRootTransform, base.transform.forward, instakill ? 65000f : DAMAGE_RESOURCE, num, 1f, out var _, out var _, getInstigatorSteamID(), EDamageOrigin.Vehicle_Bumper);
            takeCrashDamage(DAMAGE_VEHICLE * num);
        }
        else
        {
            InteractableObjectRubble componentInParent = other.transform.GetComponentInParent<InteractableObjectRubble>();
            if (componentInParent != null)
            {
                DamageTool.damage(componentInParent.transform, base.transform.forward, componentInParent.getSection(other.transform), instakill ? 65000f : DAMAGE_OBJECT, num, out var _, out var _, getInstigatorSteamID(), EDamageOrigin.Vehicle_Bumper);
                if (Time.realtimeSinceStartup - lastDamageImpact > 0.2f)
                {
                    lastDamageImpact = Time.realtimeSinceStartup;
                    flag = true;
                    takeCrashDamage(DAMAGE_VEHICLE * num);
                }
            }
            else if (Time.realtimeSinceStartup - lastDamageImpact > 0.2f)
            {
                ObjectAsset asset = LevelObjects.getAsset(other.transform);
                if (asset != null && !asset.isSoft)
                {
                    lastDamageImpact = Time.realtimeSinceStartup;
                    flag = true;
                    takeCrashDamage(DAMAGE_VEHICLE * num);
                }
            }
        }
        if (flag)
        {
            Vector3 position = base.transform.position;
            BoxCollider component = base.transform.GetComponent<BoxCollider>();
            if (component != null)
            {
                position += base.transform.forward * component.size.z * 0.5f;
            }
            string materialName = PhysicsTool.GetMaterialName(position, other.transform, other);
            if (!string.IsNullOrEmpty(materialName))
            {
                DamageTool.ServerSpawnLegacyImpact(position, -base.transform.forward, materialName, null, Provider.GatherClientConnectionsWithinSphere(position, EffectManager.SMALL));
            }
        }
        if (!vehicle.isDead && vehicle.asset.isVulnerableToBumper && !other.transform.CompareTag("Border") && ((vehicle.asset.engine == EEngine.PLANE && vehicle.speed > 20f) || (vehicle.asset.engine == EEngine.HELICOPTER && vehicle.speed > 10f)))
        {
            takeCrashDamage(20000f, canRepair: false);
        }
    }
}
