using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SDG.Unturned;

internal class PlaceableInstantiationManager
{
    public static List<PlaceableInstantiationParameters> pendingInstantiations = new List<PlaceableInstantiationParameters>();

    private static Stopwatch instantiationTimer = new Stopwatch();

    /// <summary>
    /// Instantiate at least this many items per frame even if we exceed our time budget.
    /// </summary>
    private const int MIN_INSTANTIATIONS_PER_FRAME = 10;

    /// <summary>
    /// Not ideal, but there was a problem because onLevelLoaded was not resetting these after disconnecting.
    /// </summary>
    public static void ClearNetworkStuff()
    {
        pendingInstantiations = new List<PlaceableInstantiationParameters>();
    }

    public static void CancelInstantiationsInRegion(object region, uint netIdBlockSize)
    {
        for (int num = pendingInstantiations.Count - 1; num >= 0; num--)
        {
            if (pendingInstantiations[num].region == region)
            {
                NetInvocationDeferralRegistry.Cancel(pendingInstantiations[num].netId, netIdBlockSize);
                pendingInstantiations.RemoveAt(num);
            }
        }
    }

    public static void CancelInstantiationByNetId(NetId netId, uint netIdBlockSize)
    {
        for (int num = pendingInstantiations.Count - 1; num >= 0; num--)
        {
            if (pendingInstantiations[num].netId == netId)
            {
                NetInvocationDeferralRegistry.Cancel(netId, netIdBlockSize);
                pendingInstantiations.RemoveAt(num);
                break;
            }
        }
    }

    public static void AddInstantiation(ref PlaceableInstantiationParameters instantiation)
    {
        pendingInstantiations.Insert(pendingInstantiations.FindInsertionIndex(instantiation), instantiation);
    }

    public static void ProcessPendingInstantiations()
    {
        if (pendingInstantiations == null || pendingInstantiations.Count < 1)
        {
            return;
        }
        instantiationTimer.Restart();
        int num = 0;
        do
        {
            PlaceableInstantiationParameters instantiation = pendingInstantiations[num];
            try
            {
                switch (instantiation.type)
                {
                case EPlaceableInstantiationType.Barricade:
                    BarricadeManager.HandleInstantiation(ref instantiation);
                    break;
                case EPlaceableInstantiationType.Structure:
                    StructureManager.HandleInstantiation(ref instantiation);
                    break;
                }
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, $"Caught exception handling placeable instantiation: {instantiation}");
            }
            num++;
        }
        while (num < pendingInstantiations.Count && (instantiationTimer.ElapsedMilliseconds < 2 || num < 10));
        pendingInstantiations.RemoveRange(0, num);
        instantiationTimer.Stop();
    }
}
