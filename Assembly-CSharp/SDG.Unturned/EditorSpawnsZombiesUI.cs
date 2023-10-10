using UnityEngine;

namespace SDG.Unturned;

public class EditorSpawnsZombiesUI
{
    private static SleekFullscreenBox container;

    private static Local localization;

    public static bool active;

    private static ISleekScrollView tableScrollBox;

    private static ISleekScrollView spawnsScrollBox;

    private static ISleekButton[] tableButtons;

    private static ISleekButton[] slotButtons;

    private static ISleekButton[] clothButtons;

    private static SleekColorPicker tableColorPicker;

    private static ISleekToggle megaToggle;

    private static ISleekUInt16Field healthField;

    private static ISleekUInt8Field damageField;

    private static ISleekUInt8Field lootIndexField;

    private static ISleekUInt16Field lootIDField;

    private static ISleekUInt32Field xpField;

    private static ISleekFloat32Field regenField;

    private static ISleekField difficultyGUIDField;

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

    private static byte selectedSlot;

    private static byte selectItem;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorSpawns.isSpawning = true;
            EditorSpawns.spawnMode = ESpawnMode.ADD_ZOMBIE;
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
        tableButtons = new ISleekButton[LevelZombies.tables.Count];
        tableScrollBox.ContentSizeOffset = new Vector2(0f, tableButtons.Length * 40 - 10);
        for (int j = 0; j < tableButtons.Length; j++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = 240f;
            sleekButton.PositionOffset_Y = j * 40;
            sleekButton.SizeOffset_X = 200f;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.Text = j + " " + LevelZombies.tables[j].name;
            sleekButton.OnClicked += onClickedTableButton;
            tableScrollBox.AddChild(sleekButton);
            tableButtons[j] = sleekButton;
        }
    }

    public static void updateSelection()
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            ZombieTable zombieTable = LevelZombies.tables[EditorSpawns.selectedZombie];
            selectedBox.Text = zombieTable.name;
            tableNameField.Text = zombieTable.name;
            tableColorPicker.state = zombieTable.color;
            megaToggle.Value = zombieTable.isMega;
            healthField.Value = zombieTable.health;
            damageField.Value = zombieTable.damage;
            lootIndexField.Value = zombieTable.lootIndex;
            lootIDField.Value = zombieTable.lootID;
            xpField.Value = zombieTable.xp;
            regenField.Value = zombieTable.regen;
            difficultyGUIDField.Text = zombieTable.difficultyGUID;
            if (slotButtons != null)
            {
                for (int i = 0; i < slotButtons.Length; i++)
                {
                    spawnsScrollBox.RemoveChild(slotButtons[i]);
                }
            }
            slotButtons = new ISleekButton[zombieTable.slots.Length];
            for (int j = 0; j < slotButtons.Length; j++)
            {
                ZombieSlot zombieSlot = zombieTable.slots[j];
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.PositionOffset_X = 240f;
                sleekButton.PositionOffset_Y = 460 + j * 70;
                sleekButton.SizeOffset_X = 200f;
                sleekButton.SizeOffset_Y = 30f;
                sleekButton.Text = localization.format("Slot_" + j);
                sleekButton.OnClicked += onClickedSlotButton;
                spawnsScrollBox.AddChild(sleekButton);
                ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
                sleekSlider.PositionOffset_Y = 40f;
                sleekSlider.SizeOffset_X = 200f;
                sleekSlider.SizeOffset_Y = 20f;
                sleekSlider.Orientation = ESleekOrientation.HORIZONTAL;
                sleekSlider.Value = zombieSlot.chance;
                sleekSlider.AddLabel(Mathf.RoundToInt(zombieSlot.chance * 100f) + "%", ESleekSide.LEFT);
                sleekSlider.OnValueChanged += onDraggedChanceSlider;
                sleekButton.AddChild(sleekSlider);
                slotButtons[j] = sleekButton;
            }
            if (clothButtons != null)
            {
                for (int k = 0; k < clothButtons.Length; k++)
                {
                    spawnsScrollBox.RemoveChild(clothButtons[k]);
                }
            }
            if (selectedSlot < zombieTable.slots.Length)
            {
                clothButtons = new ISleekButton[zombieTable.slots[selectedSlot].table.Count];
                for (int l = 0; l < clothButtons.Length; l++)
                {
                    ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                    sleekButton2.PositionOffset_X = 240f;
                    sleekButton2.PositionOffset_Y = 460 + slotButtons.Length * 70 + l * 40;
                    sleekButton2.SizeOffset_X = 200f;
                    sleekButton2.SizeOffset_Y = 30f;
                    ItemAsset itemAsset = Assets.find(EAssetType.ITEM, zombieTable.slots[selectedSlot].table[l].item) as ItemAsset;
                    string text = "?";
                    if (itemAsset != null)
                    {
                        text = ((!string.IsNullOrEmpty(itemAsset.itemName)) ? itemAsset.itemName : itemAsset.name);
                    }
                    sleekButton2.Text = zombieTable.slots[selectedSlot].table[l].item + " " + text;
                    sleekButton2.OnClicked += onClickItemButton;
                    spawnsScrollBox.AddChild(sleekButton2);
                    clothButtons[l] = sleekButton2;
                }
            }
            else
            {
                clothButtons = new ISleekButton[0];
            }
            itemIDField.PositionOffset_Y = 460 + slotButtons.Length * 70 + clothButtons.Length * 40;
            addItemButton.PositionOffset_Y = 460 + slotButtons.Length * 70 + clothButtons.Length * 40 + 40;
            removeItemButton.PositionOffset_Y = 460 + slotButtons.Length * 70 + clothButtons.Length * 40 + 40;
            spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 460 + slotButtons.Length * 70 + clothButtons.Length * 40 + 70);
            return;
        }
        selectedBox.Text = "";
        tableNameField.Text = "";
        tableColorPicker.state = Color.white;
        megaToggle.Value = false;
        healthField.Value = 0;
        damageField.Value = 0;
        lootIndexField.Value = 0;
        lootIDField.Value = 0;
        xpField.Value = 0u;
        regenField.Value = 0f;
        difficultyGUIDField.Text = string.Empty;
        if (slotButtons != null)
        {
            for (int m = 0; m < slotButtons.Length; m++)
            {
                spawnsScrollBox.RemoveChild(slotButtons[m]);
            }
        }
        slotButtons = null;
        if (clothButtons != null)
        {
            for (int n = 0; n < clothButtons.Length; n++)
            {
                spawnsScrollBox.RemoveChild(clothButtons[n]);
            }
        }
        clothButtons = null;
        itemIDField.PositionOffset_Y = 460f;
        addItemButton.PositionOffset_Y = 500f;
        removeItemButton.PositionOffset_Y = 500f;
        spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 530f);
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie != (byte)(button.PositionOffset_Y / 40f))
        {
            EditorSpawns.selectedZombie = (byte)(button.PositionOffset_Y / 40f);
            EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = LevelZombies.tables[EditorSpawns.selectedZombie].color;
        }
        else
        {
            EditorSpawns.selectedZombie = byte.MaxValue;
            EditorSpawns.zombieSpawn.GetComponent<Renderer>().material.color = Color.white;
        }
        updateSelection();
    }

    private static void onZombieColorPicked(SleekColorPicker picker, Color color)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].color = color;
        }
    }

    private static void onToggledMegaToggle(ISleekToggle toggle, bool state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].isMega = state;
        }
    }

    private static void onHealthFieldTyped(ISleekUInt16Field field, ushort state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].health = state;
        }
    }

    private static void onDamageFieldTyped(ISleekUInt8Field field, byte state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].damage = state;
        }
    }

    private static void onLootIndexFieldTyped(ISleekUInt8Field field, byte state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count && state < LevelItems.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].lootIndex = state;
        }
    }

    private static void onLootIDFieldTyped(ISleekUInt16Field field, ushort state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].lootID = state;
        }
    }

    private static void onXPFieldTyped(ISleekUInt32Field field, uint state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].xp = state;
        }
    }

    private static void onRegenFieldTyped(ISleekFloat32Field field, float state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].regen = state;
        }
    }

    private static void onDifficultyGUIDFieldTyped(ISleekField field, string state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].difficultyGUID = state;
        }
    }

    private static void onClickedSlotButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            selectedSlot = (byte)((button.PositionOffset_Y - 460f) / 70f);
            updateSelection();
        }
    }

    private static void onClickItemButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            selectItem = (byte)((button.PositionOffset_Y - 460f - (float)(slotButtons.Length * 70)) / 40f);
            updateSelection();
        }
    }

    private static void onDraggedChanceSlider(ISleekSlider slider, float state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            int num = Mathf.FloorToInt((slider.Parent.PositionOffset_Y - 460f) / 70f);
            LevelZombies.tables[EditorSpawns.selectedZombie].slots[num].chance = state;
            ((ISleekSlider)slotButtons[num].GetChildAtIndex(0)).UpdateLabel(Mathf.RoundToInt(state * 100f) + "%");
        }
    }

    private static void onTypedNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            selectedBox.Text = state;
            LevelZombies.tables[EditorSpawns.selectedZombie].name = state;
            tableButtons[EditorSpawns.selectedZombie].Text = EditorSpawns.selectedZombie + " " + state;
        }
    }

    private static void onClickedAddTableButton(ISleekElement button)
    {
        if (tableNameField.Text != "")
        {
            LevelZombies.addTable(tableNameField.Text);
            tableNameField.Text = "";
            updateTables();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onClickedRemoveTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            LevelZombies.removeTable();
            updateTables();
            updateSelection();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onClickedAddItemButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie >= LevelZombies.tables.Count)
        {
            return;
        }
        if (Assets.find(EAssetType.ITEM, itemIDField.Value) is ItemAsset itemAsset)
        {
            if ((selectedSlot == 0 && itemAsset.type != EItemType.SHIRT) || (selectedSlot == 1 && itemAsset.type != EItemType.PANTS) || ((selectedSlot == 2 || selectedSlot == 3) && itemAsset.type != 0 && itemAsset.type != EItemType.BACKPACK && itemAsset.type != EItemType.VEST && itemAsset.type != EItemType.MASK && itemAsset.type != EItemType.GLASSES))
            {
                return;
            }
            LevelZombies.tables[EditorSpawns.selectedZombie].addCloth(selectedSlot, itemIDField.Value);
            updateSelection();
            spawnsScrollBox.ScrollToBottom();
        }
        itemIDField.Value = 0;
    }

    private static void onClickedRemoveItemButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count && selectItem < LevelZombies.tables[EditorSpawns.selectedZombie].slots[selectedSlot].table.Count)
        {
            LevelZombies.tables[EditorSpawns.selectedZombie].removeCloth(selectedSlot, selectItem);
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
        EditorSpawns.spawnMode = ESpawnMode.ADD_ZOMBIE;
    }

    private static void onClickedRemoveButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.REMOVE_ZOMBIE;
    }

    public EditorSpawnsZombiesUI()
    {
        localization = Localization.read("/Editor/EditorSpawnsZombies.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorSpawnsZombies/EditorSpawnsZombies.unity3d");
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
        container.AddChild(tableScrollBox);
        tableNameField = Glazier.Get().CreateStringField();
        tableNameField.PositionOffset_X = -230f;
        tableNameField.PositionOffset_Y = 330f;
        tableNameField.PositionScale_X = 1f;
        tableNameField.SizeOffset_X = 230f;
        tableNameField.SizeOffset_Y = 30f;
        tableNameField.MaxLength = 64;
        tableNameField.AddLabel(localization.format("TableNameFieldLabelText"), ESleekSide.LEFT);
        tableNameField.OnTextChanged += onTypedNameField;
        container.AddChild(tableNameField);
        addTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addTableButton.PositionOffset_X = -230f;
        addTableButton.PositionOffset_Y = 370f;
        addTableButton.PositionScale_X = 1f;
        addTableButton.SizeOffset_X = 110f;
        addTableButton.SizeOffset_Y = 30f;
        addTableButton.text = localization.format("AddTableButtonText");
        addTableButton.tooltip = localization.format("AddTableButtonTooltip");
        addTableButton.onClickedButton += onClickedAddTableButton;
        container.AddChild(addTableButton);
        removeTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeTableButton.PositionOffset_X = -110f;
        removeTableButton.PositionOffset_Y = 370f;
        removeTableButton.PositionScale_X = 1f;
        removeTableButton.SizeOffset_X = 110f;
        removeTableButton.SizeOffset_Y = 30f;
        removeTableButton.text = localization.format("RemoveTableButtonText");
        removeTableButton.tooltip = localization.format("RemoveTableButtonTooltip");
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
        tableColorPicker.onColorPicked = onZombieColorPicked;
        spawnsScrollBox.AddChild(tableColorPicker);
        megaToggle = Glazier.Get().CreateToggle();
        megaToggle.PositionOffset_X = 240f;
        megaToggle.PositionOffset_Y = 130f;
        megaToggle.SizeOffset_X = 40f;
        megaToggle.SizeOffset_Y = 40f;
        megaToggle.OnValueChanged += onToggledMegaToggle;
        megaToggle.AddLabel(localization.format("MegaToggleLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(megaToggle);
        healthField = Glazier.Get().CreateUInt16Field();
        healthField.PositionOffset_X = 240f;
        healthField.PositionOffset_Y = 180f;
        healthField.SizeOffset_X = 200f;
        healthField.SizeOffset_Y = 30f;
        healthField.OnValueChanged += onHealthFieldTyped;
        healthField.AddLabel(localization.format("HealthFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(healthField);
        damageField = Glazier.Get().CreateUInt8Field();
        damageField.PositionOffset_X = 240f;
        damageField.PositionOffset_Y = 220f;
        damageField.SizeOffset_X = 200f;
        damageField.SizeOffset_Y = 30f;
        damageField.OnValueChanged += onDamageFieldTyped;
        damageField.AddLabel(localization.format("DamageFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(damageField);
        lootIndexField = Glazier.Get().CreateUInt8Field();
        lootIndexField.PositionOffset_X = 240f;
        lootIndexField.PositionOffset_Y = 260f;
        lootIndexField.SizeOffset_X = 200f;
        lootIndexField.SizeOffset_Y = 30f;
        lootIndexField.OnValueChanged += onLootIndexFieldTyped;
        lootIndexField.AddLabel(localization.format("LootIndexFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(lootIndexField);
        lootIDField = Glazier.Get().CreateUInt16Field();
        lootIDField.PositionOffset_X = 240f;
        lootIDField.PositionOffset_Y = 300f;
        lootIDField.SizeOffset_X = 200f;
        lootIDField.SizeOffset_Y = 30f;
        lootIDField.OnValueChanged += onLootIDFieldTyped;
        lootIDField.AddLabel(localization.format("LootIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(lootIDField);
        xpField = Glazier.Get().CreateUInt32Field();
        xpField.PositionOffset_X = 240f;
        xpField.PositionOffset_Y = 340f;
        xpField.SizeOffset_X = 200f;
        xpField.SizeOffset_Y = 30f;
        xpField.OnValueChanged += onXPFieldTyped;
        xpField.AddLabel(localization.format("XPFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(xpField);
        regenField = Glazier.Get().CreateFloat32Field();
        regenField.PositionOffset_X = 240f;
        regenField.PositionOffset_Y = 380f;
        regenField.SizeOffset_X = 200f;
        regenField.SizeOffset_Y = 30f;
        regenField.OnValueChanged += onRegenFieldTyped;
        regenField.AddLabel(localization.format("RegenFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(regenField);
        difficultyGUIDField = Glazier.Get().CreateStringField();
        difficultyGUIDField.PositionOffset_X = 240f;
        difficultyGUIDField.PositionOffset_Y = 420f;
        difficultyGUIDField.SizeOffset_X = 200f;
        difficultyGUIDField.SizeOffset_Y = 30f;
        difficultyGUIDField.MaxLength = 32;
        difficultyGUIDField.OnTextChanged += onDifficultyGUIDFieldTyped;
        difficultyGUIDField.AddLabel(localization.format("DifficultyGUIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(difficultyGUIDField);
        itemIDField = Glazier.Get().CreateUInt16Field();
        itemIDField.PositionOffset_X = 240f;
        itemIDField.SizeOffset_X = 200f;
        itemIDField.SizeOffset_Y = 30f;
        itemIDField.AddLabel(localization.format("ItemIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(itemIDField);
        addItemButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addItemButton.PositionOffset_X = 240f;
        addItemButton.SizeOffset_X = 95f;
        addItemButton.SizeOffset_Y = 30f;
        addItemButton.text = localization.format("AddItemButtonText");
        addItemButton.tooltip = localization.format("AddItemButtonTooltip");
        addItemButton.onClickedButton += onClickedAddItemButton;
        spawnsScrollBox.AddChild(addItemButton);
        removeItemButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeItemButton.PositionOffset_X = 345f;
        removeItemButton.SizeOffset_X = 95f;
        removeItemButton.SizeOffset_Y = 30f;
        removeItemButton.text = localization.format("RemoveItemButtonText");
        removeItemButton.tooltip = localization.format("RemoveItemButtonTooltip");
        removeItemButton.onClickedButton += onClickedRemoveItemButton;
        spawnsScrollBox.AddChild(removeItemButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = -230f;
        selectedBox.PositionOffset_Y = 80f;
        selectedBox.PositionScale_X = 1f;
        selectedBox.SizeOffset_X = 230f;
        selectedBox.SizeOffset_Y = 30f;
        selectedBox.AddLabel(localization.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        slotButtons = null;
        clothButtons = null;
        updateSelection();
        radiusSlider = Glazier.Get().CreateSlider();
        radiusSlider.PositionOffset_Y = -100f;
        radiusSlider.PositionScale_Y = 1f;
        radiusSlider.SizeOffset_X = 200f;
        radiusSlider.SizeOffset_Y = 20f;
        radiusSlider.Value = (float)(EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float)(int)EditorSpawns.MAX_REMOVE_SIZE;
        radiusSlider.Orientation = ESleekOrientation.HORIZONTAL;
        radiusSlider.AddLabel(localization.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
        radiusSlider.OnValueChanged += onDraggedRadiusSlider;
        container.AddChild(radiusSlider);
        addButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addButton.PositionOffset_Y = -70f;
        addButton.PositionScale_Y = 1f;
        addButton.SizeOffset_X = 200f;
        addButton.SizeOffset_Y = 30f;
        addButton.text = localization.format("AddButtonText", ControlsSettings.tool_0);
        addButton.tooltip = localization.format("AddButtonTooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeButton.PositionOffset_Y = -30f;
        removeButton.PositionScale_Y = 1f;
        removeButton.SizeOffset_X = 200f;
        removeButton.SizeOffset_Y = 30f;
        removeButton.text = localization.format("RemoveButtonText", ControlsSettings.tool_1);
        removeButton.tooltip = localization.format("RemoveButtonTooltip");
        removeButton.onClickedButton += onClickedRemoveButton;
        container.AddChild(removeButton);
        bundle.unload();
    }
}
