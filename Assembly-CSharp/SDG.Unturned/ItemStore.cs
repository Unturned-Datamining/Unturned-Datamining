using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SDG.Unturned;

internal abstract class ItemStore
{
    public struct Listing
    {
        public int itemdefid;

        public bool isNew;

        public ulong currentPrice;

        public ulong basePrice;
    }

    public struct CartEntry
    {
        public int itemdefid;

        public int quantity;
    }

    public enum EPurchaseResult
    {
        UnableToInitialize,
        Denied
    }

    public static readonly Color32 PremiumColor = new Color32(100, 200, 25, byte.MaxValue);

    private static ItemStore instance = new SteamItemStore();

    protected NumberFormatInfo numberFormatInfo;

    protected Listing[] listings;

    protected int[] newListingIndices;

    protected int[] featuredListingIndices;

    protected int[] discountedListingIndices;

    protected int[] exludedListingIndices;

    protected List<CartEntry> itemsInCart = new List<CartEntry>();

    public bool IsCartEmpty => itemsInCart.IsEmpty();

    public bool HasNewListings
    {
        get
        {
            if (newListingIndices != null)
            {
                return newListingIndices.Length != 0;
            }
            return false;
        }
    }

    public bool HasFeaturedListings
    {
        get
        {
            if (featuredListingIndices != null)
            {
                return featuredListingIndices.Length != 0;
            }
            return false;
        }
    }

    public bool HasDiscountedListings
    {
        get
        {
            if (discountedListingIndices != null)
            {
                return discountedListingIndices.Length != 0;
            }
            return false;
        }
    }

    public abstract event System.Action OnPricesReceived;

    public abstract event Action<EPurchaseResult> OnPurchaseResult;

    public event System.Action OnCartChanged;

    public static ItemStore Get()
    {
        return instance;
    }

    public IEnumerable<CartEntry> GetCart()
    {
        return itemsInCart;
    }

    public abstract void ViewItem(int itemdefid);

    public abstract void ViewNewItems();

    public abstract void ViewStore();

    public bool FindListing(int itemdefid, out Listing listing)
    {
        Listing[] array = listings;
        for (int i = 0; i < array.Length; i++)
        {
            Listing listing2 = array[i];
            if (listing2.itemdefid == itemdefid)
            {
                listing = listing2;
                return true;
            }
        }
        listing = default(Listing);
        return false;
    }

    public int GetQuantityInCart(int itemdefid)
    {
        foreach (CartEntry item in itemsInCart)
        {
            if (item.itemdefid == itemdefid)
            {
                return item.quantity;
            }
        }
        return 0;
    }

    public void SetQuantityInCart(int itemdefid, int quantity)
    {
        for (int i = 0; i < itemsInCart.Count; i++)
        {
            CartEntry value = itemsInCart[i];
            if (value.itemdefid == itemdefid)
            {
                if (quantity > 0)
                {
                    value.quantity = quantity;
                    itemsInCart[i] = value;
                }
                else
                {
                    itemsInCart.RemoveAt(i);
                }
                this.OnCartChanged?.Invoke();
                return;
            }
        }
        if (quantity > 0)
        {
            CartEntry item = default(CartEntry);
            item.itemdefid = itemdefid;
            item.quantity = quantity;
            itemsInCart.Add(item);
            this.OnCartChanged?.Invoke();
        }
    }

    public abstract void RequestPrices();

    public abstract void StartPurchase();

    public Listing[] GetListings()
    {
        return listings;
    }

    public int[] GetNewListingIndices()
    {
        return newListingIndices;
    }

    public int[] GetFeaturedListingIndices()
    {
        return featuredListingIndices;
    }

    public int[] GetDiscountedListingIndices()
    {
        return discountedListingIndices;
    }

    public int[] GetExcludedListingIndices()
    {
        return exludedListingIndices;
    }

    public string FormatPrice(ulong price)
    {
        return ((double)price / 100.0).ToString("C", numberFormatInfo);
    }

    public string FormatDiscount(ulong currentPrice, ulong basePrice)
    {
        return ((double)currentPrice / (double)basePrice - 1.0).ToString("P0", numberFormatInfo);
    }

    protected int FindListingIndex(int itemdefid)
    {
        for (int i = 0; i < listings.Length; i++)
        {
            if (listings[i].itemdefid == itemdefid)
            {
                return i;
            }
        }
        return -1;
    }

    protected void CheckNewItems()
    {
        int[] new_Items = Provider.statusData.Stockpile.New_Items;
        if (new_Items != null && new_Items.Length != 0)
        {
            if (ItemStoreSavedata.IsNowWithinSpan())
            {
                List<int> list = new List<int>(new_Items.Length);
                int[] array = new_Items;
                foreach (int itemdefid in array)
                {
                    int num = FindListingIndex(itemdefid);
                    if (num >= 0)
                    {
                        listings[num].isNew = true;
                        list.Add(num);
                    }
                }
                newListingIndices = list.ToArray();
            }
            else
            {
                newListingIndices = null;
            }
        }
        else
        {
            newListingIndices = null;
        }
    }

    protected void CheckFeaturedItems()
    {
        int[] featured_Items = Provider.statusData.Stockpile.Featured_Items;
        if (featured_Items != null && featured_Items.Length != 0)
        {
            List<int> list = new List<int>(featured_Items.Length);
            int[] array = featured_Items;
            foreach (int itemdefid in array)
            {
                int num = FindListingIndex(itemdefid);
                if (num >= 0)
                {
                    list.Add(num);
                }
            }
            featuredListingIndices = list.ToArray();
        }
        else
        {
            featuredListingIndices = null;
        }
    }

    protected void CheckExcludedItems()
    {
        int[] exclude_Items_From_Highlight = Provider.statusData.Stockpile.Exclude_Items_From_Highlight;
        if (exclude_Items_From_Highlight != null && exclude_Items_From_Highlight.Length != 0)
        {
            List<int> list = new List<int>();
            int[] array = exclude_Items_From_Highlight;
            foreach (int itemdefid in array)
            {
                int num = FindListingIndex(itemdefid);
                if (num >= 0)
                {
                    list.Add(num);
                }
            }
            exludedListingIndices = list.ToArray();
        }
        else
        {
            exludedListingIndices = null;
        }
    }
}
