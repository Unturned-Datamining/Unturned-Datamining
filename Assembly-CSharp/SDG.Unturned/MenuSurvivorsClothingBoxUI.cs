using System;
using System.Collections.Generic;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsClothingBoxUI
{
    private struct BoxEntry
    {
        public int id;

        public EItemRarity rarity;

        public float probability;
    }

    private class BoxEntryComparer : Comparer<BoxEntry>
    {
        public override int Compare(BoxEntry x, BoxEntry y)
        {
            int num = x.rarity.CompareTo(y.rarity);
            if (num == 0)
            {
                string inventoryName = Provider.provider.economyService.getInventoryName(x.id);
                string inventoryName2 = Provider.provider.economyService.getInventoryName(y.id);
                return -inventoryName.CompareTo(inventoryName2);
            }
            return -num;
        }
    }

    private static Dictionary<EItemRarity, float> qualityRarities = new Dictionary<EItemRarity, float>
    {
        {
            EItemRarity.RARE,
            0.75f
        },
        {
            EItemRarity.EPIC,
            0.2f
        },
        {
            EItemRarity.LEGENDARY,
            0.05f
        },
        {
            EItemRarity.MYTHICAL,
            0.03f
        }
    };

    private static readonly float BONUS_ITEM_RARITY = 0.1f;

    private static Bundle icons;

    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    public static bool isUnboxing;

    private static float lastUnbox;

    private static float lastAngle;

    private static float angle;

    private static int lastRotation;

    private static int rotation;

    private static int target;

    private static int item;

    private static ulong instance;

    private static List<SteamItemDetails_t> unboxedItems;

    private static bool didUnboxMythical;

    private static List<BoxEntry> boxEntries;

    private static int numBoxEntries;

    private static ItemBoxAsset boxAsset;

    private static ItemKeyAsset keyAsset;

    private static float size;

    private static ISleekConstraintFrame inventory;

    private static ISleekBox finalBox;

    private static SleekInventory boxButton;

    private static SleekButtonIcon keyButton;

    private static SleekButtonIcon unboxButton;

    private static ISleekBox disabledBox;

    private static ISleekLabel rareLabel;

    private static ISleekLabel epicLabel;

    private static ISleekLabel legendaryLabel;

    private static ISleekLabel mythicalLabel;

    private static ISleekLabel equalizedLabel;

    private static ISleekLabel bonusLabel;

    private static SleekInventory[] dropButtons;

    private static string formatQualityRarity(EItemRarity rarity)
    {
        return (qualityRarities[rarity] * 100f).ToString("0.0");
    }

    public static void skipAnimation()
    {
        if (!isUnboxing || target == -1)
        {
            return;
        }
        if (rotation == target)
        {
            lastAngle -= 1f;
            return;
        }
        float num = (float)Math.PI / 2f;
        float num2 = (float)target / (float)numBoxEntries * (float)Math.PI * 2f;
        if (angle > num2 - num)
        {
            rotation = target;
            lastAngle -= 1f;
        }
        else
        {
            angle = num2 - UnityEngine.Random.Range(num / 2f, num);
        }
    }

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static void viewItem(int newItem, ushort newQuantity, ulong newInstance)
    {
        item = newItem;
        instance = newInstance;
        unboxedItems = null;
        didUnboxMythical = false;
        angle = 0f;
        lastRotation = 0;
        rotation = 0;
        target = -1;
        disabledBox.isVisible = false;
        keyButton.isVisible = true;
        unboxButton.isVisible = true;
        boxButton.updateInventory(instance, item, newQuantity, isClickable: false, isLarge: true);
        boxAsset = Assets.find<ItemBoxAsset>(Provider.provider.economyService.getInventoryItemGuid(item));
        if (boxAsset != null)
        {
            organizeBoxEntries();
            synchronizeTotalProbabilities();
            string key = null;
            string key2 = null;
            switch (boxAsset.itemOrigin)
            {
            case EBoxItemOrigin.Unbox:
                key = "Unbox_Text";
                key2 = "Unbox_Tooltip";
                break;
            case EBoxItemOrigin.Unwrap:
                key = "Unwrap_Text";
                key2 = "Unwrap_Tooltip";
                break;
            }
            if (!Provider.provider.economyService.doesCountryAllowRandomItems)
            {
                if (Provider.provider.economyService.hasCountryDetails)
                {
                    disabledBox.isVisible = true;
                    disabledBox.text = localization.format("Region_Disabled", Provider.provider.economyService.getCountryWarningId());
                }
                else
                {
                    disabledBox.isVisible = false;
                }
                unboxButton.isVisible = false;
                keyButton.isVisible = false;
            }
            else if (boxAsset.destroy == 0)
            {
                keyButton.isVisible = false;
                unboxButton.icon = null;
                unboxButton.positionOffset_X = 0;
                unboxButton.positionScale_X = 0.3f;
                unboxButton.sizeOffset_X = 0;
                unboxButton.sizeScale_X = 0.4f;
                unboxButton.text = localization.format(key);
                unboxButton.tooltip = localization.format(key2);
                unboxButton.isVisible = true;
                keyAsset = null;
            }
            else
            {
                keyButton.isVisible = true;
                unboxButton.icon = icons.load<Texture2D>("Unbox");
                unboxButton.positionOffset_X = 5;
                unboxButton.positionScale_X = 0.5f;
                unboxButton.sizeOffset_X = -5;
                unboxButton.sizeScale_X = 0.2f;
                unboxButton.text = localization.format(key);
                unboxButton.tooltip = localization.format(key2);
                unboxButton.isVisible = true;
                keyAsset = Assets.find<ItemKeyAsset>(Provider.provider.economyService.getInventoryItemGuid(boxAsset.destroy));
                if (keyAsset != null)
                {
                    keyButton.icon = Provider.provider.economyService.LoadItemIcon(boxAsset.destroy, large: false);
                }
            }
            size = (float)Math.PI * 2f / (float)numBoxEntries / 2.75f;
            finalBox.positionScale_Y = 0.5f - size / 2f;
            finalBox.sizeScale_X = size;
            finalBox.sizeScale_Y = size;
            if (dropButtons != null)
            {
                for (int i = 0; i < dropButtons.Length; i++)
                {
                    inventory.RemoveChild(dropButtons[i]);
                }
            }
            dropButtons = new SleekInventory[numBoxEntries];
            for (int j = 0; j < numBoxEntries; j++)
            {
                BoxEntry boxEntry = boxEntries[j];
                float num = (float)Math.PI * 2f * (float)j / (float)numBoxEntries + (float)Math.PI;
                SleekInventory sleekInventory = new SleekInventory();
                sleekInventory.positionScale_X = 0.5f + Mathf.Cos(0f - num) * (0.5f - size / 2f) - size / 2f;
                sleekInventory.positionScale_Y = 0.5f + Mathf.Sin(0f - num) * (0.5f - size / 2f) - size / 2f;
                sleekInventory.sizeScale_X = size;
                sleekInventory.sizeScale_Y = size;
                if (boxEntry.probability > -0.5f)
                {
                    sleekInventory.extraTooltip = boxEntry.probability.ToString("P");
                }
                sleekInventory.updateInventory(0uL, boxEntry.id, 1, isClickable: false, isLarge: false);
                inventory.AddChild(sleekInventory);
                dropButtons[j] = sleekInventory;
            }
        }
        Color inventoryColor = Provider.provider.economyService.getInventoryColor(item);
        keyButton.backgroundColor = SleekColor.BackgroundIfLight(inventoryColor);
        keyButton.textColor = inventoryColor;
        unboxButton.backgroundColor = keyButton.backgroundColor;
        unboxButton.textColor = inventoryColor;
    }

    private static void synchronizeTotalProbabilities()
    {
        rareLabel.isVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Original;
        epicLabel.isVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Original;
        legendaryLabel.isVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Original;
        equalizedLabel.isVisible = boxAsset.probabilityModel == EBoxProbabilityModel.Equalized;
        bonusLabel.isVisible = boxAsset.containsBonusItems;
    }

    private static void organizeBoxEntries()
    {
        int[] drops = boxAsset.drops;
        int num = (numBoxEntries = drops.Length);
        boxEntries = new List<BoxEntry>(numBoxEntries);
        Dictionary<EItemRarity, int> dictionary = new Dictionary<EItemRarity, int>
        {
            {
                EItemRarity.RARE,
                0
            },
            {
                EItemRarity.EPIC,
                0
            },
            {
                EItemRarity.LEGENDARY,
                0
            },
            {
                EItemRarity.MYTHICAL,
                0
            }
        };
        for (int i = 0; i < num; i++)
        {
            int num2 = drops[i];
            EItemRarity eItemRarity = ((num2 >= 0) ? Provider.provider.economyService.getGameRarity(num2) : EItemRarity.MYTHICAL);
            dictionary[eItemRarity]++;
            BoxEntry boxEntry = default(BoxEntry);
            boxEntry.id = num2;
            boxEntry.rarity = eItemRarity;
            boxEntry.probability = -1f;
            BoxEntry boxEntry2 = boxEntry;
            boxEntries.Add(boxEntry2);
        }
        float num3 = 0f;
        for (int j = 0; j < num; j++)
        {
            BoxEntry value = boxEntries[j];
            if (value.rarity == EItemRarity.MYTHICAL)
            {
                value.probability = qualityRarities[EItemRarity.MYTHICAL];
            }
            else
            {
                if (boxAsset.probabilityModel == EBoxProbabilityModel.Original)
                {
                    int num4 = dictionary[value.rarity];
                    float num5 = (value.probability = qualityRarities[value.rarity] / (float)num4);
                }
                else
                {
                    int num6 = num - 1;
                    value.probability = 1f / (float)num6;
                }
                num3 += value.probability;
            }
            boxEntries[j] = value;
        }
        if (Mathf.Abs(num3 - 1f) > 0.01f)
        {
            UnturnedLog.warn("Unable to guess box probabilities ({0})", num3);
            for (int k = 0; k < num; k++)
            {
                BoxEntry value2 = boxEntries[k];
                value2.probability = -1f;
                boxEntries[k] = value2;
            }
        }
        boxEntries.Sort(new BoxEntryComparer());
    }

    private static void onClickedKeyButton(ISleekElement button)
    {
        ItemStore.Get().ViewItem(boxAsset.destroy);
    }

    private static void onClickedUnboxButton(ISleekElement button)
    {
        if (boxAsset.destroy == 0)
        {
            Provider.provider.economyService.exchangeInventory(boxAsset.generate, new List<EconExchangePair>
            {
                new EconExchangePair(instance, 1)
            });
        }
        else
        {
            ulong inventoryPackage = Provider.provider.economyService.getInventoryPackage(boxAsset.destroy);
            if (inventoryPackage == 0L)
            {
                return;
            }
            List<EconExchangePair> destroy = new List<EconExchangePair>
            {
                new EconExchangePair(instance, 1),
                new EconExchangePair(inventoryPackage, 1)
            };
            Provider.provider.economyService.exchangeInventory(boxAsset.generate, destroy);
        }
        isUnboxing = true;
        backButton.isVisible = false;
        lastUnbox = Time.realtimeSinceStartup;
        lastAngle = Time.realtimeSinceStartup;
        keyButton.isVisible = false;
        unboxButton.isVisible = false;
    }

    private static bool hasAssetsForGrantedItems(List<SteamItemDetails_t> grantedItems)
    {
        foreach (SteamItemDetails_t grantedItem in grantedItems)
        {
            Provider.provider.economyService.getInventoryTargetID(grantedItem.m_iDefinition.m_SteamItemDef, out var item_guid, out var vehicle_guid);
            if (item_guid != default(Guid))
            {
                if (Assets.find<ItemAsset>(item_guid) == null)
                {
                    return false;
                }
                continue;
            }
            if (vehicle_guid != default(Guid))
            {
                if (Assets.find<VehicleAsset>(vehicle_guid) == null)
                {
                    return false;
                }
                continue;
            }
            return false;
        }
        return true;
    }

    private static bool wasGrantedMythical(List<SteamItemDetails_t> grantedItems)
    {
        foreach (SteamItemDetails_t grantedItem in grantedItems)
        {
            if (Provider.provider.economyService.getInventoryMythicID(grantedItem.m_iDefinition.m_SteamItemDef) != 0)
            {
                return true;
            }
        }
        return false;
    }

    private static int getIndexOfGrantedItemInDrops(List<SteamItemDetails_t> grantedItems)
    {
        for (int i = 1; i < numBoxEntries; i++)
        {
            int id = boxEntries[i].id;
            foreach (SteamItemDetails_t grantedItem in grantedItems)
            {
                if (grantedItem.m_iDefinition.m_SteamItemDef == id)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private static void exchangeErrorAlert(string message)
    {
        isUnboxing = false;
        backButton.isVisible = true;
        MenuUI.alert(message);
        MenuSurvivorsClothingUI.open();
        close();
    }

    private static void onInventoryExchanged(List<SteamItemDetails_t> grantedItems)
    {
        if (!isUnboxing)
        {
            return;
        }
        if (!hasAssetsForGrantedItems(grantedItems))
        {
            exchangeErrorAlert(localization.format("Exchange_Missing_Assets"));
            return;
        }
        MenuSurvivorsClothingUI.updatePage();
        int num;
        if (wasGrantedMythical(grantedItems))
        {
            didUnboxMythical = true;
            num = 0;
        }
        else
        {
            didUnboxMythical = false;
            num = getIndexOfGrantedItemInDrops(grantedItems);
            if (num < 0)
            {
                exchangeErrorAlert(localization.format("Exchange_Not_In_Drops"));
                return;
            }
        }
        unboxedItems = grantedItems;
        unboxedItems.Sort(new EconItemRarityComparer());
        if (rotation < numBoxEntries * 2)
        {
            target = numBoxEntries * 3 + num;
        }
        else
        {
            target = ((int)((float)rotation / (float)numBoxEntries) + 2) * numBoxEntries + num;
        }
    }

    public static void update()
    {
        if (!isUnboxing)
        {
            return;
        }
        if (Time.realtimeSinceStartup - lastUnbox > (float)Provider.CLIENT_TIMEOUT)
        {
            isUnboxing = false;
            backButton.isVisible = true;
            MenuUI.alert(localization.format("Exchange_Timed_Out"));
            MenuSurvivorsClothingUI.open();
            close();
            return;
        }
        if (rotation == target)
        {
            if (Time.realtimeSinceStartup - lastAngle > 0.5f)
            {
                isUnboxing = false;
                backButton.isVisible = true;
                string key = null;
                switch (boxAsset.itemOrigin)
                {
                case EBoxItemOrigin.Unbox:
                    key = "Origin_Unbox";
                    break;
                case EBoxItemOrigin.Unwrap:
                    key = "Origin_Unwrap";
                    break;
                }
                MenuUI.alertNewItems(localization.format(key), unboxedItems);
                SteamItemDetails_t steamItemDetails_t = unboxedItems[0];
                MenuSurvivorsClothingItemUI.viewItem(steamItemDetails_t.m_iDefinition.m_SteamItemDef, steamItemDetails_t.m_unQuantity, steamItemDetails_t.m_itemId.m_SteamItemInstanceID);
                MenuSurvivorsClothingItemUI.open();
                close();
                string path = (didUnboxMythical ? "Economy/Sounds/Mythical" : "Economy/Sounds/Unbox");
                MainCamera.instance.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>(path), 0.66f);
            }
            return;
        }
        if (rotation < target - numBoxEntries || target == -1)
        {
            if (angle < (float)Math.PI * 4f)
            {
                angle += (Time.realtimeSinceStartup - lastAngle) * size * Mathf.Lerp(80f, 20f, angle / ((float)Math.PI * 4f));
            }
            else
            {
                angle += (Time.realtimeSinceStartup - lastAngle) * size * 20f;
            }
        }
        else
        {
            angle += (Time.realtimeSinceStartup - lastAngle) * Mathf.Max(((float)target - angle / ((float)Math.PI * 2f / (float)numBoxEntries)) / (float)numBoxEntries, 0.05f) * size * 20f;
        }
        lastAngle = Time.realtimeSinceStartup;
        rotation = (int)(angle / ((float)Math.PI * 2f / (float)numBoxEntries));
        if (rotation == target)
        {
            angle = (float)rotation * ((float)Math.PI * 2f / (float)numBoxEntries);
        }
        for (int i = 0; i < numBoxEntries; i++)
        {
            float num = (float)Math.PI * 2f * (float)i / (float)numBoxEntries + (float)Math.PI;
            dropButtons[i].positionScale_X = 0.5f + Mathf.Cos(angle - num) * (0.5f - size / 2f) - size / 2f;
            dropButtons[i].positionScale_Y = 0.5f + Mathf.Sin(angle - num) * (0.5f - size / 2f) - size / 2f;
        }
        if (rotation != lastRotation)
        {
            lastRotation = rotation;
            boxButton.positionScale_Y = 0.25f;
            boxButton.lerpPositionScale(0.3f, 0.3f, ESleekLerp.EXPONENTIAL, 20f);
            finalBox.positionOffset_X = -20;
            finalBox.positionOffset_Y = -20;
            finalBox.sizeOffset_X = 40;
            finalBox.sizeOffset_Y = 40;
            finalBox.lerpPositionOffset(-10, -10, ESleekLerp.EXPONENTIAL, 1f);
            finalBox.lerpSizeOffset(20, 20, ESleekLerp.EXPONENTIAL, 1f);
            boxButton.updateInventory(0uL, boxEntries[rotation % numBoxEntries].id, 1, isClickable: false, isLarge: true);
            if (rotation == target)
            {
                MainCamera.instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Economy/Sounds/Drop"), 0.33f);
            }
            else
            {
                MainCamera.instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Economy/Sounds/Tick"), 0.33f);
            }
        }
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuSurvivorsClothingItemUI.open();
        close();
    }

    public void OnDestroy()
    {
        TempSteamworksEconomy economyService = Provider.provider.economyService;
        economyService.onInventoryExchanged = (TempSteamworksEconomy.InventoryExchanged)Delegate.Remove(economyService.onInventoryExchanged, new TempSteamworksEconomy.InventoryExchanged(onInventoryExchanged));
    }

    public MenuSurvivorsClothingBoxUI()
    {
        localization = Localization.read("/Menu/Survivors/MenuSurvivorsClothingBox.dat");
        if (icons != null)
        {
            icons.unload();
        }
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Survivors/MenuSurvivorsClothingBox/MenuSurvivorsClothingBox.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        inventory = Glazier.Get().CreateConstraintFrame();
        inventory.positionScale_X = 0.5f;
        inventory.positionOffset_Y = 10;
        inventory.sizeScale_X = 0.5f;
        inventory.sizeScale_Y = 1f;
        inventory.sizeOffset_Y = -20;
        inventory.constraint = ESleekConstraint.FitInParent;
        container.AddChild(inventory);
        finalBox = Glazier.Get().CreateBox();
        finalBox.positionOffset_X = -10;
        finalBox.positionOffset_Y = -10;
        finalBox.sizeOffset_X = 20;
        finalBox.sizeOffset_Y = 20;
        inventory.AddChild(finalBox);
        boxButton = new SleekInventory();
        boxButton.positionOffset_Y = -30;
        boxButton.positionScale_X = 0.3f;
        boxButton.positionScale_Y = 0.3f;
        boxButton.sizeScale_X = 0.4f;
        boxButton.sizeScale_Y = 0.4f;
        inventory.AddChild(boxButton);
        keyButton = new SleekButtonIcon(null, 40);
        keyButton.positionOffset_Y = -20;
        keyButton.positionScale_X = 0.3f;
        keyButton.positionScale_Y = 0.7f;
        keyButton.sizeOffset_X = -5;
        keyButton.sizeOffset_Y = 50;
        keyButton.sizeScale_X = 0.2f;
        keyButton.text = localization.format("Key_Text");
        keyButton.tooltip = localization.format("Key_Tooltip");
        keyButton.onClickedButton += onClickedKeyButton;
        keyButton.fontSize = ESleekFontSize.Medium;
        keyButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        inventory.AddChild(keyButton);
        keyButton.isVisible = false;
        unboxButton = new SleekButtonIcon(null);
        unboxButton.positionOffset_X = 5;
        unboxButton.positionOffset_Y = -20;
        unboxButton.positionScale_X = 0.5f;
        unboxButton.positionScale_Y = 0.7f;
        unboxButton.sizeOffset_X = -5;
        unboxButton.sizeOffset_Y = 50;
        unboxButton.sizeScale_X = 0.2f;
        unboxButton.text = localization.format("Unbox_Text");
        unboxButton.tooltip = localization.format("Unbox_Tooltip");
        unboxButton.onClickedButton += onClickedUnboxButton;
        unboxButton.fontSize = ESleekFontSize.Medium;
        unboxButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        inventory.AddChild(unboxButton);
        unboxButton.isVisible = false;
        disabledBox = Glazier.Get().CreateBox();
        disabledBox.positionOffset_Y = -20;
        disabledBox.positionScale_X = 0.3f;
        disabledBox.positionScale_Y = 0.7f;
        disabledBox.sizeOffset_Y = 50;
        disabledBox.sizeScale_X = 0.4f;
        inventory.AddChild(disabledBox);
        disabledBox.isVisible = false;
        rareLabel = Glazier.Get().CreateLabel();
        rareLabel.positionOffset_X = 50;
        rareLabel.positionOffset_Y = 50;
        rareLabel.sizeOffset_X = 200;
        rareLabel.sizeOffset_Y = 30;
        rareLabel.text = localization.format("Rarity_Rare", formatQualityRarity(EItemRarity.RARE));
        rareLabel.textColor = ItemTool.getRarityColorUI(EItemRarity.RARE);
        rareLabel.fontAlignment = TextAnchor.MiddleLeft;
        rareLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(rareLabel);
        epicLabel = Glazier.Get().CreateLabel();
        epicLabel.positionOffset_X = 50;
        epicLabel.positionOffset_Y = 70;
        epicLabel.sizeOffset_X = 200;
        epicLabel.sizeOffset_Y = 30;
        epicLabel.text = localization.format("Rarity_Epic", formatQualityRarity(EItemRarity.EPIC));
        epicLabel.textColor = ItemTool.getRarityColorUI(EItemRarity.EPIC);
        epicLabel.fontAlignment = TextAnchor.MiddleLeft;
        epicLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(epicLabel);
        legendaryLabel = Glazier.Get().CreateLabel();
        legendaryLabel.positionOffset_X = 50;
        legendaryLabel.positionOffset_Y = 90;
        legendaryLabel.sizeOffset_X = 200;
        legendaryLabel.sizeOffset_Y = 30;
        legendaryLabel.text = localization.format("Rarity_Legendary", formatQualityRarity(EItemRarity.LEGENDARY));
        legendaryLabel.textColor = ItemTool.getRarityColorUI(EItemRarity.LEGENDARY);
        legendaryLabel.fontAlignment = TextAnchor.MiddleLeft;
        legendaryLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(legendaryLabel);
        mythicalLabel = Glazier.Get().CreateLabel();
        mythicalLabel.positionOffset_X = 50;
        mythicalLabel.positionOffset_Y = 110;
        mythicalLabel.sizeOffset_X = 200;
        mythicalLabel.sizeOffset_Y = 30;
        mythicalLabel.text = localization.format("Rarity_Mythical", formatQualityRarity(EItemRarity.MYTHICAL));
        mythicalLabel.textColor = ItemTool.getRarityColorUI(EItemRarity.MYTHICAL);
        mythicalLabel.fontAlignment = TextAnchor.MiddleLeft;
        mythicalLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(mythicalLabel);
        equalizedLabel = Glazier.Get().CreateLabel();
        equalizedLabel.positionOffset_X = 50;
        equalizedLabel.positionOffset_Y = 50;
        equalizedLabel.sizeOffset_X = 200;
        equalizedLabel.sizeOffset_Y = 30;
        equalizedLabel.text = localization.format("Rarity_Equalized");
        equalizedLabel.fontAlignment = TextAnchor.MiddleLeft;
        equalizedLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(equalizedLabel);
        bonusLabel = Glazier.Get().CreateLabel();
        bonusLabel.positionOffset_X = 50;
        bonusLabel.positionOffset_Y = 130;
        bonusLabel.sizeOffset_X = 200;
        bonusLabel.sizeOffset_Y = 30;
        bonusLabel.text = localization.format("Rarity_Bonus_Items", (BONUS_ITEM_RARITY * 100f).ToString("0.0"));
        bonusLabel.fontAlignment = TextAnchor.MiddleLeft;
        bonusLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(bonusLabel);
        dropButtons = null;
        TempSteamworksEconomy economyService = Provider.provider.economyService;
        economyService.onInventoryExchanged = (TempSteamworksEconomy.InventoryExchanged)Delegate.Combine(economyService.onInventoryExchanged, new TempSteamworksEconomy.InventoryExchanged(onInventoryExchanged));
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
