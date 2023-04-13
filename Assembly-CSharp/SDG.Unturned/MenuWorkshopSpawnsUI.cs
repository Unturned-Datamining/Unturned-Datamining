using System;
using System.IO;
using UnityEngine;

namespace SDG.Unturned;

public class MenuWorkshopSpawnsUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekScrollView spawnsBox;

    private static SleekButtonState typeButton;

    private static ISleekUInt16Field viewIDField;

    private static ISleekButton viewButton;

    private static ISleekButton rawButton;

    private static ISleekButton newButton;

    private static ISleekButton writeButton;

    private static ISleekBox rootsBox;

    private static ISleekBox tablesBox;

    private static ISleekField rawField;

    private static ISleekUInt16Field addRootIDField;

    private static SleekButtonIcon addRootSpawnButton;

    private static ISleekUInt16Field addTableIDField;

    private static SleekButtonIcon addTableAssetButton;

    private static SleekButtonIcon addTableSpawnButton;

    private static ISleekButton applyWeightsButton;

    private static SpawnAsset asset;

    private static EAssetType type;

    public static void open()
    {
        if (!active)
        {
            active = true;
            Localization.refresh();
            refresh();
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

    private static void refresh()
    {
        rawField.isVisible = false;
        rootsBox.isVisible = true;
        tablesBox.isVisible = true;
        rootsBox.RemoveAllChildren();
        tablesBox.RemoveAllChildren();
        MenuWorkshopSpawnsUI.asset = Assets.find(EAssetType.SPAWN, viewIDField.state) as SpawnAsset;
        switch (typeButton.state)
        {
        case 0:
            type = EAssetType.ITEM;
            break;
        case 1:
            type = EAssetType.VEHICLE;
            break;
        case 2:
            type = EAssetType.ANIMAL;
            break;
        default:
            type = EAssetType.NONE;
            return;
        }
        int num = 120;
        rootsBox.positionOffset_Y = num;
        num += 40;
        if (MenuWorkshopSpawnsUI.asset != null)
        {
            rootsBox.text = localization.format("Roots_Box", viewIDField.state + " " + MenuWorkshopSpawnsUI.asset.name);
            for (int i = 0; i < MenuWorkshopSpawnsUI.asset.roots.Count; i++)
            {
                SpawnTable spawnTable = MenuWorkshopSpawnsUI.asset.roots[i];
                if (spawnTable.spawnID != 0)
                {
                    ISleekButton sleekButton = Glazier.Get().CreateButton();
                    sleekButton.positionOffset_Y = 40 + i * 40;
                    sleekButton.sizeOffset_X = -260;
                    sleekButton.sizeScale_X = 1f;
                    sleekButton.sizeOffset_Y = 30;
                    sleekButton.onClickedButton += onClickedRootButton;
                    rootsBox.AddChild(sleekButton);
                    num += 40;
                    if (Assets.find(EAssetType.SPAWN, spawnTable.spawnID) is SpawnAsset spawnAsset)
                    {
                        sleekButton.text = spawnTable.spawnID + " " + spawnAsset.name;
                    }
                    else
                    {
                        sleekButton.text = spawnTable.spawnID + " ?";
                    }
                    ISleekInt32Field sleekInt32Field = Glazier.Get().CreateInt32Field();
                    sleekInt32Field.positionOffset_X = 10;
                    sleekInt32Field.positionScale_X = 1f;
                    sleekInt32Field.sizeOffset_X = 55;
                    sleekInt32Field.sizeOffset_Y = 30;
                    sleekInt32Field.state = spawnTable.weight;
                    sleekInt32Field.tooltipText = localization.format("Weight_Tooltip");
                    sleekInt32Field.onTypedInt += onTypedRootWeightField;
                    sleekButton.AddChild(sleekInt32Field);
                    ISleekBox sleekBox = Glazier.Get().CreateBox();
                    sleekBox.positionOffset_X = 75;
                    sleekBox.positionScale_X = 1f;
                    sleekBox.sizeOffset_X = 55;
                    sleekBox.sizeOffset_Y = 30;
                    sleekBox.text = Math.Round(spawnTable.chance * 1000f) / 10.0 + "%";
                    sleekBox.tooltipText = localization.format("Chance_Tooltip");
                    sleekButton.AddChild(sleekBox);
                    SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
                    sleekButtonIcon.positionOffset_X = 140;
                    sleekButtonIcon.positionScale_X = 1f;
                    sleekButtonIcon.sizeOffset_X = 120;
                    sleekButtonIcon.sizeOffset_Y = 30;
                    sleekButtonIcon.text = localization.format("Remove_Root_Button");
                    sleekButtonIcon.tooltip = localization.format("Remove_Root_Button_Tooltip");
                    sleekButtonIcon.onClickedButton += onClickedRemoveRootButton;
                    sleekButton.AddChild(sleekButtonIcon);
                }
            }
            addRootIDField.positionOffset_Y = num;
            addRootSpawnButton.positionOffset_Y = num;
            num += 40;
            addRootIDField.isVisible = true;
            addRootSpawnButton.isVisible = true;
        }
        else
        {
            rootsBox.text = localization.format("Roots_Box", viewIDField.state + " ?");
            addRootIDField.isVisible = false;
            addRootSpawnButton.isVisible = false;
        }
        num += 40;
        tablesBox.positionOffset_Y = num;
        num += 40;
        if (MenuWorkshopSpawnsUI.asset != null)
        {
            tablesBox.text = localization.format("Tables_Box", viewIDField.state + " " + MenuWorkshopSpawnsUI.asset.name);
            for (int j = 0; j < MenuWorkshopSpawnsUI.asset.tables.Count; j++)
            {
                SpawnTable spawnTable2 = MenuWorkshopSpawnsUI.asset.tables[j];
                ISleekElement sleekElement = null;
                if (spawnTable2.spawnID != 0)
                {
                    ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                    sleekButton2.positionOffset_Y = 40 + j * 40;
                    sleekButton2.sizeOffset_X = -260;
                    sleekButton2.sizeScale_X = 1f;
                    sleekButton2.sizeOffset_Y = 30;
                    sleekButton2.onClickedButton += onClickedTableButton;
                    tablesBox.AddChild(sleekButton2);
                    sleekElement = sleekButton2;
                    num += 40;
                    if (Assets.find(EAssetType.SPAWN, spawnTable2.spawnID) is SpawnAsset spawnAsset2)
                    {
                        sleekButton2.text = spawnTable2.spawnID + " " + spawnAsset2.name;
                    }
                    else
                    {
                        sleekButton2.text = spawnTable2.spawnID + " ?";
                    }
                }
                else if (spawnTable2.assetID != 0)
                {
                    ISleekBox sleekBox2 = Glazier.Get().CreateBox();
                    sleekBox2.positionOffset_Y = 40 + j * 40;
                    sleekBox2.sizeOffset_X = -260;
                    sleekBox2.sizeScale_X = 1f;
                    sleekBox2.sizeOffset_Y = 30;
                    tablesBox.AddChild(sleekBox2);
                    sleekElement = sleekBox2;
                    num += 40;
                    Asset asset = Assets.find(type, spawnTable2.assetID);
                    if (asset != null)
                    {
                        sleekBox2.text = spawnTable2.assetID + " " + asset.name;
                        if (type == EAssetType.ITEM)
                        {
                            ItemAsset itemAsset = asset as ItemAsset;
                            if (MenuWorkshopSpawnsUI.asset != null)
                            {
                                sleekBox2.textColor = ItemTool.getRarityColorUI(itemAsset.rarity);
                            }
                        }
                        else if (type == EAssetType.VEHICLE)
                        {
                            VehicleAsset vehicleAsset = asset as VehicleAsset;
                            if (MenuWorkshopSpawnsUI.asset != null)
                            {
                                sleekBox2.textColor = ItemTool.getRarityColorUI(vehicleAsset.rarity);
                            }
                        }
                    }
                    else
                    {
                        sleekBox2.text = spawnTable2.assetID + " ?";
                    }
                }
                if (sleekElement != null)
                {
                    ISleekInt32Field sleekInt32Field2 = Glazier.Get().CreateInt32Field();
                    sleekInt32Field2.positionOffset_X = 10;
                    sleekInt32Field2.positionScale_X = 1f;
                    sleekInt32Field2.sizeOffset_X = 55;
                    sleekInt32Field2.sizeOffset_Y = 30;
                    sleekInt32Field2.state = spawnTable2.weight;
                    sleekInt32Field2.tooltipText = localization.format("Weight_Tooltip");
                    sleekInt32Field2.onTypedInt += onTypedTableWeightField;
                    sleekElement.AddChild(sleekInt32Field2);
                    float num2 = spawnTable2.chance;
                    if (j > 0)
                    {
                        num2 -= MenuWorkshopSpawnsUI.asset.tables[j - 1].chance;
                    }
                    ISleekBox sleekBox3 = Glazier.Get().CreateBox();
                    sleekBox3.positionOffset_X = 75;
                    sleekBox3.positionScale_X = 1f;
                    sleekBox3.sizeOffset_X = 55;
                    sleekBox3.sizeOffset_Y = 30;
                    sleekBox3.text = Math.Round(num2 * 1000f) / 10.0 + "%";
                    sleekBox3.tooltipText = localization.format("Chance_Tooltip");
                    sleekElement.AddChild(sleekBox3);
                    SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
                    sleekButtonIcon2.positionOffset_X = 140;
                    sleekButtonIcon2.positionScale_X = 1f;
                    sleekButtonIcon2.sizeOffset_X = 120;
                    sleekButtonIcon2.sizeOffset_Y = 30;
                    sleekButtonIcon2.text = localization.format("Remove_Table_Button");
                    sleekButtonIcon2.tooltip = localization.format("Remove_Table_Button_Tooltip");
                    sleekButtonIcon2.onClickedButton += onClickedRemoveTableButton;
                    sleekElement.AddChild(sleekButtonIcon2);
                }
            }
            addTableIDField.positionOffset_Y = num;
            addTableAssetButton.positionOffset_Y = num;
            addTableSpawnButton.positionOffset_Y = num;
            num += 40;
            addTableIDField.isVisible = true;
            addTableAssetButton.isVisible = true;
            addTableSpawnButton.isVisible = true;
        }
        else
        {
            tablesBox.text = localization.format("Tables_Box", viewIDField.state + " ?");
            addTableIDField.isVisible = false;
            addTableAssetButton.isVisible = false;
            addTableSpawnButton.isVisible = false;
        }
        if (MenuWorkshopSpawnsUI.asset != null)
        {
            applyWeightsButton.positionOffset_Y = num;
            num += 40;
            applyWeightsButton.isVisible = true;
        }
        else
        {
            applyWeightsButton.isVisible = false;
        }
        spawnsBox.contentSizeOffset = new Vector2(0f, num - 10);
    }

    private static string getRaw(SpawnAsset asset)
    {
        string text = "Type Spawn";
        text = text + "\nID " + asset.id;
        int num = 0;
        int num2 = 0;
        for (int i = 0; i < asset.roots.Count; i++)
        {
            SpawnTable spawnTable = asset.roots[i];
            if (spawnTable.isLink && spawnTable.weight > 0)
            {
                num++;
            }
        }
        if (num > 0)
        {
            text += "\n";
            text = text + "\nRoots " + num;
            for (int j = 0; j < asset.roots.Count; j++)
            {
                SpawnTable spawnTable2 = asset.roots[j];
                if (spawnTable2.isLink && spawnTable2.weight > 0)
                {
                    text = text + "\nRoot_" + num2 + "_Spawn_ID " + spawnTable2.spawnID;
                    text = text + "\nRoot_" + num2 + "_Weight " + spawnTable2.weight;
                    num2++;
                }
            }
        }
        int num3 = 0;
        int num4 = 0;
        for (int k = 0; k < asset.tables.Count; k++)
        {
            SpawnTable spawnTable3 = asset.tables[k];
            if (!spawnTable3.isLink && spawnTable3.weight > 0)
            {
                num3++;
            }
        }
        if (num3 > 0)
        {
            text += "\n";
            text = text + "\nTables " + asset.tables.Count;
            for (int l = 0; l < asset.tables.Count; l++)
            {
                SpawnTable spawnTable4 = asset.tables[l];
                if (!spawnTable4.isLink && spawnTable4.weight > 0)
                {
                    text = ((spawnTable4.assetID == 0) ? (text + "\nTable_" + num4 + "_Spawn_ID " + spawnTable4.spawnID) : (text + "\nTable_" + num4 + "_Asset_ID " + spawnTable4.assetID));
                    text = text + "\nTable_" + num4 + "_Weight " + spawnTable4.weight;
                    num4++;
                }
            }
        }
        return text;
    }

    private static void raw()
    {
        rawField.isVisible = true;
        rootsBox.isVisible = false;
        tablesBox.isVisible = false;
        addRootIDField.isVisible = false;
        addRootSpawnButton.isVisible = false;
        addTableIDField.isVisible = false;
        addTableAssetButton.isVisible = false;
        addTableSpawnButton.isVisible = false;
        applyWeightsButton.isVisible = false;
        asset = Assets.find(EAssetType.SPAWN, viewIDField.state) as SpawnAsset;
        string text = ((asset == null) ? "?" : getRaw(asset));
        rawField.text = text;
        GUIUtility.systemCopyBuffer = text;
        spawnsBox.contentSizeOffset = new Vector2(0f, 1080f);
    }

    private static void write()
    {
        asset = Assets.find(EAssetType.SPAWN, viewIDField.state) as SpawnAsset;
        if (asset != null && !string.IsNullOrEmpty(asset.absoluteOriginFilePath) && File.Exists(asset.absoluteOriginFilePath))
        {
            string contents = getRaw(asset);
            File.WriteAllText(asset.absoluteOriginFilePath, contents);
        }
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuWorkshopUI.open();
        close();
    }

    private static void onClickedViewButton(ISleekElement button)
    {
        refresh();
    }

    private static void onClickedRawButton(ISleekElement button)
    {
        raw();
    }

    private static void onClickedNewButton(ISleekElement button)
    {
        Assets.addToMapping(new SpawnAsset
        {
            id = viewIDField.state
        }, overrideExistingID: false, Assets.defaultAssetMapping);
        refresh();
    }

    private static void onClickedWriteButton(ISleekElement button)
    {
        write();
    }

    private static void onClickedRootButton(ISleekElement button)
    {
        int index = rootsBox.FindIndexOfChild(button);
        ushort spawnID = asset.roots[index].spawnID;
        viewIDField.state = spawnID;
        refresh();
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        int index = tablesBox.FindIndexOfChild(button);
        ushort spawnID = asset.tables[index].spawnID;
        viewIDField.state = spawnID;
        refresh();
    }

    private static void onTypedRootWeightField(ISleekInt32Field field, int state)
    {
        int index = rootsBox.FindIndexOfChild(field.parent);
        asset.roots[index].weight = state;
    }

    private static void onClickedAddRootSpawnButton(ISleekElement button)
    {
        if (addRootIDField.state == 0)
        {
            return;
        }
        for (int i = 0; i < asset.roots.Count; i++)
        {
            if (asset.roots[i].spawnID == addRootIDField.state)
            {
                return;
            }
        }
        if (Assets.find(EAssetType.SPAWN, addRootIDField.state) is SpawnAsset spawnAsset)
        {
            SpawnTable spawnTable = new SpawnTable();
            spawnTable.spawnID = addRootIDField.state;
            spawnTable.isLink = true;
            asset.roots.Add(spawnTable);
            SpawnTable spawnTable2 = new SpawnTable();
            spawnTable2.spawnID = asset.id;
            spawnTable2.isLink = true;
            spawnAsset.tables.Add(spawnTable2);
            spawnAsset.markTablesDirty();
            addRootIDField.state = 0;
            refresh();
        }
    }

    private static void onClickedRemoveRootButton(ISleekElement button)
    {
        int rootIndex = rootsBox.FindIndexOfChild(button.parent);
        asset.removeRootAtIndex(rootIndex);
        refresh();
    }

    private static void onTypedTableWeightField(ISleekInt32Field field, int state)
    {
        int tableIndex = tablesBox.FindIndexOfChild(field.parent);
        asset.setTableWeightAtIndex(tableIndex, state);
    }

    private static void onClickedAddTableAssetButton(ISleekElement button)
    {
        if (addTableIDField.state == 0)
        {
            return;
        }
        for (int i = 0; i < asset.tables.Count; i++)
        {
            if (asset.tables[i].assetID == addTableIDField.state)
            {
                return;
            }
        }
        asset.addAssetTable(addTableIDField.state);
        addTableIDField.state = 0;
        refresh();
    }

    private static void onClickedAddTableSpawnButton(ISleekElement button)
    {
        if (addTableIDField.state == 0)
        {
            return;
        }
        for (int i = 0; i < asset.tables.Count; i++)
        {
            if (asset.tables[i].spawnID == addTableIDField.state)
            {
                return;
            }
        }
        if (Assets.find(EAssetType.SPAWN, addTableIDField.state) is SpawnAsset other)
        {
            asset.addSpawnTable(other);
            addTableIDField.state = 0;
            refresh();
        }
    }

    private static void onClickedRemoveTableButton(ISleekElement button)
    {
        int tableIndex = tablesBox.FindIndexOfChild(button.parent);
        asset.removeTableAtIndex(tableIndex);
        refresh();
    }

    private static void onClickedApplyWeightsButton(ISleekElement button)
    {
        asset.sortAndNormalizeWeights();
        refresh();
    }

    public MenuWorkshopSpawnsUI()
    {
        localization = Localization.read("/Menu/Workshop/MenuWorkshopSpawns.dat");
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
        spawnsBox = Glazier.Get().CreateScrollView();
        spawnsBox.positionOffset_X = -315;
        spawnsBox.positionOffset_Y = 100;
        spawnsBox.positionScale_X = 0.5f;
        spawnsBox.sizeOffset_X = 630;
        spawnsBox.sizeOffset_Y = -200;
        spawnsBox.sizeScale_Y = 1f;
        spawnsBox.scaleContentToWidth = true;
        container.AddChild(spawnsBox);
        typeButton = new SleekButtonState(new GUIContent(localization.format("Type_Item")), new GUIContent(localization.format("Type_Vehicle")), new GUIContent(localization.format("Type_Animal")));
        typeButton.sizeOffset_X = 600;
        typeButton.sizeOffset_Y = 30;
        typeButton.tooltip = localization.format("Type_Tooltip");
        spawnsBox.AddChild(typeButton);
        viewIDField = Glazier.Get().CreateUInt16Field();
        viewIDField.positionOffset_Y = 40;
        viewIDField.sizeOffset_X = 160;
        viewIDField.sizeOffset_Y = 30;
        spawnsBox.AddChild(viewIDField);
        viewButton = Glazier.Get().CreateButton();
        viewButton.positionOffset_X = 170;
        viewButton.positionOffset_Y = 40;
        viewButton.sizeOffset_X = 100;
        viewButton.sizeOffset_Y = 30;
        viewButton.text = localization.format("View_Button");
        viewButton.tooltipText = localization.format("View_Button_Tooltip");
        viewButton.onClickedButton += onClickedViewButton;
        spawnsBox.AddChild(viewButton);
        rawButton = Glazier.Get().CreateButton();
        rawButton.positionOffset_X = 280;
        rawButton.positionOffset_Y = 40;
        rawButton.sizeOffset_X = 100;
        rawButton.sizeOffset_Y = 30;
        rawButton.text = localization.format("Raw_Button");
        rawButton.tooltipText = localization.format("Raw_Button_Tooltip");
        rawButton.onClickedButton += onClickedRawButton;
        spawnsBox.AddChild(rawButton);
        newButton = Glazier.Get().CreateButton();
        newButton.positionOffset_X = 390;
        newButton.positionOffset_Y = 40;
        newButton.sizeOffset_X = 100;
        newButton.sizeOffset_Y = 30;
        newButton.text = localization.format("New_Button");
        newButton.tooltipText = localization.format("New_Button_Tooltip");
        newButton.onClickedButton += onClickedNewButton;
        spawnsBox.AddChild(newButton);
        writeButton = Glazier.Get().CreateButton();
        writeButton.positionOffset_X = 500;
        writeButton.positionOffset_Y = 40;
        writeButton.sizeOffset_X = 100;
        writeButton.sizeOffset_Y = 30;
        writeButton.text = localization.format("Write_Button");
        writeButton.tooltipText = localization.format("Write_Button_Tooltip");
        writeButton.onClickedButton += onClickedWriteButton;
        spawnsBox.AddChild(writeButton);
        addRootIDField = Glazier.Get().CreateUInt16Field();
        addRootIDField.sizeOffset_X = 470;
        addRootIDField.sizeOffset_Y = 30;
        spawnsBox.AddChild(addRootIDField);
        addRootSpawnButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
        addRootSpawnButton.positionOffset_X = 480;
        addRootSpawnButton.sizeOffset_X = 120;
        addRootSpawnButton.sizeOffset_Y = 30;
        addRootSpawnButton.text = localization.format("Add_Root_Spawn_Button");
        addRootSpawnButton.tooltip = localization.format("Add_Root_Spawn_Button_Tooltip");
        addRootSpawnButton.onClickedButton += onClickedAddRootSpawnButton;
        spawnsBox.AddChild(addRootSpawnButton);
        addTableIDField = Glazier.Get().CreateUInt16Field();
        addTableIDField.sizeOffset_X = 340;
        addTableIDField.sizeOffset_Y = 30;
        spawnsBox.AddChild(addTableIDField);
        addTableAssetButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
        addTableAssetButton.positionOffset_X = 350;
        addTableAssetButton.sizeOffset_X = 120;
        addTableAssetButton.sizeOffset_Y = 30;
        addTableAssetButton.text = localization.format("Add_Table_Asset_Button");
        addTableAssetButton.tooltip = localization.format("Add_Table_Asset_Button_Tooltip");
        addTableAssetButton.onClickedButton += onClickedAddTableAssetButton;
        spawnsBox.AddChild(addTableAssetButton);
        addTableSpawnButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
        addTableSpawnButton.positionOffset_X = 480;
        addTableSpawnButton.sizeOffset_X = 120;
        addTableSpawnButton.sizeOffset_Y = 30;
        addTableSpawnButton.text = localization.format("Add_Table_Spawn_Button");
        addTableSpawnButton.tooltip = localization.format("Add_Table_Spawn_Button_Tooltip");
        addTableSpawnButton.onClickedButton += onClickedAddTableSpawnButton;
        spawnsBox.AddChild(addTableSpawnButton);
        applyWeightsButton = Glazier.Get().CreateButton();
        applyWeightsButton.sizeOffset_X = 600;
        applyWeightsButton.sizeOffset_Y = 30;
        applyWeightsButton.text = localization.format("Apply_Weights_Button");
        applyWeightsButton.tooltipText = localization.format("Apply_Weights_Button_Tooltip");
        applyWeightsButton.onClickedButton += onClickedApplyWeightsButton;
        spawnsBox.AddChild(applyWeightsButton);
        rootsBox = Glazier.Get().CreateBox();
        rootsBox.positionOffset_Y = 40;
        rootsBox.sizeOffset_X = 600;
        rootsBox.sizeOffset_Y = 30;
        rootsBox.tooltipText = localization.format("Roots_Box_Tooltip");
        spawnsBox.AddChild(rootsBox);
        tablesBox = Glazier.Get().CreateBox();
        tablesBox.positionOffset_Y = 80;
        tablesBox.sizeOffset_X = 600;
        tablesBox.sizeOffset_Y = 30;
        tablesBox.tooltipText = localization.format("Tables_Box_Tooltip");
        spawnsBox.AddChild(tablesBox);
        rawField = Glazier.Get().CreateStringField();
        rawField.positionOffset_Y = 80;
        rawField.sizeOffset_X = 600;
        rawField.sizeOffset_Y = 1000;
        rawField.multiline = true;
        rawField.maxLength = 4096;
        rawField.fontAlignment = TextAnchor.UpperLeft;
        spawnsBox.AddChild(rawField);
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
