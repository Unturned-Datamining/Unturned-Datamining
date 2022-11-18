using System;
using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace SDG.Unturned;

public static class UnturnedPlayerLoop
{
    private static Type[] DISABLED_SYSTEMS = new Type[19]
    {
        typeof(EarlyUpdate.AnalyticsCoreStatsUpdate),
        typeof(EarlyUpdate.UpdateKinect),
        typeof(EarlyUpdate.XRUpdate),
        typeof(FixedUpdate.DirectorFixedSampleTime),
        typeof(FixedUpdate.DirectorFixedUpdate),
        typeof(FixedUpdate.DirectorFixedUpdatePostPhysics),
        typeof(FixedUpdate.Physics2DFixedUpdate),
        typeof(FixedUpdate.XRFixedUpdate),
        typeof(Initialization.XREarlyUpdate),
        typeof(PostLateUpdate.DirectorLateUpdate),
        typeof(PostLateUpdate.DirectorRenderImage),
        typeof(PostLateUpdate.XRPostPresent),
        typeof(PreLateUpdate.UNetUpdate),
        typeof(PreLateUpdate.UpdateMasterServerInterface),
        typeof(PreLateUpdate.UpdateNetworkManager),
        typeof(PreUpdate.AIUpdate),
        typeof(PreUpdate.SendMouseEvents),
        typeof(PreUpdate.UpdateVideo),
        typeof(Update.DirectorUpdate)
    };

    public static void initialize()
    {
        PlayerLoopSystem system = PlayerLoop.GetDefaultPlayerLoop();
        recursiveTidyPlayerLoop(ref system);
        PlayerLoop.SetPlayerLoop(system);
    }

    private static void recursiveTidyPlayerLoop(ref PlayerLoopSystem system)
    {
        int num = system.subSystemList.Length;
        List<PlayerLoopSystem> list = new List<PlayerLoopSystem>(num);
        for (int i = 0; i < num; i++)
        {
            PlayerLoopSystem system2 = system.subSystemList[i];
            if (!isTypeDisabled(system2.type))
            {
                if (system2.subSystemList != null && system2.subSystemList.Length != 0)
                {
                    recursiveTidyPlayerLoop(ref system2);
                }
                list.Add(system2);
            }
        }
        system.subSystemList = list.ToArray();
    }

    private static bool isTypeDisabled(Type type)
    {
        Type[] dISABLED_SYSTEMS = DISABLED_SYSTEMS;
        for (int i = 0; i < dISABLED_SYSTEMS.Length; i++)
        {
            if (dISABLED_SYSTEMS[i] == type)
            {
                return true;
            }
        }
        return false;
    }
}
