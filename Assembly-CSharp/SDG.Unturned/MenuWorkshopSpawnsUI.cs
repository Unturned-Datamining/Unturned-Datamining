using System;
using System.IO;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class MenuWorkshopSpawnsUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekScrollView spawnsBox;

    private static SleekButtonState typeButton;

    private static ISleekField viewIDField;

    private static ISleekButton viewButton;

    private static ISleekButton rawButton;

    private static ISleekButton newButton;

    private static ISleekButton writeButton;

    private static ISleekBox rootsBox;

    private static ISleekBox tablesBox;

    private static ISleekField rawField;

    private static ISleekField addRootIDField;

    private static SleekButtonIcon addRootSpawnButton;

    private static ISleekField addTableIDField;

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

    private static SpawnAsset FindCurrentAsset()
    {
        if (ushort.TryParse(viewIDField.Text, out var result))
        {
            return Assets.find(EAssetType.SPAWN, result) as SpawnAsset;
        }
        if (Guid.TryParse(viewIDField.Text, out var result2))
        {
            return Assets.find(result2) as SpawnAsset;
        }
        return null;
    }

    private static void refresh()
    {
        rawField.IsVisible = false;
        rootsBox.IsVisible = true;
        tablesBox.IsVisible = true;
        rootsBox.RemoveAllChildren();
        tablesBox.RemoveAllChildren();
        MenuWorkshopSpawnsUI.asset = FindCurrentAsset();
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
        rootsBox.PositionOffset_Y = num;
        num += 40;
        if (MenuWorkshopSpawnsUI.asset != null)
        {
            rootsBox.Text = localization.format("Roots_Box", MenuWorkshopSpawnsUI.asset.name);
            for (int i = 0; i < MenuWorkshopSpawnsUI.asset.roots.Count; i++)
            {
                SpawnTable spawnTable = MenuWorkshopSpawnsUI.asset.roots[i];
                SpawnAsset spawnAsset;
                if (spawnTable.legacySpawnId != 0)
                {
                    spawnAsset = Assets.find(EAssetType.SPAWN, spawnTable.legacySpawnId) as SpawnAsset;
                }
                else
                {
                    if (spawnTable.targetGuid.IsEmpty())
                    {
                        continue;
                    }
                    spawnAsset = Assets.find(spawnTable.targetGuid) as SpawnAsset;
                }
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.PositionOffset_Y = 40 + i * 40;
                sleekButton.SizeOffset_X = -260f;
                sleekButton.SizeScale_X = 1f;
                sleekButton.SizeOffset_Y = 30f;
                sleekButton.OnClicked += onClickedRootButton;
                rootsBox.AddChild(sleekButton);
                num += 40;
                if (spawnAsset != null)
                {
                    sleekButton.Text = spawnAsset.name;
                    if (spawnTable.legacySpawnId != 0)
                    {
                        sleekButton.TooltipText = $"{spawnTable.legacySpawnId} - {spawnAsset.GetOriginName()}";
                    }
                    else
                    {
                        sleekButton.TooltipText = $"{spawnTable.targetGuid:N} - {spawnAsset.GetOriginName()}";
                    }
                }
                else if (spawnTable.legacySpawnId != 0)
                {
                    sleekButton.Text = $"{spawnTable.legacySpawnId} ?";
                }
                else
                {
                    sleekButton.Text = $"{spawnTable.targetGuid:N} ?";
                }
                ISleekInt32Field sleekInt32Field = Glazier.Get().CreateInt32Field();
                sleekInt32Field.PositionOffset_X = 10f;
                sleekInt32Field.PositionScale_X = 1f;
                sleekInt32Field.SizeOffset_X = 55f;
                sleekInt32Field.SizeOffset_Y = 30f;
                sleekInt32Field.Value = spawnTable.weight;
                sleekInt32Field.TooltipText = localization.format("Weight_Tooltip");
                sleekInt32Field.OnValueChanged += onTypedRootWeightField;
                sleekButton.AddChild(sleekInt32Field);
                ISleekBox sleekBox = Glazier.Get().CreateBox();
                sleekBox.PositionOffset_X = 75f;
                sleekBox.PositionScale_X = 1f;
                sleekBox.SizeOffset_X = 55f;
                sleekBox.SizeOffset_Y = 30f;
                sleekBox.Text = spawnTable.normalizedWeight.ToString("P");
                sleekBox.TooltipText = localization.format("Chance_Tooltip");
                sleekButton.AddChild(sleekBox);
                SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
                sleekButtonIcon.PositionOffset_X = 140f;
                sleekButtonIcon.PositionScale_X = 1f;
                sleekButtonIcon.SizeOffset_X = 120f;
                sleekButtonIcon.SizeOffset_Y = 30f;
                sleekButtonIcon.text = localization.format("Remove_Root_Button");
                sleekButtonIcon.tooltip = localization.format("Remove_Root_Button_Tooltip");
                sleekButtonIcon.onClickedButton += onClickedRemoveRootButton;
                sleekButton.AddChild(sleekButtonIcon);
            }
            addRootIDField.PositionOffset_Y = num;
            addRootSpawnButton.PositionOffset_Y = num;
            num += 40;
            addRootIDField.IsVisible = true;
            addRootSpawnButton.IsVisible = true;
        }
        else
        {
            rootsBox.Text = localization.format("Roots_Box", viewIDField.Text + " ?");
            addRootIDField.IsVisible = false;
            addRootSpawnButton.IsVisible = false;
        }
        num += 40;
        tablesBox.PositionOffset_Y = num;
        num += 40;
        if (MenuWorkshopSpawnsUI.asset != null)
        {
            tablesBox.Text = localization.format("Tables_Box", MenuWorkshopSpawnsUI.asset.name);
            for (int j = 0; j < MenuWorkshopSpawnsUI.asset.tables.Count; j++)
            {
                SpawnTable spawnTable2 = MenuWorkshopSpawnsUI.asset.tables[j];
                Asset asset;
                SpawnAsset spawnAsset2;
                bool flag;
                if (spawnTable2.legacySpawnId != 0)
                {
                    asset = null;
                    spawnAsset2 = Assets.find(EAssetType.SPAWN, spawnTable2.legacySpawnId) as SpawnAsset;
                    flag = true;
                }
                else if (spawnTable2.legacyAssetId != 0)
                {
                    asset = Assets.find(type, spawnTable2.legacyAssetId);
                    spawnAsset2 = null;
                    flag = false;
                }
                else
                {
                    asset = Assets.find(spawnTable2.targetGuid);
                    spawnAsset2 = asset as SpawnAsset;
                    flag = spawnAsset2 != null;
                }
                ISleekElement sleekElement;
                if (flag)
                {
                    ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                    sleekButton2.PositionOffset_Y = 40 + j * 40;
                    sleekButton2.SizeOffset_X = -260f;
                    sleekButton2.SizeScale_X = 1f;
                    sleekButton2.SizeOffset_Y = 30f;
                    sleekButton2.OnClicked += onClickedTableButton;
                    tablesBox.AddChild(sleekButton2);
                    sleekElement = sleekButton2;
                    num += 40;
                    if (spawnAsset2 != null)
                    {
                        sleekButton2.Text = spawnAsset2.name;
                        if (spawnTable2.legacySpawnId != 0)
                        {
                            sleekButton2.TooltipText = $"{spawnTable2.legacySpawnId} - {spawnAsset2.GetOriginName()}";
                        }
                        else
                        {
                            sleekButton2.TooltipText = $"{spawnTable2.targetGuid:N} - {spawnAsset2.GetOriginName()}";
                        }
                    }
                    else if (spawnTable2.legacySpawnId != 0)
                    {
                        sleekButton2.Text = $"{spawnTable2.legacySpawnId} ?";
                    }
                    else
                    {
                        sleekButton2.Text = $"{spawnTable2.targetGuid:N} ?";
                    }
                }
                else
                {
                    ISleekBox sleekBox2 = Glazier.Get().CreateBox();
                    sleekBox2.PositionOffset_Y = 40 + j * 40;
                    sleekBox2.SizeOffset_X = -260f;
                    sleekBox2.SizeScale_X = 1f;
                    sleekBox2.SizeOffset_Y = 30f;
                    tablesBox.AddChild(sleekBox2);
                    sleekElement = sleekBox2;
                    num += 40;
                    if (asset != null)
                    {
                        sleekBox2.Text = asset.FriendlyName;
                        if (asset is ItemAsset itemAsset)
                        {
                            sleekBox2.TextColor = ItemTool.getRarityColorUI(itemAsset.rarity);
                        }
                        else if (asset is VehicleAsset vehicleAsset)
                        {
                            sleekBox2.TextColor = ItemTool.getRarityColorUI(vehicleAsset.rarity);
                        }
                        if (spawnTable2.legacyAssetId != 0)
                        {
                            sleekBox2.TooltipText = $"{spawnTable2.legacyAssetId} - {asset.GetOriginName()}";
                        }
                        else
                        {
                            sleekBox2.TooltipText = $"{spawnTable2.targetGuid:N} - {asset.GetOriginName()}";
                        }
                    }
                    else if (spawnTable2.legacyAssetId != 0)
                    {
                        sleekBox2.Text = $"{spawnTable2.legacyAssetId} ?";
                    }
                    else
                    {
                        sleekBox2.Text = $"{spawnTable2.targetGuid:N} ?";
                    }
                }
                if (sleekElement != null)
                {
                    ISleekInt32Field sleekInt32Field2 = Glazier.Get().CreateInt32Field();
                    sleekInt32Field2.PositionOffset_X = 10f;
                    sleekInt32Field2.PositionScale_X = 1f;
                    sleekInt32Field2.SizeOffset_X = 55f;
                    sleekInt32Field2.SizeOffset_Y = 30f;
                    sleekInt32Field2.Value = spawnTable2.weight;
                    sleekInt32Field2.TooltipText = localization.format("Weight_Tooltip");
                    sleekInt32Field2.OnValueChanged += onTypedTableWeightField;
                    sleekElement.AddChild(sleekInt32Field2);
                    float num2 = spawnTable2.normalizedWeight;
                    if (j > 0)
                    {
                        num2 -= MenuWorkshopSpawnsUI.asset.tables[j - 1].normalizedWeight;
                    }
                    ISleekBox sleekBox3 = Glazier.Get().CreateBox();
                    sleekBox3.PositionOffset_X = 75f;
                    sleekBox3.PositionScale_X = 1f;
                    sleekBox3.SizeOffset_X = 55f;
                    sleekBox3.SizeOffset_Y = 30f;
                    sleekBox3.Text = num2.ToString("P");
                    sleekBox3.TooltipText = localization.format("Chance_Tooltip");
                    sleekElement.AddChild(sleekBox3);
                    SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
                    sleekButtonIcon2.PositionOffset_X = 140f;
                    sleekButtonIcon2.PositionScale_X = 1f;
                    sleekButtonIcon2.SizeOffset_X = 120f;
                    sleekButtonIcon2.SizeOffset_Y = 30f;
                    sleekButtonIcon2.text = localization.format("Remove_Table_Button");
                    sleekButtonIcon2.tooltip = localization.format("Remove_Table_Button_Tooltip");
                    sleekButtonIcon2.onClickedButton += onClickedRemoveTableButton;
                    sleekElement.AddChild(sleekButtonIcon2);
                }
            }
            addTableIDField.PositionOffset_Y = num;
            addTableAssetButton.PositionOffset_Y = num;
            addTableSpawnButton.PositionOffset_Y = num;
            num += 40;
            addTableIDField.IsVisible = true;
            addTableAssetButton.IsVisible = true;
            addTableSpawnButton.IsVisible = true;
        }
        else
        {
            tablesBox.Text = localization.format("Tables_Box", viewIDField.Text + " ?");
            addTableIDField.IsVisible = false;
            addTableAssetButton.IsVisible = false;
            addTableSpawnButton.IsVisible = false;
        }
        if (MenuWorkshopSpawnsUI.asset != null)
        {
            applyWeightsButton.PositionOffset_Y = num;
            num += 40;
            applyWeightsButton.IsVisible = true;
        }
        else
        {
            applyWeightsButton.IsVisible = false;
        }
        spawnsBox.ContentSizeOffset = new Vector2(0f, num - 10);
    }

    private static string getRaw(SpawnAsset asset)
    {
        using StringWriter stringWriter = new StringWriter();
        using DatWriter datWriter = new DatWriter(stringWriter);
        datWriter.WriteKeyValue("GUID", asset.GUID);
        datWriter.WriteKeyValue("Type", "Spawn");
        datWriter.WriteKeyValue("ID", asset.id);
        bool flag = false;
        if (asset.roots != null)
        {
            foreach (SpawnTable root in asset.roots)
            {
                if (root.isLink && (root.weight > 0 || root.isOverride))
                {
                    flag = true;
                    break;
                }
            }
        }
        if (flag)
        {
            datWriter.WriteEmptyLine();
            datWriter.WriteListStart("Roots");
            foreach (SpawnTable root2 in asset.roots)
            {
                if (root2.isLink && (root2.weight > 0 || root2.isOverride))
                {
                    datWriter.WriteDictionaryStart();
                    root2.Write(datWriter, type);
                    datWriter.WriteDictionaryEnd();
                }
            }
            datWriter.WriteListEnd();
        }
        bool flag2 = false;
        if (asset.tables != null)
        {
            foreach (SpawnTable table in asset.tables)
            {
                if (!table.isLink && table.weight > 0)
                {
                    flag2 = true;
                    break;
                }
            }
        }
        if (flag2)
        {
            datWriter.WriteEmptyLine();
            datWriter.WriteListStart("Tables");
            foreach (SpawnTable table2 in asset.tables)
            {
                if (!table2.isLink && table2.weight > 0)
                {
                    datWriter.WriteDictionaryStart();
                    table2.Write(datWriter, type);
                    datWriter.WriteDictionaryEnd();
                }
            }
            datWriter.WriteListEnd();
        }
        return stringWriter.ToString();
    }

    private static void raw()
    {
        rawField.IsVisible = true;
        rootsBox.IsVisible = false;
        tablesBox.IsVisible = false;
        addRootIDField.IsVisible = false;
        addRootSpawnButton.IsVisible = false;
        addTableIDField.IsVisible = false;
        addTableAssetButton.IsVisible = false;
        addTableSpawnButton.IsVisible = false;
        applyWeightsButton.IsVisible = false;
        asset = FindCurrentAsset();
        string text = ((asset == null) ? "?" : getRaw(asset));
        rawField.Text = text;
        GUIUtility.systemCopyBuffer = text;
        spawnsBox.ContentSizeOffset = new Vector2(0f, 1080f);
    }

    private static void write()
    {
        asset = FindCurrentAsset();
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
        ushort.TryParse(viewIDField.Text, out var result);
        SpawnAsset spawnAsset = Assets.CreateAtRuntime<SpawnAsset>(result);
        if (spawnAsset != null)
        {
            viewIDField.Text = spawnAsset.GUID.ToString("N");
            refresh();
        }
    }

    private static void onClickedWriteButton(ISleekElement button)
    {
        write();
    }

    private static void onClickedRootButton(ISleekElement button)
    {
        int index = rootsBox.FindIndexOfChild(button);
        SpawnTable spawnTable = asset.roots[index];
        if (spawnTable.legacySpawnId != 0)
        {
            viewIDField.Text = spawnTable.legacySpawnId.ToString();
        }
        else
        {
            viewIDField.Text = spawnTable.targetGuid.ToString("N");
        }
        refresh();
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        int index = tablesBox.FindIndexOfChild(button);
        SpawnTable spawnTable = asset.tables[index];
        if (spawnTable.legacySpawnId != 0)
        {
            viewIDField.Text = spawnTable.legacySpawnId.ToString();
        }
        else
        {
            viewIDField.Text = spawnTable.targetGuid.ToString("N");
        }
        refresh();
    }

    private static void onTypedRootWeightField(ISleekInt32Field field, int state)
    {
        int index = rootsBox.FindIndexOfChild(field.Parent);
        asset.roots[index].weight = state;
    }

    private static void onClickedAddRootSpawnButton(ISleekElement button)
    {
        ushort result;
        Guid result2;
        SpawnAsset spawnAsset = (ushort.TryParse(addRootIDField.Text, out result) ? (Assets.find(EAssetType.SPAWN, result) as SpawnAsset) : ((!Guid.TryParse(addRootIDField.Text, out result2)) ? null : Assets.find<SpawnAsset>(result2)));
        if (spawnAsset == null)
        {
            UnturnedLog.info("Spawns editor unable to find parent spawn asset matching \"" + addRootIDField.Text + "\"");
            return;
        }
        foreach (SpawnTable root in asset.roots)
        {
            if ((root.legacySpawnId != 0 && root.legacySpawnId == spawnAsset.id) || root.targetGuid == spawnAsset.GUID)
            {
                UnturnedLog.info("Spawns editor current asset " + asset.FriendlyName + " already contains parent " + spawnAsset.FriendlyName);
                return;
            }
        }
        SpawnTable spawnTable = new SpawnTable();
        spawnTable.targetGuid = spawnAsset.GUID;
        spawnTable.isLink = true;
        asset.roots.Add(spawnTable);
        SpawnTable spawnTable2 = new SpawnTable();
        spawnTable2.targetGuid = asset.GUID;
        spawnTable2.isLink = true;
        spawnAsset.tables.Add(spawnTable2);
        spawnAsset.markTablesDirty();
        addRootIDField.Text = string.Empty;
        refresh();
    }

    private static void onClickedRemoveRootButton(ISleekElement button)
    {
        int parentIndex = rootsBox.FindIndexOfChild(button.Parent);
        asset.EditorRemoveParentAtIndex(parentIndex);
        refresh();
    }

    private static void onTypedTableWeightField(ISleekInt32Field field, int state)
    {
        int tableIndex = tablesBox.FindIndexOfChild(field.Parent);
        asset.setTableWeightAtIndex(tableIndex, state);
    }

    private static void onClickedAddTableAssetButton(ISleekElement button)
    {
        ushort result;
        Guid result2;
        Asset asset = (ushort.TryParse(addTableIDField.Text, out result) ? Assets.find(type, result) : ((!Guid.TryParse(addTableIDField.Text, out result2)) ? null : Assets.find(result2)));
        if (asset == null)
        {
            UnturnedLog.info("Spawns editor unable to find child asset matching \"" + addRootIDField.Text + "\"");
            return;
        }
        foreach (SpawnTable table in MenuWorkshopSpawnsUI.asset.tables)
        {
            if ((table.legacyAssetId != 0 && table.legacyAssetId == asset.id) || table.targetGuid == asset.GUID)
            {
                UnturnedLog.info("Spawns editor current asset " + MenuWorkshopSpawnsUI.asset.FriendlyName + " already contains child asset " + asset.FriendlyName);
                return;
            }
        }
        MenuWorkshopSpawnsUI.asset.EditorAddChild(asset);
        addTableIDField.Text = string.Empty;
        refresh();
    }

    private static void onClickedAddTableSpawnButton(ISleekElement button)
    {
        ushort result;
        Guid result2;
        SpawnAsset spawnAsset = (ushort.TryParse(addTableIDField.Text, out result) ? (Assets.find(EAssetType.SPAWN, result) as SpawnAsset) : ((!Guid.TryParse(addTableIDField.Text, out result2)) ? null : (Assets.find(result2) as SpawnAsset)));
        if (spawnAsset == null)
        {
            UnturnedLog.info("Spawns editor unable to find child spawn matching \"" + addTableIDField.Text + "\"");
            return;
        }
        foreach (SpawnTable table in asset.tables)
        {
            if ((table.legacySpawnId != 0 && table.legacySpawnId == spawnAsset.id) || table.targetGuid == spawnAsset.GUID)
            {
                UnturnedLog.info("Spawns editor current asset " + asset.FriendlyName + " already contains child spawn " + spawnAsset.FriendlyName);
                return;
            }
        }
        asset.EditorAddChild(spawnAsset);
        addTableIDField.Text = string.Empty;
        refresh();
    }

    private static void onClickedRemoveTableButton(ISleekElement button)
    {
        int childIndex = tablesBox.FindIndexOfChild(button.Parent);
        asset.EditorRemoveChildAtIndex(childIndex);
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        spawnsBox = Glazier.Get().CreateScrollView();
        spawnsBox.PositionOffset_X = -315f;
        spawnsBox.PositionOffset_Y = 100f;
        spawnsBox.PositionScale_X = 0.5f;
        spawnsBox.SizeOffset_X = 630f;
        spawnsBox.SizeOffset_Y = -200f;
        spawnsBox.SizeScale_Y = 1f;
        spawnsBox.ScaleContentToWidth = true;
        container.AddChild(spawnsBox);
        typeButton = new SleekButtonState(new GUIContent(localization.format("Type_Item")), new GUIContent(localization.format("Type_Vehicle")), new GUIContent(localization.format("Type_Animal")));
        typeButton.SizeOffset_X = 600f;
        typeButton.SizeOffset_Y = 30f;
        typeButton.tooltip = localization.format("Type_Tooltip");
        spawnsBox.AddChild(typeButton);
        viewIDField = Glazier.Get().CreateStringField();
        viewIDField.PositionOffset_Y = 40f;
        viewIDField.SizeOffset_X = 160f;
        viewIDField.SizeOffset_Y = 30f;
        viewIDField.PlaceholderText = localization.format("ID_Field_Hint");
        spawnsBox.AddChild(viewIDField);
        viewButton = Glazier.Get().CreateButton();
        viewButton.PositionOffset_X = 170f;
        viewButton.PositionOffset_Y = 40f;
        viewButton.SizeOffset_X = 100f;
        viewButton.SizeOffset_Y = 30f;
        viewButton.Text = localization.format("View_Button");
        viewButton.TooltipText = localization.format("View_Button_Tooltip");
        viewButton.OnClicked += onClickedViewButton;
        spawnsBox.AddChild(viewButton);
        rawButton = Glazier.Get().CreateButton();
        rawButton.PositionOffset_X = 280f;
        rawButton.PositionOffset_Y = 40f;
        rawButton.SizeOffset_X = 100f;
        rawButton.SizeOffset_Y = 30f;
        rawButton.Text = localization.format("Raw_Button");
        rawButton.TooltipText = localization.format("Raw_Button_Tooltip");
        rawButton.OnClicked += onClickedRawButton;
        spawnsBox.AddChild(rawButton);
        newButton = Glazier.Get().CreateButton();
        newButton.PositionOffset_X = 390f;
        newButton.PositionOffset_Y = 40f;
        newButton.SizeOffset_X = 100f;
        newButton.SizeOffset_Y = 30f;
        newButton.Text = localization.format("New_Button");
        newButton.TooltipText = localization.format("New_Button_Tooltip");
        newButton.OnClicked += onClickedNewButton;
        spawnsBox.AddChild(newButton);
        writeButton = Glazier.Get().CreateButton();
        writeButton.PositionOffset_X = 500f;
        writeButton.PositionOffset_Y = 40f;
        writeButton.SizeOffset_X = 100f;
        writeButton.SizeOffset_Y = 30f;
        writeButton.Text = localization.format("Write_Button");
        writeButton.TooltipText = localization.format("Write_Button_Tooltip");
        writeButton.OnClicked += onClickedWriteButton;
        spawnsBox.AddChild(writeButton);
        addRootIDField = Glazier.Get().CreateStringField();
        addRootIDField.SizeOffset_X = 470f;
        addRootIDField.SizeOffset_Y = 30f;
        addRootIDField.PlaceholderText = localization.format("ID_Field_Hint");
        spawnsBox.AddChild(addRootIDField);
        addRootSpawnButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
        addRootSpawnButton.PositionOffset_X = 480f;
        addRootSpawnButton.SizeOffset_X = 120f;
        addRootSpawnButton.SizeOffset_Y = 30f;
        addRootSpawnButton.text = localization.format("Add_Root_Spawn_Button");
        addRootSpawnButton.tooltip = localization.format("Add_Root_Spawn_Button_Tooltip");
        addRootSpawnButton.onClickedButton += onClickedAddRootSpawnButton;
        spawnsBox.AddChild(addRootSpawnButton);
        addTableIDField = Glazier.Get().CreateStringField();
        addTableIDField.SizeOffset_X = 340f;
        addTableIDField.SizeOffset_Y = 30f;
        addTableIDField.PlaceholderText = localization.format("ID_Field_Hint");
        spawnsBox.AddChild(addTableIDField);
        addTableAssetButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
        addTableAssetButton.PositionOffset_X = 350f;
        addTableAssetButton.SizeOffset_X = 120f;
        addTableAssetButton.SizeOffset_Y = 30f;
        addTableAssetButton.text = localization.format("Add_Table_Asset_Button");
        addTableAssetButton.tooltip = localization.format("Add_Table_Asset_Button_Tooltip");
        addTableAssetButton.onClickedButton += onClickedAddTableAssetButton;
        spawnsBox.AddChild(addTableAssetButton);
        addTableSpawnButton = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
        addTableSpawnButton.PositionOffset_X = 480f;
        addTableSpawnButton.SizeOffset_X = 120f;
        addTableSpawnButton.SizeOffset_Y = 30f;
        addTableSpawnButton.text = localization.format("Add_Table_Spawn_Button");
        addTableSpawnButton.tooltip = localization.format("Add_Table_Spawn_Button_Tooltip");
        addTableSpawnButton.onClickedButton += onClickedAddTableSpawnButton;
        spawnsBox.AddChild(addTableSpawnButton);
        applyWeightsButton = Glazier.Get().CreateButton();
        applyWeightsButton.SizeOffset_X = 600f;
        applyWeightsButton.SizeOffset_Y = 30f;
        applyWeightsButton.Text = localization.format("Apply_Weights_Button");
        applyWeightsButton.TooltipText = localization.format("Apply_Weights_Button_Tooltip");
        applyWeightsButton.OnClicked += onClickedApplyWeightsButton;
        spawnsBox.AddChild(applyWeightsButton);
        rootsBox = Glazier.Get().CreateBox();
        rootsBox.PositionOffset_Y = 40f;
        rootsBox.SizeOffset_X = 600f;
        rootsBox.SizeOffset_Y = 30f;
        spawnsBox.AddChild(rootsBox);
        tablesBox = Glazier.Get().CreateBox();
        tablesBox.PositionOffset_Y = 80f;
        tablesBox.SizeOffset_X = 600f;
        tablesBox.SizeOffset_Y = 30f;
        spawnsBox.AddChild(tablesBox);
        rawField = Glazier.Get().CreateStringField();
        rawField.PositionOffset_Y = 80f;
        rawField.SizeOffset_X = 600f;
        rawField.SizeOffset_Y = 1000f;
        rawField.IsMultiline = true;
        rawField.MaxLength = 4096;
        rawField.TextAlignment = TextAnchor.UpperLeft;
        spawnsBox.AddChild(rawField);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
