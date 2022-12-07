using System;
using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace SDG.Unturned;

public static class UnturnedPlayerLoop
{
    public static void initialize()
    {
        HashSet<Type> disabledSystems = new HashSet<Type>
        {
            typeof(EarlyUpdate.AnalyticsCoreStatsUpdate),
            typeof(EarlyUpdate.ARCoreUpdate),
            typeof(EarlyUpdate.DeliverIosPlatformEvents),
            typeof(EarlyUpdate.UpdateKinect),
            typeof(EarlyUpdate.XRUpdate),
            typeof(FixedUpdate.DirectorFixedSampleTime),
            typeof(FixedUpdate.DirectorFixedUpdate),
            typeof(FixedUpdate.DirectorFixedUpdatePostPhysics),
            typeof(FixedUpdate.NewInputFixedUpdate),
            typeof(FixedUpdate.Physics2DFixedUpdate),
            typeof(FixedUpdate.XRFixedUpdate),
            typeof(Initialization.DirectorSampleTime),
            typeof(Initialization.XREarlyUpdate),
            typeof(PostLateUpdate.DirectorLateUpdate),
            typeof(PostLateUpdate.DirectorRenderImage),
            typeof(PostLateUpdate.EnlightenRuntimeUpdate),
            typeof(PostLateUpdate.ExecuteGameCenterCallbacks),
            typeof(PostLateUpdate.UpdateLightProbeProxyVolumes),
            typeof(PostLateUpdate.UpdateSubstance),
            typeof(PostLateUpdate.XRPostLateUpdate),
            typeof(PostLateUpdate.XRPostPresent),
            typeof(PostLateUpdate.XRPreEndFrame),
            typeof(PreLateUpdate.AIUpdatePostScript),
            typeof(PreLateUpdate.DirectorDeferredEvaluate),
            typeof(PreLateUpdate.DirectorUpdateAnimationBegin),
            typeof(PreLateUpdate.DirectorUpdateAnimationEnd),
            typeof(PreLateUpdate.Physics2DLateUpdate),
            typeof(PreLateUpdate.UNetUpdate),
            typeof(PreLateUpdate.UpdateMasterServerInterface),
            typeof(PreLateUpdate.UpdateNetworkManager),
            typeof(PreUpdate.AIUpdate),
            typeof(PreUpdate.NewInputUpdate),
            typeof(PreUpdate.Physics2DUpdate),
            typeof(PreUpdate.SendMouseEvents),
            typeof(Update.DirectorUpdate)
        };
        PlayerLoopSystem system = PlayerLoop.GetDefaultPlayerLoop();
        recursiveTidyPlayerLoop(disabledSystems, ref system);
        PlayerLoop.SetPlayerLoop(system);
    }

    private static void recursiveTidyPlayerLoop(HashSet<Type> disabledSystems, ref PlayerLoopSystem system)
    {
        int num = system.subSystemList.Length;
        List<PlayerLoopSystem> list = new List<PlayerLoopSystem>(num);
        for (int i = 0; i < num; i++)
        {
            PlayerLoopSystem system2 = system.subSystemList[i];
            if (!disabledSystems.Contains(system2.type))
            {
                if (system2.subSystemList != null && system2.subSystemList.Length != 0)
                {
                    recursiveTidyPlayerLoop(disabledSystems, ref system2);
                }
                list.Add(system2);
            }
        }
        system.subSystemList = list.ToArray();
    }
}
