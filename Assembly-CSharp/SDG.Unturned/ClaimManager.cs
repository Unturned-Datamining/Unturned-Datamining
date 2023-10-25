using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class ClaimManager : MonoBehaviour
{
    private static List<ClaimBubble> bubbles;

    private static List<ClaimPlant> plants;

    public static bool checkCanBuild(Vector3 point, CSteamID owner, CSteamID group, bool isClaim)
    {
        for (int i = 0; i < bubbles.Count; i++)
        {
            ClaimBubble claimBubble = bubbles[i];
            if ((isClaim ? ((claimBubble.origin - point).sqrMagnitude < 4f * claimBubble.sqrRadius) : ((claimBubble.origin - point).sqrMagnitude < claimBubble.sqrRadius)) && (Dedicator.IsDedicatedServer ? (!OwnershipTool.checkToggle(owner, claimBubble.owner, group, claimBubble.group)) : (!claimBubble.hasOwnership)))
            {
                return false;
            }
        }
        return true;
    }

    /// <param name="isClaim">True if it's a new claim flag.</param>
    public static bool canBuildOnVehicle(Transform vehicle, CSteamID owner, CSteamID group)
    {
        foreach (ClaimPlant plant in plants)
        {
            if (plant.parent != vehicle)
            {
                continue;
            }
            if (Dedicator.IsDedicatedServer)
            {
                if (!OwnershipTool.checkToggle(owner, plant.owner, group, plant.group))
                {
                    return false;
                }
            }
            else if (!plant.hasOwnership)
            {
                return false;
            }
        }
        return true;
    }

    public static ClaimBubble registerBubble(Vector3 origin, float radius, ulong owner, ulong group)
    {
        ClaimBubble claimBubble = new ClaimBubble(origin, radius * radius, owner, group);
        bubbles.Add(claimBubble);
        return claimBubble;
    }

    public static void deregisterBubble(ClaimBubble bubble)
    {
        bubbles.Remove(bubble);
    }

    public static ClaimPlant registerPlant(Transform parent, ulong owner, ulong group)
    {
        ClaimPlant claimPlant = new ClaimPlant(parent, owner, group);
        plants.Add(claimPlant);
        return claimPlant;
    }

    public static void deregisterPlant(ClaimPlant plant)
    {
        plants.Remove(plant);
    }

    private void onLevelLoaded(int level)
    {
        bubbles = new List<ClaimBubble>();
        plants = new List<ClaimPlant>();
    }

    private void Start()
    {
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onLevelLoaded));
    }
}
