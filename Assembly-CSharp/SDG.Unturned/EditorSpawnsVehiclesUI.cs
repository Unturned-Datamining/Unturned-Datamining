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
        tableScrollBox.ContentSizeOffset = new Vector2(0f, tableButtons.Length * 40 - 10);
        for (int j = 0; j < tableButtons.Length; j++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = 240f;
            sleekButton.PositionOffset_Y = j * 40;
            sleekButton.SizeOffset_X = 200f;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.Text = j + " " + LevelVehicles.tables[j].name;
            sleekButton.OnClicked += onClickedTableButton;
            tableScrollBox.AddChild(sleekButton);
            tableButtons[j] = sleekButton;
        }
    }

    public static void updateSelection()
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            VehicleTable vehicleTable = LevelVehicles.tables[EditorSpawns.selectedVehicle];
            selectedBox.Text = vehicleTable.name;
            tableNameField.Text = vehicleTable.name;
            tableIDField.Value = vehicleTable.tableID;
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
                sleekButton.PositionOffset_X = 240f;
                sleekButton.PositionOffset_Y = 170 + j * 70;
                sleekButton.SizeOffset_X = 200f;
                sleekButton.SizeOffset_Y = 30f;
                sleekButton.Text = vehicleTier.name;
                sleekButton.OnClicked += onClickedTierButton;
                spawnsScrollBox.AddChild(sleekButton);
                ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
                sleekSlider.PositionOffset_Y = 40f;
                sleekSlider.SizeOffset_X = 200f;
                sleekSlider.SizeOffset_Y = 20f;
                sleekSlider.Orientation = ESleekOrientation.HORIZONTAL;
                sleekSlider.Value = vehicleTier.chance;
                sleekSlider.AddLabel(Mathf.RoundToInt(vehicleTier.chance * 100f) + "%", ESleekSide.LEFT);
                sleekSlider.OnValueChanged += onDraggedChanceSlider;
                sleekButton.AddChild(sleekSlider);
                tierButtons[j] = sleekButton;
            }
            tierNameField.PositionOffset_Y = 170 + tierButtons.Length * 70;
            addTierButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            removeTierButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            if (vehicleButtons != null)
            {
                for (int k = 0; k < vehicleButtons.Length; k++)
                {
                    spawnsScrollBox.RemoveChild(vehicleButtons[k]);
                }
            }
            if (selectedTier < vehicleTable.tiers.Count)
            {
                tierNameField.Text = vehicleTable.tiers[selectedTier].name;
                vehicleButtons = new ISleekButton[vehicleTable.tiers[selectedTier].table.Count];
                for (int l = 0; l < vehicleButtons.Length; l++)
                {
                    ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                    sleekButton2.PositionOffset_X = 240f;
                    sleekButton2.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + l * 40;
                    sleekButton2.SizeOffset_X = 200f;
                    sleekButton2.SizeOffset_Y = 30f;
                    VehicleAsset vehicleAsset = VehicleTool.FindVehicleByLegacyIdAndHandleRedirects(vehicleTable.tiers[selectedTier].table[l].vehicle);
                    string text = "?";
                    if (vehicleAsset != null)
                    {
                        text = ((!string.IsNullOrEmpty(vehicleAsset.vehicleName)) ? vehicleAsset.vehicleName : vehicleAsset.name);
                    }
                    sleekButton2.Text = vehicleTable.tiers[selectedTier].table[l].vehicle + " " + text;
                    sleekButton2.OnClicked += onClickVehicleButton;
                    spawnsScrollBox.AddChild(sleekButton2);
                    vehicleButtons[l] = sleekButton2;
                }
            }
            else
            {
                tierNameField.Text = "";
                vehicleButtons = new ISleekButton[0];
            }
            vehicleIDField.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40;
            addVehicleButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40 + 40;
            removeVehicleButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40 + 40;
            spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 170 + tierButtons.Length * 70 + 80 + vehicleButtons.Length * 40 + 70);
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
        if (vehicleButtons != null)
        {
            for (int n = 0; n < vehicleButtons.Length; n++)
            {
                spawnsScrollBox.RemoveChild(vehicleButtons[n]);
            }
        }
        vehicleButtons = null;
        vehicleIDField.PositionOffset_Y = 250f;
        addVehicleButton.PositionOffset_Y = 290f;
        removeVehicleButton.PositionOffset_Y = 290f;
        spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 320f);
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle != (byte)(button.PositionOffset_Y / 40f))
        {
            EditorSpawns.selectedVehicle = (byte)(button.PositionOffset_Y / 40f);
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

    private static void onClickVehicleButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            selectVehicle = (byte)((button.PositionOffset_Y - 170f - (float)(tierButtons.Length * 70) - 80f) / 40f);
            updateSelection();
        }
    }

    private static void onDraggedChanceSlider(ISleekSlider slider, float state)
    {
        if (EditorSpawns.selectedVehicle >= LevelVehicles.tables.Count)
        {
            return;
        }
        int num = Mathf.FloorToInt((slider.Parent.PositionOffset_Y - 170f) / 70f);
        LevelVehicles.tables[EditorSpawns.selectedVehicle].updateChance(num, state);
        for (int i = 0; i < LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers.Count; i++)
        {
            VehicleTier vehicleTier = LevelVehicles.tables[EditorSpawns.selectedVehicle].tiers[i];
            ISleekSlider sleekSlider = (ISleekSlider)tierButtons[i].GetChildAtIndex(0);
            if (i != num)
            {
                sleekSlider.Value = vehicleTier.chance;
            }
            sleekSlider.UpdateLabel(Mathf.RoundToInt(vehicleTier.chance * 100f) + "%");
        }
    }

    private static void onTypedNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count)
        {
            selectedBox.Text = state;
            LevelVehicles.tables[EditorSpawns.selectedVehicle].name = state;
            tableButtons[EditorSpawns.selectedVehicle].Text = EditorSpawns.selectedVehicle + " " + state;
        }
    }

    private static void onClickedAddTableButton(ISleekElement button)
    {
        if (tableNameField.Text != "")
        {
            LevelVehicles.addTable(tableNameField.Text);
            tableNameField.Text = "";
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
            tierButtons[selectedTier].Text = state;
        }
    }

    private static void onClickedAddTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedVehicle < LevelVehicles.tables.Count && tierNameField.Text != "")
        {
            LevelVehicles.tables[EditorSpawns.selectedVehicle].addTier(tierNameField.Text);
            tierNameField.Text = "";
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
            if (VehicleTool.FindVehicleByLegacyIdAndHandleRedirects(vehicleIDField.Value) != null)
            {
                LevelVehicles.tables[EditorSpawns.selectedVehicle].addVehicle(selectedTier, vehicleIDField.Value);
                updateSelection();
                spawnsScrollBox.ScrollToBottom();
            }
            vehicleIDField.Value = 0;
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
        tableNameField.OnTextChanged += onTypedNameField;
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
        tableColorPicker.onColorPicked = onVehicleColorPicked;
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
        vehicleIDField = Glazier.Get().CreateUInt16Field();
        vehicleIDField.PositionOffset_X = 240f;
        vehicleIDField.SizeOffset_X = 200f;
        vehicleIDField.SizeOffset_Y = 30f;
        vehicleIDField.AddLabel(local.format("VehicleIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(vehicleIDField);
        addVehicleButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addVehicleButton.PositionOffset_X = 240f;
        addVehicleButton.SizeOffset_X = 95f;
        addVehicleButton.SizeOffset_Y = 30f;
        addVehicleButton.text = local.format("AddVehicleButtonText");
        addVehicleButton.tooltip = local.format("AddVehicleButtonTooltip");
        addVehicleButton.onClickedButton += onClickedAddVehicleButton;
        spawnsScrollBox.AddChild(addVehicleButton);
        removeVehicleButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeVehicleButton.PositionOffset_X = 345f;
        removeVehicleButton.SizeOffset_X = 95f;
        removeVehicleButton.SizeOffset_Y = 30f;
        removeVehicleButton.text = local.format("RemoveVehicleButtonText");
        removeVehicleButton.tooltip = local.format("RemoveVehicleButtonTooltip");
        removeVehicleButton.onClickedButton += onClickedRemoveVehicleButton;
        spawnsScrollBox.AddChild(removeVehicleButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = -230f;
        selectedBox.PositionOffset_Y = 80f;
        selectedBox.PositionScale_X = 1f;
        selectedBox.SizeOffset_X = 230f;
        selectedBox.SizeOffset_Y = 30f;
        selectedBox.AddLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        tierButtons = null;
        vehicleButtons = null;
        updateSelection();
        radiusSlider = Glazier.Get().CreateSlider();
        radiusSlider.PositionOffset_Y = -130f;
        radiusSlider.PositionScale_Y = 1f;
        radiusSlider.SizeOffset_X = 200f;
        radiusSlider.SizeOffset_Y = 20f;
        radiusSlider.Value = (float)(EditorSpawns.radius - EditorSpawns.MIN_REMOVE_SIZE) / (float)(int)EditorSpawns.MAX_REMOVE_SIZE;
        radiusSlider.Orientation = ESleekOrientation.HORIZONTAL;
        radiusSlider.AddLabel(local.format("RadiusSliderLabelText"), ESleekSide.RIGHT);
        radiusSlider.OnValueChanged += onDraggedRadiusSlider;
        container.AddChild(radiusSlider);
        rotationSlider = Glazier.Get().CreateSlider();
        rotationSlider.PositionOffset_Y = -100f;
        rotationSlider.PositionScale_Y = 1f;
        rotationSlider.SizeOffset_X = 200f;
        rotationSlider.SizeOffset_Y = 20f;
        rotationSlider.Value = EditorSpawns.rotation / 360f;
        rotationSlider.Orientation = ESleekOrientation.HORIZONTAL;
        rotationSlider.AddLabel(local.format("RotationSliderLabelText"), ESleekSide.RIGHT);
        rotationSlider.OnValueChanged += onDraggedRotationSlider;
        container.AddChild(rotationSlider);
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
