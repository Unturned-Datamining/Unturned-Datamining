using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class BeaconManager : MonoBehaviour
{
    private static List<InteractableBeacon>[] beacons;

    public static BeaconUpdated onBeaconUpdated;

    public static int getParticipants(byte nav)
    {
        int num = 0;
        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer steamPlayer = Provider.clients[i];
            if (!(steamPlayer.player == null) && !(steamPlayer.player.movement == null) && !(steamPlayer.player.life == null) && !steamPlayer.player.life.isDead && steamPlayer.player.movement.nav == nav)
            {
                num++;
            }
        }
        return num;
    }

    public static InteractableBeacon checkBeacon(byte nav)
    {
        if (beacons[nav].Count > 0)
        {
            return beacons[nav][0];
        }
        return null;
    }

    public static void registerBeacon(byte nav, InteractableBeacon beacon)
    {
        if (LevelNavigation.checkSafe(nav))
        {
            beacons[nav].Add(beacon);
            if (onBeaconUpdated != null)
            {
                onBeaconUpdated(nav, beacons[nav].Count > 0);
            }
        }
    }

    public static void deregisterBeacon(byte nav, InteractableBeacon beacon)
    {
        if (LevelNavigation.checkSafe(nav))
        {
            beacons[nav].Remove(beacon);
            if (onBeaconUpdated != null)
            {
                onBeaconUpdated(nav, beacons[nav].Count > 0);
            }
        }
    }

    private void onLevelLoaded(int level)
    {
        if (LevelNavigation.bounds != null)
        {
            beacons = new List<InteractableBeacon>[LevelNavigation.bounds.Count];
            for (int i = 0; i < beacons.Length; i++)
            {
                beacons[i] = new List<InteractableBeacon>();
            }
        }
    }

    private void Start()
    {
        Level.onPrePreLevelLoaded = (PrePreLevelLoaded)Delegate.Combine(Level.onPrePreLevelLoaded, new PrePreLevelLoaded(onLevelLoaded));
    }
}
