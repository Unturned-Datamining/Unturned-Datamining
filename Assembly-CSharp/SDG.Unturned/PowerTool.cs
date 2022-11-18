using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PowerTool
{
    public static readonly float MAX_POWER_RANGE = 256f;

    private static List<RegionCoordinate> regionsInRadius = new List<RegionCoordinate>(4);

    private static List<Transform> barricadesInRadius = new List<Transform>();

    private static List<InteractableFire> firesInRadius = new List<InteractableFire>();

    private static List<InteractableOven> ovensInRadius = new List<InteractableOven>();

    private static List<InteractablePower> powerInRadius = new List<InteractablePower>();

    private static List<InteractableGenerator> generatorsInRadius = new List<InteractableGenerator>();

    public static void checkInteractables<T>(Vector3 point, float radius, ushort plant, List<T> interactablesInRadius) where T : Interactable
    {
        float sqrRadius = radius * radius;
        if (plant == ushort.MaxValue)
        {
            regionsInRadius.Clear();
            Regions.getRegionsInRadius(point, radius, regionsInRadius);
            barricadesInRadius.Clear();
            BarricadeManager.getBarricadesInRadius(point, sqrRadius, regionsInRadius, barricadesInRadius);
            ObjectManager.getObjectsInRadius(point, sqrRadius, regionsInRadius, barricadesInRadius);
        }
        else
        {
            barricadesInRadius.Clear();
            BarricadeManager.getBarricadesInRadius(point, sqrRadius, plant, barricadesInRadius);
        }
        for (int i = 0; i < barricadesInRadius.Count; i++)
        {
            T component = barricadesInRadius[i].GetComponent<T>();
            if (!((Object)component == (Object)null))
            {
                interactablesInRadius.Add(component);
            }
        }
    }

    public static void checkInteractables<T>(Vector3 point, float radius, List<T> interactablesInRadius) where T : Interactable
    {
        float sqrRadius = radius * radius;
        regionsInRadius.Clear();
        Regions.getRegionsInRadius(point, radius, regionsInRadius);
        barricadesInRadius.Clear();
        BarricadeManager.getBarricadesInRadius(point, sqrRadius, regionsInRadius, barricadesInRadius);
        BarricadeManager.getBarricadesInRadius(point, sqrRadius, barricadesInRadius);
        for (int i = 0; i < barricadesInRadius.Count; i++)
        {
            T component = barricadesInRadius[i].GetComponent<T>();
            if (!((Object)component == (Object)null))
            {
                interactablesInRadius.Add(component);
            }
        }
    }

    public static bool checkFires(Vector3 point, float radius)
    {
        firesInRadius.Clear();
        checkInteractables(point, radius, firesInRadius);
        for (int i = 0; i < firesInRadius.Count; i++)
        {
            if (firesInRadius[i].isLit)
            {
                return true;
            }
        }
        ovensInRadius.Clear();
        checkInteractables(point, radius, ovensInRadius);
        for (int j = 0; j < ovensInRadius.Count; j++)
        {
            if (ovensInRadius[j].isWired && ovensInRadius[j].isLit)
            {
                return true;
            }
        }
        return false;
    }

    public static List<InteractableGenerator> checkGenerators(Vector3 point, float radius, ushort plant)
    {
        generatorsInRadius.Clear();
        checkInteractables(point, radius, plant, generatorsInRadius);
        return generatorsInRadius;
    }

    public static List<InteractablePower> checkPower(Vector3 point, float radius, ushort plant)
    {
        powerInRadius.Clear();
        checkInteractables(point, radius, plant, powerInRadius);
        return powerInRadius;
    }
}
