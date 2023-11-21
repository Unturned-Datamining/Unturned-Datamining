using UnityEngine;

namespace SDG.Unturned;

public class EditorSpawnsItemsUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekScrollView tableScrollBox;

    private static ISleekScrollView spawnsScrollBox;

    private static ISleekButton[] tableButtons;

    private static ISleekButton[] tierButtons;

    private static ISleekButton[] itemButtons;

    private static SleekColorPicker tableColorPicker;

    private static ISleekUInt16Field tableIDField;

    private static ISleekField tierNameField;

    private static SleekButtonIcon addTierButton;

    private static SleekButtonIcon removeTierButton;

    private static ISleekUInt16Field itemIDField;

    private static SleekButtonIcon addItemButton;

    private static SleekButtonIcon removeItemButton;

    private static ISleekBox selectedBox;

    private static ISleekField tableNameField;

    private static SleekButtonIcon addTableButton;

    private static SleekButtonIcon removeTableButton;

    private static ISleekSlider radiusSlider;

    private static SleekButtonIcon addButton;

    private static SleekButtonIcon removeButton;

    private static byte selectedTier;

    private static byte selectItem;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorSpawns.isSpawning = true;
            EditorSpawns.spawnMode = ESpawnMode.ADD_ITEM;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            EditorSpawns.isSpawning = false;
            container.AnimateOutOfView(1f, 0f);
        }
    }

    public static void updateTables()
    {
        if (tableButtons != null)
        {
            for (int i = 0; i < tableButtons.Length; i++)
            {
                tableScrollBox.RemoveChild(tableButtons[i]);
            }
        }
        tableButtons = new ISleekButton[LevelItems.tables.Count];
        tableScrollBox.ContentSizeOffset = new Vector2(0f, tableButtons.Length * 40 - 10);
        for (int j = 0; j < tableButtons.Length; j++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = 240f;
            sleekButton.PositionOffset_Y = j * 40;
            sleekButton.SizeOffset_X = 200f;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.Text = j + " " + LevelItems.tables[j].name;
            sleekButton.OnClicked += onClickedTableButton;
            tableScrollBox.AddChild(sleekButton);
            tableButtons[j] = sleekButton;
        }
    }

    public static void updateSelection()
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count)
        {
            ItemTable itemTable = LevelItems.tables[EditorSpawns.selectedItem];
            selectedBox.Text = itemTable.name;
            tableNameField.Text = itemTable.name;
            tableIDField.Value = itemTable.tableID;
            tableColorPicker.state = itemTable.color;
            if (tierButtons != null)
            {
                for (int i = 0; i < tierButtons.Length; i++)
                {
                    spawnsScrollBox.RemoveChild(tierButtons[i]);
                }
            }
            tierButtons = new ISleekButton[itemTable.tiers.Count];
            for (int j = 0; j < tierButtons.Length; j++)
            {
                ItemTier itemTier = itemTable.tiers[j];
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.PositionOffset_X = 240f;
                sleekButton.PositionOffset_Y = 170 + j * 70;
                sleekButton.SizeOffset_X = 200f;
                sleekButton.SizeOffset_Y = 30f;
                sleekButton.Text = itemTier.name;
                sleekButton.OnClicked += onClickedTierButton;
                spawnsScrollBox.AddChild(sleekButton);
                ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
                sleekSlider.PositionOffset_Y = 40f;
                sleekSlider.SizeOffset_X = 200f;
                sleekSlider.SizeOffset_Y = 20f;
                sleekSlider.Orientation = ESleekOrientation.HORIZONTAL;
                sleekSlider.Value = itemTier.chance;
                sleekSlider.AddLabel(Mathf.RoundToInt(itemTier.chance * 100f) + "%", ESleekSide.LEFT);
                sleekSlider.OnValueChanged += onDraggedChanceSlider;
                sleekButton.AddChild(sleekSlider);
                tierButtons[j] = sleekButton;
            }
            tierNameField.PositionOffset_Y = 170 + tierButtons.Length * 70;
            addTierButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            removeTierButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            if (itemButtons != null)
            {
                for (int k = 0; k < itemButtons.Length; k++)
                {
                    spawnsScrollBox.RemoveChild(itemButtons[k]);
                }
            }
            if (selectedTier < itemTable.tiers.Count)
            {
                tierNameField.Text = itemTable.tiers[selectedTier].name;
                itemButtons = new ISleekButton[itemTable.tiers[selectedTier].table.Count];
                for (int l = 0; l < itemButtons.Length; l++)
                {
                    ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                    sleekButton2.PositionOffset_X = 240f;
                    sleekButton2.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + l * 40;
                    sleekButton2.SizeOffset_X = 200f;
                    sleekButton2.SizeOffset_Y = 30f;
                    ItemAsset itemAsset = Assets.find(EAssetType.ITEM, itemTable.tiers[selectedTier].table[l].item) as ItemAsset;
                    string text = "?";
                    if (itemAsset != null)
                    {
                        text = ((!string.IsNullOrEmpty(itemAsset.itemName)) ? itemAsset.itemName : itemAsset.name);
                    }
                    sleekButton2.Text = itemTable.tiers[selectedTier].table[l].item + " " + text;
                    sleekButton2.OnClicked += onClickItemButton;
                    spawnsScrollBox.AddChild(sleekButton2);
                    itemButtons[l] = sleekButton2;
                }
            }
            else
            {
                tierNameField.Text = "";
                itemButtons = new ISleekButton[0];
            }
            itemIDField.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + itemButtons.Length * 40;
            addItemButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + itemButtons.Length * 40 + 40;
            removeItemButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + itemButtons.Length * 40 + 40;
            spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 170 + tierButtons.Length * 70 + 80 + itemButtons.Length * 40 + 70);
            return;
        }
        selectedBox.Text = "";
        tableNameField.Text = "";
        tableIDField.Value = 0;
        tableColorPicker.state = Color.white;
        if (tierButtons != null)
        {
            for (int m = 0; m < tierButtons.Length; m++)
            {
                spawnsScrollBox.RemoveChild(tierButtons[m]);
            }
        }
        tierButtons = null;
        tierNameField.Text = "";
        tierNameField.PositionOffset_Y = 170f;
        addTierButton.PositionOffset_Y = 210f;
        removeTierButton.PositionOffset_Y = 210f;
        if (itemButtons != null)
        {
            for (int n = 0; n < itemButtons.Length; n++)
            {
                spawnsScrollBox.RemoveChild(itemButtons[n]);
            }
        }
        itemButtons = null;
        itemIDField.PositionOffset_Y = 250f;
        addItemButton.PositionOffset_Y = 290f;
        removeItemButton.PositionOffset_Y = 290f;
        spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 320f);
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem != (byte)(button.PositionOffset_Y / 40f))
        {
            EditorSpawns.selectedItem = (byte)(button.PositionOffset_Y / 40f);
            EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = LevelItems.tables[EditorSpawns.selectedItem].color;
        }
        else
        {
            EditorSpawns.selectedItem = byte.MaxValue;
            EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = Color.white;
        }
        updateSelection();
    }

    private static void onItemColorPicked(SleekColorPicker picker, Color color)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count)
        {
            LevelItems.tables[EditorSpawns.selectedItem].color = color;
        }
    }

    private static void onTableIDFieldTyped(ISleekUInt16Field field, ushort state)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count)
        {
            LevelItems.tables[EditorSpawns.selectedItem].tableID = state;
        }
    }

    private static void onClickedTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count)
        {
            if (selectedTier != (byte)((button.PositionOffset_Y - 170f) / 70f))
            {
                selectedTier = (byte)((button.PositionOffset_Y - 170f) / 70f);
            }
            else
            {
                selectedTier = byte.MaxValue;
            }
            updateSelection();
        }
    }

    private static void onClickItemButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count)
        {
            selectItem = (byte)((button.PositionOffset_Y - 170f - (float)(tierButtons.Length * 70) - 80f) / 40f);
            updateSelection();
        }
    }

    private static void onDraggedChanceSlider(ISleekSlider slider, float state)
    {
        if (EditorSpawns.selectedItem >= LevelItems.tables.Count)
        {
            return;
        }
        int num = Mathf.FloorToInt((slider.Parent.PositionOffset_Y - 170f) / 70f);
        LevelItems.tables[EditorSpawns.selectedItem].updateChance(num, state);
        for (int i = 0; i < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count; i++)
        {
            ItemTier itemTier = LevelItems.tables[EditorSpawns.selectedItem].tiers[i];
            ISleekSlider sleekSlider = (ISleekSlider)tierButtons[i].GetChildAtIndex(0);
            if (i != num)
            {
                sleekSlider.Value = itemTier.chance;
            }
            sleekSlider.UpdateLabel(Mathf.RoundToInt(itemTier.chance * 100f) + "%");
        }
    }

    private static void onTypedTableNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count)
        {
            selectedBox.Text = state;
            LevelItems.tables[EditorSpawns.selectedItem].name = state;
            tableButtons[EditorSpawns.selectedItem].Text = EditorSpawns.selectedItem + " " + state;
        }
    }

    private static void onClickedAddTableButton(ISleekElement button)
    {
        if (tableNameField.Text != "")
        {
            LevelItems.addTable(tableNameField.Text);
            tableNameField.Text = "";
            updateTables();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onClickedRemoveTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count)
        {
            LevelItems.removeTable();
            updateTables();
            updateSelection();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onTypedTierNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count && selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count)
        {
            LevelItems.tables[EditorSpawns.selectedItem].tiers[selectedTier].name = state;
            tierButtons[selectedTier].Text = state;
        }
    }

    private static void onClickedAddTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count && tierNameField.Text != "")
        {
            LevelItems.tables[EditorSpawns.selectedItem].addTier(tierNameField.Text);
            tierNameField.Text = "";
            updateSelection();
        }
    }

    private static void onClickedRemoveTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count && selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count)
        {
            LevelItems.tables[EditorSpawns.selectedItem].removeTier(selectedTier);
            updateSelection();
        }
    }

    private static void onClickedAddItemButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count && selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count)
        {
            if (Assets.find(EAssetType.ITEM, itemIDField.Value) is ItemAsset { isPro: false })
            {
                LevelItems.tables[EditorSpawns.selectedItem].addItem(selectedTier, itemIDField.Value);
                updateSelection();
                spawnsScrollBox.ScrollToBottom();
            }
            itemIDField.Value = 0;
        }
    }

    private static void onClickedRemoveItemButton(ISleekElement button)
    {
        if (EditorSpawns.selectedItem < LevelItems.tables.Count && selectedTier < LevelItems.tables[EditorSpawns.selectedItem].tiers.Count && selectItem < LevelItems.tables[EditorSpawns.selectedItem].tiers[selectedTier].table.Count)
        {
            LevelItems.tables[EditorSpawns.selectedItem].removeItem(selectedTier, selectItem);
            updateSelection();
            spawnsScrollBox.ScrollToBottom();
        }
    }

    private static void onDraggedRadiusSlider(ISleekSlider slider, float state)
    {
        EditorSpawns.radius = (byte)((float)(int)EditorSpawns.MIN_REMOVE_SIZE + state * (float)(int)EditorSpawns.MAX_REMOVE_SIZE);
    }

    private static void onClickedAddButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.ADD_ITEM;
    }

    private static void onClickedRemoveButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.REMOVE_ITEM;
    }

    public EditorSpawnsItemsUI()
    {
        Local local = Localization.read("/Editor/EditorSpawnsItems.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorSpawnsItems/EditorSpawnsItems.unity3d");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        tableScrollBox = Glazier.Get().CreateScrollView();
        tableScrollBox.PositionOffset_X = -470f;
        tableScrollBox.PositionOffset_Y = 120f;
        tableScrollBox.PositionScale_X = 1f;
        tableScrollBox.SizeOffset_X = 470f;
        tableScrollBox.SizeOffset_Y = 200f;
        tableScrollBox.ScaleContentToWidth = true;
        container.AddChild(tableScrollBox);
        tableNameField = Glazier.Get().CreateStringField();
        tableNameField.PositionOffset_X = -230f;
        tableNameField.PositionOffset_Y = 330f;
        tableNameField.PositionScale_X = 1f;
        tableNameField.SizeOffset_X = 230f;
        tableNameField.SizeOffset_Y = 30f;
        tableNameField.MaxLength = 64;
        tableNameField.AddLabel(local.format("TableNameFieldLabelText"), ESleekSide.LEFT);
        tableNameField.OnTextChanged += onTypedTableNameField;
        container.AddChild(tableNameField);
        addTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addTableButton.PositionOffset_X = -230f;
        addTableButton.PositionOffset_Y = 370f;
        addTableButton.PositionScale_X = 1f;
        addTableButton.SizeOffset_X = 110f;
        addTableButton.SizeOffset_Y = 30f;
        addTableButton.text = local.format("AddTableButtonText");
        addTableButton.tooltip = local.format("AddTableButtonTooltip");
        addTableButton.onClickedButton += onClickedAddTableButton;
        container.AddChild(addTableButton);
        removeTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeTableButton.PositionOffset_X = -110f;
        removeTableButton.PositionOffset_Y = 370f;
        removeTableButton.PositionScale_X = 1f;
        removeTableButton.SizeOffset_X = 110f;
        removeTableButton.SizeOffset_Y = 30f;
        removeTableButton.text = local.format("RemoveTableButtonText");
        removeTableButton.tooltip = local.format("RemoveTableButtonTooltip");
        removeTableButton.onClickedButton += onClickedRemoveTableButton;
        container.AddChild(removeTableButton);
        tableButtons = null;
        updateTables();
        spawnsScrollBox = Glazier.Get().CreateScrollView();
        spawnsScrollBox.PositionOffset_X = -470f;
        spawnsScrollBox.PositionOffset_Y = 410f;
        spawnsScrollBox.PositionScale_X = 1f;
        spawnsScrollBox.SizeOffset_X = 470f;
        spawnsScrollBox.SizeOffset_Y = -410f;
        spawnsScrollBox.SizeScale_Y = 1f;
        spawnsScrollBox.ScaleContentToWidth = true;
        spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 1000f);
        container.AddChild(spawnsScrollBox);
        tableColorPicker = new SleekColorPicker();
        tableColorPicker.PositionOffset_X = 200f;
        tableColorPicker.onColorPicked = onItemColorPicked;
        spawnsScrollBox.AddChild(tableColorPicker);
        tableIDField = Glazier.Get().CreateUInt16Field();
        tableIDField.PositionOffset_X = 240f;
        tableIDField.PositionOffset_Y = 130f;
        tableIDField.SizeOffset_X = 200f;
        tableIDField.SizeOffset_Y = 30f;
        tableIDField.OnValueChanged += onTableIDFieldTyped;
        tableIDField.AddLabel(local.format("TableIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(tableIDField);
        tierNameField = Glazier.Get().CreateStringField();
        tierNameField.PositionOffset_X = 240f;
        tierNameField.SizeOffset_X = 200f;
        tierNameField.SizeOffset_Y = 30f;
        tierNameField.MaxLength = 64;
        tierNameField.AddLabel(local.format("TierNameFieldLabelText"), ESleekSide.LEFT);
        tierNameField.OnTextChanged += onTypedTierNameField;
        spawnsScrollBox.AddChild(tierNameField);
        addTierButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addTierButton.PositionOffset_X = 240f;
        addTierButton.SizeOffset_X = 95f;
        addTierButton.SizeOffset_Y = 30f;
        addTierButton.text = local.format("AddTierButtonText");
        addTierButton.tooltip = local.format("AddTierButtonTooltip");
        addTierButton.onClickedButton += onClickedAddTierButton;
        spawnsScrollBox.AddChild(addTierButton);
        removeTierButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeTierButton.PositionOffset_X = 345f;
        removeTierButton.SizeOffset_X = 95f;
        removeTierButton.SizeOffset_Y = 30f;
        removeTierButton.text = local.format("RemoveTierButtonText");
        removeTierButton.tooltip = local.format("RemoveTierButtonTooltip");
        removeTierButton.onClickedButton += onClickedRemoveTierButton;
        spawnsScrollBox.AddChild(removeTierButton);
        itemIDField = Glazier.Get().CreateUInt16Field();
        itemIDField.PositionOffset_X = 240f;
        itemIDField.SizeOffset_X = 200f;
        itemIDField.SizeOffset_Y = 30f;
        itemIDField.AddLabel(local.format("ItemIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(itemIDField);
        addItemButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addItemButton.PositionOffset_X = 240f;
        addItemButton.SizeOffset_X = 95f;
        addItemButton.SizeOffset_Y = 30f;
        addItemButton.text = local.format("AddItemButtonText");
        addItemButton.tooltip = local.format("AddItemButtonTooltip");
        addItemButton.onClickedButton += onClickedAddItemButton;
        spawnsScrollBox.AddChild(addItemButton);
        removeItemButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeItemButton.PositionOffset_X = 345f;
        removeItemButton.SizeOffset_X = 95f;
        removeItemButton.SizeOffset_Y = 30f;
        removeItemButton.text = local.format("RemoveItemButtonText");
        removeItemButton.tooltip = local.format("RemoveItemButtonTooltip");
        removeItemButton.onClickedButton += onClickedRemoveItemButton;
        spawnsScrollBox.AddChild(removeItemButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = -230f;
        selectedBox.PositionOffset_Y = 80f;
        selectedBox.PositionScale_X = 1f;
        selectedBox.SizeOffset_X = 230f;
        selectedBox.SizeOffset_Y = 30f;
        selectedBox.AddLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        tierButtons = null;
        itemButtons = null;
        updateSelection();
        radiusSlider = Glazier.Get().CreateSlider();
        radiusSlider.PositionOffset_Y = -100f;
        radiusSlider.PositionScale_Y = 1f;
        radiusSlider.SizeOffset_X = 200f;
        radiusSlider.SizeOffset_Y = 20f;
        radiusSlider.Value = (float)(EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float)(int)EditorSpawns.MAX_REMOVE_SIZE;
        radiusSlider.Orientation = ESleekOrientation.HORIZONTAL;
        radiusSlider.AddLabel(local.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
        radiusSlider.OnValueChanged += onDraggedRadiusSlider;
        container.AddChild(radiusSlider);
        addButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addButton.PositionOffset_Y = -70f;
        addButton.PositionScale_Y = 1f;
        addButton.SizeOffset_X = 200f;
        addButton.SizeOffset_Y = 30f;
        addButton.text = local.format("AddButtonText", ControlsSettings.tool_0);
        addButton.tooltip = local.format("AddButtonTooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeButton.PositionOffset_Y = -30f;
        removeButton.PositionScale_Y = 1f;
        removeButton.SizeOffset_X = 200f;
        removeButton.SizeOffset_Y = 30f;
        removeButton.text = local.format("RemoveButtonText", ControlsSettings.tool_1);
        removeButton.tooltip = local.format("RemoveButtonTooltip");
        removeButton.onClickedButton += onClickedRemoveButton;
        container.AddChild(removeButton);
        bundle.unload();
    }
}
