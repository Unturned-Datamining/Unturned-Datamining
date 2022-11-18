using UnityEngine;

namespace SDG.Unturned;

public class EditorSpawnsVehiclesUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekScrollView tableScrollBox;

    private static ISleekScrollView spawnsScrollBox;

    private static ISleekButton[] tableButtons;

    private static ISleekButton[] tierButtons;

    private static ISleekButton[] vehicleButtons;

    private static SleekColorPicker tableColorPicker;

    private static ISleekUInt16Field tableIDField;

    private static ISleekField tierNameField;

    private static SleekButtonIcon addTierButton;

    private static SleekButtonIcon removeTierButton;

    private static ISleekUInt16Field vehicleIDField;

    private static SleekButtonIcon addVehicleButton;

    private static SleekButtonIcon removeVehicleButton;

    private static ISleekBox selectedBox;

    private static ISleekField tableNameField;

    private static SleekButtonIcon addTableButton;

    private static SleekButtonIcon removeTableButton;

    private static ISleekSlider radiusSlider;

    private static ISleekSlider rotationSlider;

    private static SleekButtonIcon addButton;

    private static SleekButtonIcon removeButton;

    private static byte selectedTier;

    private static byte selectVehicle;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorSpawns.isSpawning = true;
            EditorSpawns.spawnMode = ESpawnMode.ADD_VEHICLE;
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
        tableButtons = new ISleekButton[LevelVehicles.tables.Count];
        tableScrollBox.contentSizeOffset = new Vector2(0f, tableButtons.Length * 40 - 10);
        for (int j = 0; j < tableButtons.Length; j++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = 240;
            sleekButton.positionOffset_Y = j * 40;
            sleekButton.sizeOffset_X = 200;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.text = j + " " + LevelVehicles.tables[j].name;
            sleekButton.onClickedButton += onClickedTableButton;
            tableScrollBox.AddChild(sleekButton);
            tableButtons[j] = sleekButton;
        }
    }

    public static void updateSelection()
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            VehicleTable vehicleTable = LevelVehicles.tables[EditorSpawns.selectedVehicle];
            selectedBox.text = vehicleTable.name;
            tableNameField.text = vehicleTable.name;
            tableIDField.state = vehicleTable.tableID;
            tableColorPicker.state = vehicleTable.color;
            if (tierButtons != null)
            {
                for (int i = 0; i < tierButtons.Length; i++)
                {
                    spawnsScrollBox.RemoveChild(tierButtons[i]);
                }
            }
            tierButtons = new ISleekButton[vehicleTable.tiers.Count];
            for (int j = 0; j < tierButtons.Length; j++)
            {
                VehicleTier vehicleTier = vehicleTable.tiers[j];
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.positionOffset_X = 240;
                sleekButton.positionOffset_Y = 170 + j * 70;
                sleekButton.sizeOffset_X = 200;
                sleekButton.sizeOffset_Y = 30;
                sleekButton.text = vehicleTier.name;
                sleekButton.onClickedButton += onClickedTierButton;
                spawnsScrollBox.AddChild(sleekButton);
                ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
                sleekSlider.positionOffset_Y = 40;
                sleekSlider.sizeOffset_X = 200;
                sleekSlider.sizeOffset_Y = 20;
                sleekSlider.orientation = ESleekOrientation.HORIZONTAL;
                sleekSlider.state = vehicleTier.chance;
                sleekSlider.addLabel(Mathf.RoundToInt(vehicleTier.chance * 100f) + "%", ESleekSide.LEFT);
                sleekSlider.onDragged += onDraggedChanceSlider;
                sleekButton.AddChild(sleekSlider);
                tierButtons[j] = sleekButton;
            }
            tierNameField.positionOffset_Y = 170 + tierButtons.Length * 70;
            addTierButton.positionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            removeTierButton.positionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            if (vehicleButtons != null)
            {
                for (int k = 0; k < vehicleButtons.Length; k++)
                {
                    spawnsScrollBox.RemoveChild(vehicleButtons[k]);
                }
            }
            if (selectedTier < vehicleTable.tiers.Count)
            {
                tierNameField.text = vehicleTable.tiers[selectedTier].name;
                vehicleButtons = new ISleekButton[vehicleTable.tiers[selectedTier].table.Count];
                for (int l = 0; l < vehicleButtons.Length; l++)
                {
                    ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                    sleekButton2.positionOffset_X = 240;
                    sleekButton2.positionOffset_Y = 170 + tierButtons.Length * 70 + 80 + l * 40;
                    sleekButton2.sizeOffset_X = 200;
                    sleekButton2.sizeOffset_Y = 30;
                    VehicleAsset vehicleAsset = Assets.find(EAssetType.VEHICLE, vehicleTable.tiers[selectedTier].table[l].vehicle) as VehicleAsset;
                    string text = "?";
                    if (vehicleAsset != null)
                    {
                        text = ((!string.IsNullOrEmpty(vehicleAsset.vehicleName)) ? vehicleAsset.vehicleName : vehicleAsset.name);
                    }
                    sleekButton2.text = vehicleTable.tiers[selectedTier].table[l].vehicle + " " + text;
                    sleekButton2.onClickedButton += onClickVehicleButton;
                    spawnsScrollBox.AddChild(sleekButton2);
                    vehicleButtons[l] = sleekButton2;
                }
            }
            else
            {
                tierNameField.text = "";
                vehicleButtons = new ISleekButton[0];
            }
            vehicleIDField.positionOffset_Y = 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40;
            addVehicleButton.positionOffset_Y = 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40 + 40;
            removeVehicleButton.positionOffset_Y = 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40 + 40;
            spawnsScrollBox.contentSizeOffset = new Vector2(0f, 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40 + 70);
            return;
        }
        selectedBox.text = "";
        tableNameField.text = "";
        tableIDField.state = 0;
        tableColorPicker.state = Color.white;
        if (tierButtons != null)
        {
            for (int m = 0; m < tierButtons.Length; m++)
            {
                spawnsScrollBox.RemoveChild(tierButtons[m]);
            }
        }
        tierButtons = null;
        tierNameField.text = "";
        tierNameField.positionOffset_Y = 170;
        addTierButton.positionOffset_Y = 210;
        removeTierButton.positionOffset_Y = 210;
        if (vehicleButtons != null)
        {
            for (int n = 0; n < vehicleButtons.Length; n++)
            {
                spawnsScrollBox.RemoveChild(vehicleButtons[n]);
            }
        }
        vehicleButtons = null;
        vehicleIDField.positionOffset_Y = 250;
        addVehicleButton.positionOffset_Y = 290;
        removeVehicleButton.positionOffset_Y = 290;
        spawnsScrollBox.contentSizeOffset = new Vector2(0f, 320f);
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle != (byte)(button.positionOffset_Y / 40))
        {
            EditorSpawns.selectedVehicle = (byte)(button.positionOffset_Y / 40);
            EditorSpawns.vehicleSpawn.GetComponent<Renderer>().material.color = LevelVehicles.tables[EditorSpawns.selectedVehicle].color;
            EditorSpawns.vehicleSpawn.Find("Arrow").GetComponent<Renderer>().material.color = LevelVehicles.tables[EditorSpawns.selectedVehicle].color;
        }
        else
        {
            EditorSpawns.selectedVehicle = byte.MaxValue;
            EditorSpawns.vehicleSpawn.GetComponent<Renderer>().material.color = Color.white;
            EditorSpawns.vehicleSpawn.Find("Arrow").GetComponent<Renderer>().material.color = Color.white;
        }
        updateSelection();
    }

    private static void onVehicleColorPicked(SleekColorPicker picker, Color color)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            LevelVehicles.tables[EditorSpawns.selectedVehicle].color = color;
        }
    }

    private static void onTableIDFieldTyped(ISleekUInt16Field field, ushort state)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            LevelVehicles.tables[EditorSpawns.selectedVehicle].tableID = state;
        }
    }

    private static void onClickedTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            if (selectedTier != (byte)((button.positionOffset_Y - 170) / 70))
            {
                selectedTier = (byte)((button.positionOffset_Y - 170) / 70);
            }
            else
            {
                selectedTier = byte.MaxValue;
            }
            updateSelection();
        }
    }

    private static void onClickVehicleButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            selectVehicle = (byte)((button.positionOffset_Y - 170 - tierButtons.Length * 70 - 80) / 40);
            updateSelection();
        }
    }

    private static void onDraggedChanceSlider(ISleekSlider slider, float state)
    {
        if (EditorSpawns.selectedVehicle >= LevelVehicles.tables.Count)
        {
            return;
        }
        int num = (slider.parent.positionOffset_Y - 170) / 70;
        LevelVehicles.tables[EditorSpawns.selectedVehicle].updateChance(num, state);
        for (int i = 0; i < LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers.Count; i++)
        {
            VehicleTier vehicleTier = LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers[i];
            ISleekSlider sleekSlider = (ISleekSlider)tierButtons[i].GetChildAtIndex(0);
            if (i != num)
            {
                sleekSlider.state = vehicleTier.chance;
            }
            sleekSlider.updateLabel(Mathf.RoundToInt(vehicleTier.chance * 100f) + "%");
        }
    }

    private static void onTypedNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            selectedBox.text = state;
            LevelVehicles.tables[EditorSpawns.selectedVehicle].name = state;
            tableButtons[EditorSpawns.selectedVehicle].text = EditorSpawns.selectedVehicle + " " + state;
        }
    }

    private static void onClickedAddTableButton(ISleekElement button)
    {
        if (tableNameField.text != "")
        {
            LevelVehicles.addTable(tableNameField.text);
            tableNameField.text = "";
            updateTables();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onClickedRemoveTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            LevelVehicles.removeTable();
            updateTables();
            updateSelection();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onTypedTierNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count && selectedTier < LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers.Count)
        {
            LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers[selectedTier].name = state;
            tierButtons[selectedTier].text = state;
        }
    }

    private static void onClickedAddTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count && tierNameField.text != "")
        {
            LevelVehicles.tables[EditorSpawns.selectedVehicle].addTier(tierNameField.text);
            tierNameField.text = "";
            updateSelection();
        }
    }

    private static void onClickedRemoveTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count && selectedTier < LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers.Count)
        {
            LevelVehicles.tables[EditorSpawns.selectedVehicle].removeTier(selectedTier);
            updateSelection();
        }
    }

    private static void onClickedAddVehicleButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count && selectedTier < LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers.Count)
        {
            if (Assets.find(EAssetType.VEHICLE, vehicleIDField.state) is VehicleAsset)
            {
                LevelVehicles.tables[EditorSpawns.selectedVehicle].addVehicle(selectedTier, vehicleIDField.state);
                updateSelection();
                spawnsScrollBox.ScrollToBottom();
            }
            vehicleIDField.state = 0;
        }
    }

    private static void onClickedRemoveVehicleButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count && selectedTier < LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers.Count && selectVehicle < LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers[selectedTier].table.Count)
        {
            LevelVehicles.tables[EditorSpawns.selectedVehicle].removeVehicle(selectedTier, selectVehicle);
            updateSelection();
            spawnsScrollBox.ScrollToBottom();
        }
    }

    private static void onDraggedRadiusSlider(ISleekSlider slider, float state)
    {
        EditorSpawns.radius = (byte)((float)(int)EditorSpawns.MIN_REMOVE_SIZE + state * (float)(int)EditorSpawns.MAX_REMOVE_SIZE);
    }

    private static void onDraggedRotationSlider(ISleekSlider slider, float state)
    {
        EditorSpawns.rotation = state * 360f;
    }

    private static void onClickedAddButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.ADD_VEHICLE;
    }

    private static void onClickedRemoveButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.REMOVE_VEHICLE;
    }

    public EditorSpawnsVehiclesUI()
    {
        Local local = Localization.read("/Editor/EditorSpawnsVehicles.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorSpawnsVehicles/EditorSpawnsVehicles.unity3d");
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
        tableScrollBox.scaleContentToWidth = true;
        container.AddChild(tableScrollBox);
        tableNameField = Glazier.Get().CreateStringField();
        tableNameField.positionOffset_X = -230;
        tableNameField.positionOffset_Y = 330;
        tableNameField.positionScale_X = 1f;
        tableNameField.sizeOffset_X = 230;
        tableNameField.sizeOffset_Y = 30;
        tableNameField.maxLength = 64;
        tableNameField.addLabel(local.format("TableNameFieldLabelText"), ESleekSide.LEFT);
        tableNameField.onTyped += onTypedNameField;
        container.AddChild(tableNameField);
        addTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addTableButton.positionOffset_X = -230;
        addTableButton.positionOffset_Y = 370;
        addTableButton.positionScale_X = 1f;
        addTableButton.sizeOffset_X = 110;
        addTableButton.sizeOffset_Y = 30;
        addTableButton.text = local.format("AddTableButtonText");
        addTableButton.tooltip = local.format("AddTableButtonTooltip");
        addTableButton.onClickedButton += onClickedAddTableButton;
        container.AddChild(addTableButton);
        removeTableButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeTableButton.positionOffset_X = -110;
        removeTableButton.positionOffset_Y = 370;
        removeTableButton.positionScale_X = 1f;
        removeTableButton.sizeOffset_X = 110;
        removeTableButton.sizeOffset_Y = 30;
        removeTableButton.text = local.format("RemoveTableButtonText");
        removeTableButton.tooltip = local.format("RemoveTableButtonTooltip");
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
        tableColorPicker.onColorPicked = onVehicleColorPicked;
        spawnsScrollBox.AddChild(tableColorPicker);
        tableIDField = Glazier.Get().CreateUInt16Field();
        tableIDField.positionOffset_X = 240;
        tableIDField.positionOffset_Y = 130;
        tableIDField.sizeOffset_X = 200;
        tableIDField.sizeOffset_Y = 30;
        tableIDField.onTypedUInt16 += onTableIDFieldTyped;
        tableIDField.addLabel(local.format("TableIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(tableIDField);
        tierNameField = Glazier.Get().CreateStringField();
        tierNameField.positionOffset_X = 240;
        tierNameField.sizeOffset_X = 200;
        tierNameField.sizeOffset_Y = 30;
        tierNameField.maxLength = 64;
        tierNameField.addLabel(local.format("TierNameFieldLabelText"), ESleekSide.LEFT);
        tierNameField.onTyped += onTypedTierNameField;
        spawnsScrollBox.AddChild(tierNameField);
        addTierButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addTierButton.positionOffset_X = 240;
        addTierButton.sizeOffset_X = 95;
        addTierButton.sizeOffset_Y = 30;
        addTierButton.text = local.format("AddTierButtonText");
        addTierButton.tooltip = local.format("AddTierButtonTooltip");
        addTierButton.onClickedButton += onClickedAddTierButton;
        spawnsScrollBox.AddChild(addTierButton);
        removeTierButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeTierButton.positionOffset_X = 345;
        removeTierButton.sizeOffset_X = 95;
        removeTierButton.sizeOffset_Y = 30;
        removeTierButton.text = local.format("RemoveTierButtonText");
        removeTierButton.tooltip = local.format("RemoveTierButtonTooltip");
        removeTierButton.onClickedButton += onClickedRemoveTierButton;
        spawnsScrollBox.AddChild(removeTierButton);
        vehicleIDField = Glazier.Get().CreateUInt16Field();
        vehicleIDField.positionOffset_X = 240;
        vehicleIDField.sizeOffset_X = 200;
        vehicleIDField.sizeOffset_Y = 30;
        vehicleIDField.addLabel(local.format("VehicleIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(vehicleIDField);
        addVehicleButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addVehicleButton.positionOffset_X = 240;
        addVehicleButton.sizeOffset_X = 95;
        addVehicleButton.sizeOffset_Y = 30;
        addVehicleButton.text = local.format("AddVehicleButtonText");
        addVehicleButton.tooltip = local.format("AddVehicleButtonTooltip");
        addVehicleButton.onClickedButton += onClickedAddVehicleButton;
        spawnsScrollBox.AddChild(addVehicleButton);
        removeVehicleButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeVehicleButton.positionOffset_X = 345;
        removeVehicleButton.sizeOffset_X = 95;
        removeVehicleButton.sizeOffset_Y = 30;
        removeVehicleButton.text = local.format("RemoveVehicleButtonText");
        removeVehicleButton.tooltip = local.format("RemoveVehicleButtonTooltip");
        removeVehicleButton.onClickedButton += onClickedRemoveVehicleButton;
        spawnsScrollBox.AddChild(removeVehicleButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.positionOffset_X = -230;
        selectedBox.positionOffset_Y = 80;
        selectedBox.positionScale_X = 1f;
        selectedBox.sizeOffset_X = 230;
        selectedBox.sizeOffset_Y = 30;
        selectedBox.addLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        tierButtons = null;
        vehicleButtons = null;
        updateSelection();
        radiusSlider = Glazier.Get().CreateSlider();
        radiusSlider.positionOffset_Y = -130;
        radiusSlider.positionScale_Y = 1f;
        radiusSlider.sizeOffset_X = 200;
        radiusSlider.sizeOffset_Y = 20;
        radiusSlider.state = (float)(EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float)(int)EditorSpawns.MAX_REMOVE_SIZE;
        radiusSlider.orientation = ESleekOrientation.HORIZONTAL;
        radiusSlider.addLabel(local.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
        radiusSlider.onDragged += onDraggedRadiusSlider;
        container.AddChild(radiusSlider);
        rotationSlider = Glazier.Get().CreateSlider();
        rotationSlider.positionOffset_Y = -100;
        rotationSlider.positionScale_Y = 1f;
        rotationSlider.sizeOffset_X = 200;
        rotationSlider.sizeOffset_Y = 20;
        rotationSlider.state = EditorSpawns.rotation / 360f;
        rotationSlider.orientation = ESleekOrientation.HORIZONTAL;
        rotationSlider.addLabel(local.format("RotationSliderLabelText"), ESleekSide.RIGHT);
        rotationSlider.onDragged += onDraggedRotationSlider;
        container.AddChild(rotationSlider);
        addButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addButton.positionOffset_Y = -70;
        addButton.positionScale_Y = 1f;
        addButton.sizeOffset_X = 200;
        addButton.sizeOffset_Y = 30;
        addButton.text = local.format("AddButtonText", ControlsSettings.tool_0);
        addButton.tooltip = local.format("AddButtonTooltip");
        addButton.onClickedButton += onClickedAddButton;
        container.AddChild(addButton);
        removeButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeButton.positionOffset_Y = -30;
        removeButton.positionScale_Y = 1f;
        removeButton.sizeOffset_X = 200;
        removeButton.sizeOffset_Y = 30;
        removeButton.text = local.format("RemoveButtonText", ControlsSettings.tool_1);
        removeButton.tooltip = local.format("RemoveButtonTooltip");
        removeButton.onClickedButton += onClickedRemoveButton;
        container.AddChild(removeButton);
        bundle.unload();
    }
}
