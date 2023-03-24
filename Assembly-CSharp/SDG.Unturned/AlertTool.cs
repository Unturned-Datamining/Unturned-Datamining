using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SDG.Unturned;

public class AlertTool
{
    private static List<Zombie> zombiesInRadius = new List<Zombie>();

    private static List<Animal> animalsInRadius = new List<Animal>();

    private static bool check(Vector3 forward, Vector3 offset, float sqrRadius, bool sneak, Vector3 spotlightDir, bool isSpotlightOn, bool isLightSensitive)
    {
        if (isSpotlightOn && offset.sqrMagnitude < 576f && Vector3.Dot(spotlightDir, offset.normalized) > (isLightSensitive ? 0.4f : 0.75f))
        {
            return true;
        }
        if (offset.sqrMagnitude > sqrRadius)
        {
            return false;
        }
        if ((double)Vector3.Dot(forward, offset.normalized) > 0.5 && sneak)
        {
            return false;
        }
        return true;
    }

    public static void alert(Player player, Vector3 position, float radius, bool sneak, Vector3 spotDir, bool isSpotOn)
    {
        _ = Level.getAsset()?.minStealthRadius;
        float min = Mathf.Max(1f, radius);
        radius *= Provider.modeConfigData.Players.Detect_Radius_Multiplier;
        radius = Mathf.Clamp(radius, min, 64f);
        if (player == null)
        {
            return;
        }
        float sqrRadius = radius * radius;
        RaycastHit hitInfo;
        if (player.movement.nav != byte.MaxValue)
        {
            ZombieRegion zombieRegion = ZombieManager.regions[player.movement.nav];
            if (zombieRegion.HasInfiniteAgroRange)
            {
                for (int i = 0; i < zombieRegion.zombies.Count; i++)
                {
                    Zombie zombie = zombieRegion.zombies[i];
                    if (!zombie.isDead && zombie.checkAlert(player))
                    {
                        zombie.alert(player);
                    }
                }
            }
            zombiesInRadius.Clear();
            ZombieManager.getZombiesInRadius(position, sqrRadius, zombiesInRadius);
            for (int j = 0; j < zombiesInRadius.Count; j++)
            {
                Zombie zombie2 = zombiesInRadius[j];
                if (zombie2.isDead || !zombie2.checkAlert(player))
                {
                    continue;
                }
                Vector3 vector = zombie2.transform.position - position;
                if (check(zombie2.transform.forward, vector, sqrRadius, sneak, spotDir, isSpotOn, zombie2.speciality.IsDLVolatile()))
                {
                    Physics.Raycast(zombie2.transform.position + Vector3.up, -vector, out hitInfo, vector.magnitude * 0.95f, RayMasks.BLOCK_VISION);
                    if (!(hitInfo.transform != null))
                    {
                        zombie2.alert(player);
                    }
                }
            }
        }
        animalsInRadius.Clear();
        AnimalManager.getAnimalsInRadius(position, sqrRadius, animalsInRadius);
        for (int k = 0; k < animalsInRadius.Count; k++)
        {
            Animal animal = animalsInRadius[k];
            if (animal.isDead || animal.asset == null)
            {
                continue;
            }
            if (animal.asset.behaviour == EAnimalBehaviour.DEFENSE)
            {
                if (!animal.isFleeing)
                {
                    Vector3 vector2 = animal.transform.position - position;
                    if (!check(animal.transform.forward, vector2, sqrRadius, sneak, spotDir, isSpotOn, isLightSensitive: false))
                    {
                        continue;
                    }
                    Physics.Raycast(animal.transform.position + Vector3.up, -vector2, out hitInfo, vector2.magnitude * 0.95f, RayMasks.BLOCK_VISION);
                    if (hitInfo.transform != null)
                    {
                        continue;
                    }
                }
                animal.alertRunAwayFromPoint(player.transform.position, sendToPack: true);
            }
            else
            {
                if (animal.asset.behaviour != EAnimalBehaviour.OFFENSE || !animal.checkAlert(player))
                {
                    continue;
                }
                Vector3 vector3 = animal.transform.position - position;
                if (check(animal.transform.forward, vector3, sqrRadius, sneak, spotDir, isSpotOn, isLightSensitive: false))
                {
                    Physics.Raycast(animal.transform.position + Vector3.up, -vector3, out hitInfo, vector3.magnitude * 0.95f, RayMasks.BLOCK_VISION);
                    if (!(hitInfo.transform != null))
                    {
                        animal.alertPlayer(player, sendToPack: true);
                    }
                }
            }
        }
    }

    public static void alert(Vector3 position, float radius)
    {
        float sqrRadius = radius * radius;
        if (LevelNavigation.checkNavigation(position))
        {
            zombiesInRadius.Clear();
            ZombieManager.getZombiesInRadius(position, sqrRadius, zombiesInRadius);
            for (int i = 0; i < zombiesInRadius.Count; i++)
            {
                Zombie zombie = zombiesInRadius[i];
                if (!zombie.isDead)
                {
                    zombie.alert(position, isStartling: true);
                }
            }
        }
        animalsInRadius.Clear();
        AnimalManager.getAnimalsInRadius(position, sqrRadius, animalsInRadius);
        for (int j = 0; j < animalsInRadius.Count; j++)
        {
            Animal animal = animalsInRadius[j];
            if (!animal.isDead && animal.asset != null)
            {
                if (animal.asset.behaviour == EAnimalBehaviour.DEFENSE)
                {
                    animal.alertRunAwayFromPoint(position, sendToPack: true);
                }
                else if (animal.asset.behaviour == EAnimalBehaviour.OFFENSE)
                {
                    animal.alertGoToPoint(position, sendToPack: true);
                }
            }
        }
    }

    [Conditional("LOG_ALERTS")]
    private static void LogAlert(string format, params object[] args)
    {
        UnturnedLog.info(format, args);
    }
}
