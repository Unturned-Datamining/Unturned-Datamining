using System;
using System.Collections.Generic;
using SDG.Provider.Services;
using SDG.Provider.Services.Economy;
using SDG.Unturned;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy;

public class SteamworksEconomyService : Service, IEconomyService, IService
{
    private List<SteamworksEconomyRequestHandle> steamworksEconomyRequestHandles;

    private Callback<SteamInventoryResultReady_t> steamInventoryResultReady;

    public bool canOpenInventory => true;

    public IEconomyRequestHandle requestInventory(EconomyRequestReadyCallback inventoryRequestReadyCallback)
    {
        SteamInventory.GetAllItems(out var pResultHandle);
        return addInventoryRequestHandle(pResultHandle, inventoryRequestReadyCallback);
    }

    public IEconomyRequestHandle requestPromo(EconomyRequestReadyCallback inventoryRequestReadyCallback)
    {
        SteamInventory.GrantPromoItems(out var pResultHandle);
        return addInventoryRequestHandle(pResultHandle, inventoryRequestReadyCallback);
    }

    public IEconomyRequestHandle exchangeItems(IEconomyItemInstance[] inputItemInstanceIDs, uint[] inputItemQuantities, IEconomyItemDefinition[] outputItemDefinitionIDs, uint[] outputItemQuantities, EconomyRequestReadyCallback inventoryRequestReadyCallback)
    {
        if (inputItemInstanceIDs.Length != inputItemQuantities.Length)
        {
            throw new ArgumentException("Input item arrays need to be the same length.", "inputItemQuantities");
        }
        if (outputItemDefinitionIDs.Length != outputItemQuantities.Length)
        {
            throw new ArgumentException("Output item arrays need to be the same length.", "outputItemQuantities");
        }
        SteamItemInstanceID_t[] array = new SteamItemInstanceID_t[inputItemInstanceIDs.Length];
        for (int i = 0; i < inputItemInstanceIDs.Length; i++)
        {
            SteamworksEconomyItemInstance steamworksEconomyItemInstance = (SteamworksEconomyItemInstance)inputItemInstanceIDs[i];
            array[i] = steamworksEconomyItemInstance.steamItemInstanceID;
        }
        SteamItemDef_t[] array2 = new SteamItemDef_t[outputItemDefinitionIDs.Length];
        for (int j = 0; j < outputItemDefinitionIDs.Length; j++)
        {
            SteamworksEconomyItemDefinition steamworksEconomyItemDefinition = (SteamworksEconomyItemDefinition)outputItemDefinitionIDs[j];
            array2[j] = steamworksEconomyItemDefinition.steamItemDef;
        }
        SteamInventory.ExchangeItems(out var pResultHandle, array2, outputItemQuantities, (uint)array2.Length, array, inputItemQuantities, (uint)array.Length);
        return addInventoryRequestHandle(pResultHandle, inventoryRequestReadyCallback);
    }

    public void open(ulong id)
    {
        SDG.Unturned.Provider.openURL("http://steamcommunity.com/profiles/" + SteamUser.GetSteamID().ToString() + "/inventory/?sellOnLoad=1#" + SteamUtils.GetAppID().ToString() + "_2_" + id);
    }

    private SteamworksEconomyRequestHandle findSteamworksEconomyRequestHandles(SteamInventoryResult_t steamInventoryResult)
    {
        return steamworksEconomyRequestHandles.Find((SteamworksEconomyRequestHandle handle) => handle.steamInventoryResult == steamInventoryResult);
    }

    private IEconomyRequestHandle addInventoryRequestHandle(SteamInventoryResult_t steamInventoryResult, EconomyRequestReadyCallback inventoryRequestReadyCallback)
    {
        SteamworksEconomyRequestHandle steamworksEconomyRequestHandle = new SteamworksEconomyRequestHandle(steamInventoryResult, inventoryRequestReadyCallback);
        steamworksEconomyRequestHandles.Add(steamworksEconomyRequestHandle);
        return steamworksEconomyRequestHandle;
    }

    private IEconomyRequestResult createInventoryRequestResult(SteamInventoryResult_t steamInventoryResult)
    {
        uint punOutItemsArraySize = 0u;
        SteamworksEconomyItem[] array2;
        if (SteamGameServerInventory.GetResultItems(steamInventoryResult, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
        {
            SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
            SteamGameServerInventory.GetResultItems(steamInventoryResult, array, ref punOutItemsArraySize);
            array2 = new SteamworksEconomyItem[punOutItemsArraySize];
            for (uint num = 0u; num < punOutItemsArraySize; num++)
            {
                SteamworksEconomyItem steamworksEconomyItem = new SteamworksEconomyItem(array[num]);
                array2[num] = steamworksEconomyItem;
            }
        }
        else
        {
            array2 = new SteamworksEconomyItem[0];
        }
        IEconomyItem[] newItems = array2;
        return new EconomyRequestResult(EEconomyRequestState.SUCCESS, newItems);
    }

    private void onSteamInventoryResultReady(SteamInventoryResultReady_t callback)
    {
        SteamworksEconomyRequestHandle steamworksEconomyRequestHandle = findSteamworksEconomyRequestHandles(callback.m_handle);
        if (steamworksEconomyRequestHandle != null)
        {
            IEconomyRequestResult inventoryRequestResult = createInventoryRequestResult(steamworksEconomyRequestHandle.steamInventoryResult);
            steamworksEconomyRequestHandle.triggerInventoryRequestReadyCallback(inventoryRequestResult);
            SteamInventory.DestroyResult(steamworksEconomyRequestHandle.steamInventoryResult);
        }
    }

    public SteamworksEconomyService()
    {
        steamworksEconomyRequestHandles = new List<SteamworksEconomyRequestHandle>();
        steamInventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(onSteamInventoryResultReady);
    }
}
