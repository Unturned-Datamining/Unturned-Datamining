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
        tableScrollBox.contentSizeOffset = new Vector2(0f, tableButtons.Length * 40 - 10);
        for (int j = 0; j < tableButtons.Length; j++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = 240;
            sleekButton.positionOffset_Y = j * 40;
            sleekButton.sizeOffset_X = 200;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.text = j + " " + LevelZombies.tables[j].name;
            sleekButton.onClickedButton += onClickedTableButton;
            tableScrollBox.AddChild(sleekButton);
            tableButtons[j] = sleekButton;
        }
    }

    public static void updateSelection()
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            ZombieTable zombieTable = LevelZombies.tables[EditorSpawns.selectedZombie];
            selectedBox.text = zombieTable.name;
            tableNameField.text = zombieTable.name;
            tableColorPicker.state = zombieTable.color;
            megaToggle.state = zombieTable.isMega;
            healthField.state = zombieTable.health;
            damageField.state = zombieTable.damage;
            lootIndexField.state = zombieTable.lootIndex;
            lootIDField.state = zombieTable.lootID;
            xpField.state = zombieTable.xp;
            regenField.state = zombieTable.regen;
            difficultyGUIDField.text = zombieTable.difficultyGUID;
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
                sleekButton.positionOffset_X = 240;
                sleekButton.positionOffset_Y = 460 + j * 70;
                sleekButton.sizeOffset_X = 200;
                sleekButton.sizeOffset_Y = 30;
                sleekButton.text = localization.format("Slot_" + j);
                sleekButton.onClickedButton += onClickedSlotButton;
                spawnsScrollBox.AddChild(sleekButton);
                ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
                sleekSlider.positionOffset_Y = 40;
                sleekSlider.sizeOffset_X = 200;
                sleekSlider.sizeOffset_Y = 20;
                sleekSlider.orientation = ESleekOrientation.HORIZONTAL;
                sleekSlider.state = zombieSlot.chance;
                sleekSlider.addLabel(Mathf.RoundToInt(zombieSlot.chance * 100f) + "%", ESleekSide.LEFT);
                sleekSlider.onDragged += onDraggedChanceSlider;
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
                    sleekButton2.positionOffset_X = 240;
                    sleekButton2.positionOffset_Y = 460 + slotButtons.Length * 70 + l * 40;
                    sleekButton2.sizeOffset_X = 200;
                    sleekButton2.sizeOffset_Y = 30;
                    ItemAsset itemAsset = Assets.find(EAssetType.ITEM, zombieTable.slots[selectedSlot].table[l].item) as ItemAsset;
                    string text = "?";
                    if (itemAsset != null)
                    {
                        text = ((!string.IsNullOrEmpty(itemAsset.itemName)) ? itemAsset.itemName : itemAsset.name);
                    }
                    sleekButton2.text = zombieTable.slots[selectedSlot].table[l].item + " " + text;
                    sleekButton2.onClickedButton += onClickItemButton;
                    spawnsScrollBox.AddChild(sleekButton2);
                    clothButtons[l] = sleekButton2;
                }
            }
            else
            {
                clothButtons = new ISleekButton[0];
            }
            itemIDField.positionOffset_Y = 460 + slotButtons.Length * 70 + clothButtons.Length * 40;
            addItemButton.positionOffset_Y = 460 + slotButtons.Length * 70 + clothButtons.Length * 40 + 40;
            removeItemButton.positionOffset_Y = 460 + slotButtons.Length * 70 + clothButtons.Length * 40 + 40;
            spawnsScrollBox.contentSizeOffset = new Vector2(0f, 460 + slotButtons.Length * 70 + clothButtons.Length * 40 + 70);
            return;
        }
        selectedBox.text = "";
        tableNameField.text = "";
        tableColorPicker.state = Color.white;
        megaToggle.state = false;
        healthField.state = 0;
        damageField.state = 0;
        lootIndexField.state = 0;
        lootIDField.state = 0;
        xpField.state = 0u;
        regenField.state = 0f;
        difficultyGUIDField.text = string.Empty;
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
        itemIDField.positionOffset_Y = 460;
        addItemButton.positionOffset_Y = 500;
        removeItemButton.positionOffset_Y = 500;
        spawnsScrollBox.contentSizeOffset = new Vector2(0f, 530f);
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie != (byte)(button.positionOffset_Y / 40))
        {
            EditorSpawns.selectedZombie = (byte)(button.positionOffset_Y / 40);
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
            selectedSlot = (byte)((button.positionOffset_Y - 460) / 70);
            updateSelection();
        }
    }

    private static void onClickItemButton(ISleekElement button)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            selectItem = (byte)((button.positionOffset_Y - 460 - slotButtons.Length * 70) / 40);
            updateSelection();
        }
    }

    private static void onDraggedChanceSlider(ISleekSlider slider, float state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            int num = (slider.parent.positionOffset_Y - 460) / 70;
            LevelZombies.tables[EditorSpawns.selectedZombie].slots[num].chance = state;
            ((ISleekSlider)slotButtons[num].GetChildAtIndex(0)).updateLabel(Mathf.RoundToInt(state * 100f) + "%");
        }
    }

    private static void onTypedNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedZombie < LevelZombies.tables.Count)
        {
            selectedBox.text = state;
            LevelZombies.tables[EditorSpawns.selectedZombie].name = state;
            tableButtons[EditorSpawns.selectedZombie].text = EditorSpawns.selectedZombie + " " + state;
        }
    }

    private static void onClickedAddTableButton(ISleekElement button)
    {
        if (tableNameField.text != "")
        {
            LevelZombies.addTable(tableNameField.text);
            tableNameField.text = "";
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
        if (Assets.find(EAssetType.ITEM, itemIDField.state) is ItemAsset itemAsset)
        {
            if ((selectedSlot == 0 && itemAsset.type != EItemType.SHIRT) || (selectedSlot == 1 && itemAsset.type != EItemType.PANTS) || ((selectedSlot == 2 || selectedSlot == 3) && itemAsset.type != 0 && itemAsset.type != EItemType.BACKPACK && itemAsset.type != EItemType.VEST && itemAsset.type != EItemType.MASK && itemAsset.type != EItemType.GLASSES))
            {
                return;
            }
            LevelZombies.tables[EditorSpawns.selectedZombie].addCloth(selectedSlot, itemIDField.state);
            updateSelection();
            spawnsScrollBox.ScrollToBottom();
        }
        itemIDField.state = 0;
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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        tableScrollBox = Glazier.Get().CreateScrollView();
        tableScrollBox.positionOffset_X = -470;
        tableScrollBox.positionOffset_Y = 120;
        tableScrollBox.positionScale_X = 1f;
        tableScrollBox.sizeOffset_X = 470;
        tableScrollBox.sizeOffset_Y = 200;
        container.AddChild(tableScrollBox);
        tableNameField = Glazier.Get().CreateStringField();
        tableNameField.positionOffset_X = -230;
        tableNameField.positionOffset_Y = 330;
        tableNameField.positionScale_X = 1f;
        tableNameField.sizeOffset_X = 230;
        tableNameField.sizeOffset_Y = 30;
        tableNameField.maxLength = 64;
        tableNameField.addLabel(localization.format("TableNameFieldLabelText"), ESleekSide.LEFT);
        tableNameField.onTyped += onTypedNameField;
        container.AddChild(tableNameField);
        addTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addTableButton.positionOffset_X = -230;
        addTableButton.positionOffset_Y = 370;
        addTableButton.positionScale_X = 1f;
        addTableButton.sizeOffset_X = 110;
        addTableButton.sizeOffset_Y = 30;
        addTableButton.text = localization.format("AddTableButtonText");
        addTableButton.tooltip = localization.format("AddTableButtonTooltip");
        addTableButton.onClickedButton += onClickedAddTableButton;
        container.AddChild(addTableButton);
        removeTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeTableButton.positionOffset_X = -110;
        removeTableButton.positionOffset_Y = 370;
        removeTableButton.positionScale_X = 1f;
        removeTableButton.sizeOffset_X = 110;
        removeTableButton.sizeOffset_Y = 30;
        removeTableButton.text = localization.format("RemoveTableButtonText");
        removeTableButton.tooltip = localization.format("RemoveTableButtonTooltip");
        removeTableButton.onClickedButton += onClickedRemoveTableButton;
        container.AddChild(removeTableButton);
        tableButtons = null;
        updateTables();
        spawnsScrollBox = Glazier.Get().CreateScrollView();
        spawnsScrollBox.positionOffset_X = -470;
        spawnsScrollBox.positionOffset_Y = 410;
        spawnsScrollBox.positionScale_X = 1f;
        spawnsScrollBox.sizeOffset_X = 470;
        spawnsScrollBox.sizeOffset_Y = -410;
        spawnsScrollBox.sizeScale_Y = 1f;
        spawnsScrollBox.scaleContentToWidth = true;
        spawnsScrollBox.contentSizeOffset = new Vector2(0f, 1000f);
        container.AddChild(spawnsScrollBox);
        tableColorPicker = new SleekColorPicker();
        tableColorPicker.positionOffset_X = 200;
        tableColorPicker.onColorPicked = onZombieColorPicked;
        spawnsScrollBox.AddChild(tableColorPicker);
        megaToggle = Glazier.Get().CreateToggle();
        megaToggle.positionOffset_X = 240;
        megaToggle.positionOffset_Y = 130;
        megaToggle.sizeOffset_X = 40;
        megaToggle.sizeOffset_Y = 40;
        megaToggle.onToggled += onToggledMegaToggle;
        megaToggle.addLabel(localization.format("MegaToggleLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(megaToggle);
        healthField = Glazier.Get().CreateUInt16Field();
        healthField.positionOffset_X = 240;
        healthField.positionOffset_Y = 180;
        healthField.sizeOffset_X = 200;
        healthField.sizeOffset_Y = 30;
        healthField.onTypedUInt16 += onHealthFieldTyped;
        healthField.addLabel(localization.format("HealthFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(healthField);
        damageField = Glazier.Get().CreateUInt8Field();
        damageField.positionOffset_X = 240;
        damageField.positionOffset_Y = 220;
        damageField.sizeOffset_X = 200;
        damageField.sizeOffset_Y = 30;
        damageField.onTypedByte += onDamageFieldTyped;
        damageField.addLabel(localization.format("DamageFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(damageField);
        lootIndexField = Glazier.Get().CreateUInt8Field();
        lootIndexField.positionOffset_X = 240;
        lootIndexField.positionOffset_Y = 260;
        lootIndexField.sizeOffset_X = 200;
        lootIndexField.sizeOffset_Y = 30;
        lootIndexField.onTypedByte += onLootIndexFieldTyped;
        lootIndexField.addLabel(localization.format("LootIndexFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(lootIndexField);
        lootIDField = Glazier.Get().CreateUInt16Field();
        lootIDField.positionOffset_X = 240;
        lootIDField.positionOffset_Y = 300;
        lootIDField.sizeOffset_X = 200;
        lootIDField.sizeOffset_Y = 30;
        lootIDField.onTypedUInt16 += onLootIDFieldTyped;
        lootIDField.addLabel(localization.format("LootIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(lootIDField);
        xpField = Glazier.Get().CreateUInt32Field();
        xpField.positionOffset_X = 240;
        xpField.positionOffset_Y = 340;
        xpField.sizeOffset_X = 200;
        xpField.sizeOffset_Y = 30;
        xpField.onTypedUInt32 += onXPFieldTyped;
        xpField.addLabel(localization.format("XPFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(xpField);
        regenField = Glazier.Get().CreateFloat32Field();
        regenField.positionOffset_X = 240;
        regenField.positionOffset_Y = 380;
        regenField.sizeOffset_X = 200;
        regenField.sizeOffset_Y = 30;
        regenField.onTypedSingle += onRegenFieldTyped;
        regenField.addLabel(localization.format("RegenFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(regenField);
        difficultyGUIDField = Glazier.Get().CreateStringField();
        difficultyGUIDField.positionOffset_X = 240;
        difficultyGUIDField.positionOffset_Y = 420;
        difficultyGUIDField.sizeOffset_X = 200;
        difficultyGUIDField.sizeOffset_Y = 30;
        difficultyGUIDField.maxLength = 32;
        difficultyGUIDField.onTyped += onDifficultyGUIDFieldTyped;
        difficultyGUIDField.addLabel(localization.format("DifficultyGUIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(difficultyGUIDField);
        itemIDField = Glazier.Get().CreateUInt16Field();
        itemIDField.positionOffset_X = 240;
        itemIDField.sizeOffset_X = 200;
        itemIDField.sizeOffset_Y = 30;
        itemIDField.addLabel(localization.format("ItemIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(itemIDField);
        addItemButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addItemButton.positionOffset_X = 240;
        addItemButton.sizeOffset_X = 95;
        addItemButton.sizeOffset_Y = 30;
        addItemButton.text = localization.format("AddItemButtonText");
        addItemButton.tooltip = localization.format("AddItemButtonTooltip");
        addItemButton.onClickedButton += onClickedAddItemButton;
        spawnsScrollBox.AddChild(addItemButton);
        removeItemButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeItemButton.positionOffset_X = 345;
        removeItemButton.sizeOffset_X = 95;
        removeItemButton.sizeOffset_Y = 30;
        removeItemButton.text = localization.format("RemoveItemButtonText");
        removeItemButton.tooltip = localization.format("RemoveItemButtonTooltip");
        removeItemButton.onClickedButton += onClickedRemoveItemButton;
        spawnsScrollBox.AddChild(removeItemButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.positionOffset_X = -230;
        selectedBox.positionOffset_Y = 80;
        selectedBox.positionScale_X = 1f;
        selectedBox.sizeOffset_X = 230;
        selectedBox.sizeOffset_Y = 30;
        selectedBox.addLabel(localization.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        slotButtons = null;
        clothButtons = null;
        updateSelection();
        radiusSlider = Glazier.Get().CreateSlider();
        radiusSlider.positionOffset_Y = -100;
        radiusSlider.positionScale_Y = 1f;
        radiusSlider.sizeOffset_X = 200;
        radiusSlider.sizeOffset_Y = 20;
        radiusSlider.state = (float)(EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float)(int)EditorSpawns.MAX_REMOVE_SIZE;
        radiusSlider.orientation = ESleekOrientation.HORIZONTAL;
        radiusSlider.addLabel(localization.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
        radiusSlider.onDragged += onDraggedRadiusSlider;
        container.AddChild(radiusSlider);
        addButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addButton.positionOffset_Y = -70;
        addButton.positionScale_Y = 1f;
        addButton.sizeOffset_X = 200;
        addButton.sizeOffset_Y = 30;
        addButton.text = localization.format("AddButtonText", ControlsSettings.tool_0);
        addButton.tooltip = localization.format("AddButtonTooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeButton.positionOffset_Y = -30;
        removeButton.positionScale_Y = 1f;
        removeButton.sizeOffset_X = 200;
        removeButton.sizeOffset_Y = 30;
        removeButton.text = localization.format("RemoveButtonText", ControlsSettings.tool_1);
        removeButton.tooltip = localization.format("RemoveButtonTooltip");
        removeButton.onClickedButton += onClickedRemoveButton;
        container.AddChild(removeButton);
        bundle.unload();
    }
}
