using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Bumper : MonoBehaviour
{
    public bool reverse;

    public bool instakill;

    private static readonly float DAMAGE_VEHICLE = 8f;

    private InteractableVehicle vehicle;

    private float lastDamageImpact;

    public void init(InteractableVehicle newVehicle)
    {
        vehicle = newVehicle;
    }

    /// <summary>
    /// Get SteamID of vehicle's driver, or nil if not driven.
    /// </summary>
    protected CSteamID getInstigatorSteamID()
    {
        if ((bool)vehicle && vehicle.isDriven)
        {
            return vehicle.passengers[0].player.playerID.steamID;
        }
        return CSteamID.Nil;
    }

    /// <summary>
    /// Crashed into something, if applicable take self damage from collision.
    /// </summary>
    protected void takeCrashDamage(float damage, bool canRepair = true)
    {
        if (!(vehicle == null) && vehicle.asset != null && vehicle.asset.isVulnerableToBumper)
        {
            float bumperSelfDamageMultiplier = vehicle.asset.BumperSelfDamageMultiplier;
            DamageTool.damage(vehicle, damageTires: false, base.transform.position, isRepairing: false, damage, bumperSelfDamageMultiplier, canRepair, out var _, getInstigatorSteamID(), EDamageOrigin.Vehicle_Collision_Self_Damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || !Provider.isServer || vehicle == null || vehicle.asset == null || other.isTrigger || other.transform.IsChildOf(vehicle.transform) || DamageTool.getVehicle(other.transform) != null || other.CompareTag("Debris"))
        {
            return;
        }
        float num = Mathf.Clamp(vehicle.ReplicatedSpeed * vehicle.asset.bumperMultiplier, -10f, 10f);
        if (reverse)
        {
            num = 0f - num;
        }
        if (num < vehicle.asset.BumperSpeedDamageThreshold)
        {
            return;
        }
        Player driverPlayer = vehicle.GetDriverPlayer();
        EPlayerKill kill = EPlayerKill.NONE;
        ERagdollEffect ragdollEffect = ERagdollEffect.None;
        if (vehicle.isSkinned)
        {
            ragdollEffect = driverPlayer.movement.GetVehicleRagdollEffect();
        }
        if (other.transform.CompareTag("Player"))
        {
            if (driverPlayer != null)
            {
                Player player = DamageTool.getPlayer(other.transform);
                if (driverPlayer != null && player != null && player.movement.getVehicle() == null && DamageTool.isPlayerAllowedToDamagePlayer(driverPlayer, player))
                {
                    DamageTool.damage(player, EDeathCause.ROADKILL, ELimb.SPINE, vehicle.passengers[0].player.playerID.steamID, base.transform.forward, instakill ? 101f : vehicle.asset.BumperPlayerDamage, num, out kill, applyGlobalArmorMultiplier: true, trackKill: true, ragdollEffect);
                    DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up, -base.transform.forward, "Flesh", null, Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
                    takeCrashDamage(2f);
                }
            }
        }
        else if (other.transform.CompareTag("Agent"))
        {
            Zombie zombie = DamageTool.getZombie(other.transform);
            if (zombie != null)
            {
                DamageZombieParameters parameters = new DamageZombieParameters(zombie, base.transform.forward, instakill ? 65000f : vehicle.asset.BumperZombieDamage);
                parameters.times = num;
                parameters.instigator = this;
                parameters.ragdollEffect = ragdollEffect;
                DamageTool.damageZombie(parameters, out kill, out var _);
                DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up, -base.transform.forward, zombie.isRadioactive ? "Alien" : "Flesh", null, Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
                takeCrashDamage(2f);
            }
            else
            {
                Animal animal = DamageTool.getAnimal(other.transform);
                if (animal != null)
                {
                    DamageAnimalParameters parameters2 = new DamageAnimalParameters(animal, base.transform.forward, instakill ? 65000f : vehicle.asset.BumperAnimalDamage);
                    parameters2.times = num;
                    parameters2.instigator = this;
                    parameters2.ragdollEffect = ragdollEffect;
                    DamageTool.damageAnimal(parameters2, out kill, out var _);
                    DamageTool.ServerSpawnLegacyImpact(other.transform.position + other.transform.up, -base.transform.forward, "Flesh", null, Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
                    takeCrashDamage(2f);
                }
            }
        }
        else
        {
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
                ResourceManager.damage(resourceRootTransform, base.transform.forward, instakill ? 65000f : vehicle.asset.BumperResourceDamage, num, 1f, out kill, out var _, getInstigatorSteamID(), EDamageOrigin.Vehicle_Bumper);
                takeCrashDamage(DAMAGE_VEHICLE * num);
            }
            else
            {
                InteractableObjectRubble componentInParent = other.transform.GetComponentInParent<InteractableObjectRubble>();
                if (componentInParent != null)
                {
                    DamageTool.damage(componentInParent.transform, base.transform.forward, componentInParent.getSection(other.transform), instakill ? 65000f : vehicle.asset.BumperObjectDamage, num, out kill, out var _, getInstigatorSteamID(), EDamageOrigin.Vehicle_Bumper);
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
            if (!vehicle.isDead && vehicle.asset.isVulnerableToBumper && !other.transform.CompareTag("Border") && ((vehicle.asset.engine == EEngine.PLANE && vehicle.ReplicatedSpeed > 20f) || (vehicle.asset.engine == EEngine.HELICOPTER && vehicle.ReplicatedSpeed > 10f)))
            {
                takeCrashDamage(20000f, canRepair: false);
            }
        }
        if (kill != 0 && driverPlayer != null)
        {
            driverPlayer.sendStat(kill);
        }
    }
}
