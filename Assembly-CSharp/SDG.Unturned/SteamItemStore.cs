using System;
using System.Collections.Generic;
using System.Globalization;
using Steamworks;

namespace SDG.Unturned;

internal class SteamItemStore : ItemStore
{
    private bool hasPricingInformation;

    private CallResult<SteamInventoryRequestPricesResult_t> requestPricesCallResult;

    private CallResult<SteamInventoryStartPurchaseResult_t> startPurchaseCallResult;

    private Callback<MicroTxnAuthorizationResponse_t> microTxnAuthCallback;

    private bool IsOverlayEnabledForCheckout => SteamUtils.IsOverlayEnabled();

    public override event System.Action OnPricesReceived;

    public override event Action<EPurchaseResult> OnPurchaseResult;

    public override void ViewItem(int itemdefid)
    {
        if (listings != null && listings.Length != 0)
        {
            if (FindListing(itemdefid, out var listing))
            {
                if (IsOverlayEnabledForCheckout)
                {
                    UnturnedLog.info("Item store has listing for itemdefid {0}, using in-game menu", itemdefid);
                    MenuUI.closeAll();
                    ItemStoreDetailsMenu.instance.Open(listing);
                    return;
                }
                UnturnedLog.warn("Would not be able to checkout because Steam overlay is disabled, using browser");
            }
            else
            {
                UnturnedLog.warn("Item store does not have a listing for itemdefid {0}, using browser", itemdefid);
            }
        }
        else
        {
            UnturnedLog.warn("Item store unavailable for itemdefid {0}, using browser", itemdefid);
        }
        AppId_t aPP_ID = Provider.APP_ID;
        Provider.openURL("http://store.steampowered.com/itemstore/" + aPP_ID.ToString() + "/detail/" + itemdefid);
    }

    public override void ViewNewItems()
    {
        if (base.HasNewListings)
        {
            if (IsOverlayEnabledForCheckout)
            {
                MenuUI.closeAll();
                ItemStoreMenu.instance.OpenNewItems();
                return;
            }
            UnturnedLog.warn("Would not be able to checkout because Steam overlay is disabled, using browser");
        }
        else
        {
            UnturnedLog.warn("Item store does not have listings for new items, using browser");
        }
        AppId_t aPP_ID = Provider.APP_ID;
        Provider.openURL("http://store.steampowered.com/itemstore/" + aPP_ID.ToString() + "/browse/?filter=New");
    }

    public override void ViewStore()
    {
        if (listings != null && listings.Length != 0)
        {
            if (IsOverlayEnabledForCheckout)
            {
                MenuUI.closeAll();
                ItemStoreMenu.instance.Open();
                return;
            }
            UnturnedLog.warn("Would not be able to checkout because Steam overlay is disabled, using browser");
        }
        else
        {
            UnturnedLog.warn("Item store unavailable, using browser");
        }
        AppId_t aPP_ID = Provider.APP_ID;
        Provider.openURL("http://store.steampowered.com/itemstore/" + aPP_ID.ToString());
    }

    public override void RequestPrices()
    {
        UnturnedLog.info("Requesting Steam item store prices");
        SteamAPICall_t steamAPICall_t = SteamInventory.RequestPrices();
        if (steamAPICall_t != SteamAPICall_t.Invalid)
        {
            requestPricesCallResult.Set(steamAPICall_t);
        }
        else
        {
            UnturnedLog.info("Steam internal problem requesting item store prices");
        }
    }

    public override void StartPurchase()
    {
        if (base.IsCartEmpty)
        {
            throw new Exception("should not have been called with an empty cart");
        }
        uint count = (uint)itemsInCart.Count;
        UnturnedLog.info("Requesting purchase of {0} item(s)", count);
        SteamItemDef_t[] array = new SteamItemDef_t[count];
        uint[] array2 = new uint[count];
        for (int i = 0; i < count; i++)
        {
            CartEntry cartEntry = itemsInCart[i];
            array[i] = new SteamItemDef_t(cartEntry.itemdefid);
            array2[i] = (uint)cartEntry.quantity;
            UnturnedLog.info("[{0}]: {1} x {2}", i, cartEntry.itemdefid, cartEntry.quantity);
        }
        itemsInCart.Clear();
        SteamAPICall_t steamAPICall_t = SteamInventory.StartPurchase(array, array2, count);
        if (steamAPICall_t != SteamAPICall_t.Invalid)
        {
            startPurchaseCallResult.Set(steamAPICall_t);
            return;
        }
        UnturnedLog.info("Start purchase invalid input");
        OnPurchaseResult?.Invoke(EPurchaseResult.UnableToInitialize);
    }

    private NumberFormatInfo GetCurrencyFormatInfo(string threeLetterCode)
    {
        try
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            if (currentCulture != null && string.Equals(new RegionInfo(currentCulture.LCID).ISOCurrencySymbol, threeLetterCode, StringComparison.OrdinalIgnoreCase))
            {
                UnturnedLog.info("Item store using current culture {0} for Steam currency code {1}", currentCulture.DisplayName, threeLetterCode);
                return currentCulture.NumberFormat;
            }
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo cultureInfo in cultures)
            {
                if (string.Equals(new RegionInfo(cultureInfo.LCID).ISOCurrencySymbol, threeLetterCode, StringComparison.OrdinalIgnoreCase))
                {
                    UnturnedLog.info("Item store using fallback culture {0} for Steam currency code {1}", cultureInfo.DisplayName, threeLetterCode);
                    return cultureInfo.NumberFormat;
                }
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception trying to find region for Steam currency code {0}:", threeLetterCode);
        }
        return null;
    }

    private void OnRequestPricesResultReady(SteamInventoryRequestPricesResult_t result, bool ioFailure)
    {
        if (ioFailure || result.m_result != EResult.k_EResultOK)
        {
            UnturnedLog.error("Request prices result: {0} I/O Failure: {1}", result.m_result, ioFailure);
            return;
        }
        numberFormatInfo = GetCurrencyFormatInfo(result.m_rgchCurrency);
        if (numberFormatInfo == null)
        {
            UnturnedLog.error("Unable to find currency format info for Steam currency code: {0}", result.m_rgchCurrency);
            return;
        }
        uint numItemsWithPrices = SteamInventory.GetNumItemsWithPrices();
        if (numItemsWithPrices < 1)
        {
            UnturnedLog.error("Steam returned zero items with prices");
            return;
        }
        SteamItemDef_t[] array = new SteamItemDef_t[numItemsWithPrices];
        ulong[] array2 = new ulong[numItemsWithPrices];
        ulong[] array3 = new ulong[numItemsWithPrices];
        if (!SteamInventory.GetItemsWithPrices(array, array2, array3, numItemsWithPrices))
        {
            UnturnedLog.error("Unable to get items with prices");
            return;
        }
        List<Listing> list = new List<Listing>((int)numItemsWithPrices);
        List<int> list2 = new List<int>();
        for (uint num = 0u; num < numItemsWithPrices; num++)
        {
            int steamItemDef = array[num].m_SteamItemDef;
            if (!Provider.provider.economyService.IsItemKnown(steamItemDef))
            {
                UnturnedLog.warn("Item store missing details for itemdefid {0}", steamItemDef);
                continue;
            }
            if (Provider.provider.economyService.isItemHiddenByCountryRestrictions(steamItemDef))
            {
                UnturnedLog.info("Item store hiding \"{0}\" due to country restrictions", Provider.provider.economyService.getInventoryName(steamItemDef));
                continue;
            }
            Listing item = default(Listing);
            item.itemdefid = steamItemDef;
            item.currentPrice = array2[num];
            item.basePrice = array3[num];
            int count = list.Count;
            list.Add(item);
            if (item.currentPrice < item.basePrice)
            {
                list2.Add(count);
            }
        }
        if (list.Count < 1)
        {
            UnturnedLog.error("Item store has no valid listings");
            return;
        }
        listings = list.ToArray();
        discountedListingIndices = list2.ToArray();
        hasPricingInformation = true;
        int num2 = (base.HasDiscountedListings ? GetDiscountedListingIndices().Length : 0);
        int num3 = GetListings().Length;
        UnturnedLog.info($"Received Steam item store prices - Discounted: {num2} All: {num3}");
        if (LiveConfig.WasPopulated)
        {
            OnLiveConfigRefreshed();
        }
    }

    private void OnStartPurchaseResultReady(SteamInventoryStartPurchaseResult_t result, bool ioFailure)
    {
        if (ioFailure || result.m_result != EResult.k_EResultOK)
        {
            UnturnedLog.error("Start purchase result: {0} I/O Failure: {1}", result.m_result, ioFailure);
            OnPurchaseResult?.Invoke(EPurchaseResult.UnableToInitialize);
        }
        else
        {
            UnturnedLog.info("Start purchase Order ID: {0} Transaction ID: {1}", result.m_ulOrderID, result.m_ulTransID);
            Provider.provider.economyService.isExpectingPurchaseResult = true;
        }
    }

    private void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t responseData)
    {
        if (responseData.m_unAppID == Provider.APP_ID.m_AppId)
        {
            if (responseData.m_bAuthorized > 0)
            {
                UnturnedLog.info("Purchase authorized Order ID: {0}", responseData.m_ulOrderID);
                return;
            }
            Provider.provider.economyService.isExpectingPurchaseResult = false;
            UnturnedLog.info("Purchase denied Order ID: {0}", responseData.m_ulOrderID);
            OnPurchaseResult?.Invoke(EPurchaseResult.Denied);
        }
    }

    private void OnLiveConfigRefreshed()
    {
        if (hasPricingInformation)
        {
            RefreshNewItems();
            RefreshFeaturedItems();
            RefreshExcludedItems();
            int num = (base.HasNewListings ? GetNewListingIndices().Length : 0);
            int num2 = (base.HasFeaturedListings ? GetFeaturedListingIndices().Length : 0);
            UnturnedLog.info($"Received Steam item store live config - New: {num} Featured: {num2}");
            OnPricesReceived?.Invoke();
        }
    }

    public SteamItemStore()
    {
        requestPricesCallResult = CallResult<SteamInventoryRequestPricesResult_t>.Create(OnRequestPricesResultReady);
        startPurchaseCallResult = CallResult<SteamInventoryStartPurchaseResult_t>.Create(OnStartPurchaseResultReady);
        microTxnAuthCallback = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
        LiveConfig.OnRefreshed += OnLiveConfigRefreshed;
    }
}
