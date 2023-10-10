using UnityEngine;

namespace SDG.Unturned;

public class EditorSpawnsAnimalsUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekScrollView tableScrollBox;

    private static ISleekScrollView spawnsScrollBox;

    private static ISleekButton[] tableButtons;

    private static ISleekButton[] tierButtons;

    private static ISleekButton[] animalButtons;

    private static SleekColorPicker tableColorPicker;

    private static ISleekUInt16Field tableIDField;

    private static ISleekField tierNameField;

    private static SleekButtonIcon addTierButton;

    private static SleekButtonIcon removeTierButton;

    private static ISleekUInt16Field animalIDField;

    private static SleekButtonIcon addAnimalButton;

    private static SleekButtonIcon removeAnimalButton;

    private static ISleekBox selectedBox;

    private static ISleekField tableNameField;

    private static SleekButtonIcon addTableButton;

    private static SleekButtonIcon removeTableButton;

    private static ISleekSlider radiusSlider;

    private static SleekButtonIcon addButton;

    private static SleekButtonIcon removeButton;

    private static byte selectedTier;

    private static byte selectAnimal;

    public static void open()
    {
        if (!active)
        {
            active = true;
            EditorSpawns.isSpawning = true;
            EditorSpawns.spawnMode = ESpawnMode.ADD_ANIMAL;
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
        tableButtons = new ISleekButton[LevelAnimals.tables.Count];
        tableScrollBox.ContentSizeOffset = new Vector2(0f, tableButtons.Length * 40 - 10);
        for (int j = 0; j < tableButtons.Length; j++)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = 240f;
            sleekButton.PositionOffset_Y = j * 40;
            sleekButton.SizeOffset_X = 200f;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.Text = j + " " + LevelAnimals.tables[j].name;
            sleekButton.OnClicked += onClickedTableButton;
            tableScrollBox.AddChild(sleekButton);
            tableButtons[j] = sleekButton;
        }
    }

    public static void updateSelection()
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count)
        {
            AnimalTable animalTable = LevelAnimals.tables[EditorSpawns.selectedAnimal];
            selectedBox.Text = animalTable.name;
            tableNameField.Text = animalTable.name;
            tableIDField.Value = animalTable.tableID;
            tableColorPicker.state = animalTable.color;
            if (tierButtons != null)
            {
                for (int i = 0; i < tierButtons.Length; i++)
                {
                    spawnsScrollBox.RemoveChild(tierButtons[i]);
                }
            }
            tierButtons = new ISleekButton[animalTable.tiers.Count];
            for (int j = 0; j < tierButtons.Length; j++)
            {
                AnimalTier animalTier = animalTable.tiers[j];
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.PositionOffset_X = 240f;
                sleekButton.PositionOffset_Y = 170 + j * 70;
                sleekButton.SizeOffset_X = 200f;
                sleekButton.SizeOffset_Y = 30f;
                sleekButton.Text = animalTier.name;
                sleekButton.OnClicked += onClickedTierButton;
                spawnsScrollBox.AddChild(sleekButton);
                ISleekSlider sleekSlider = Glazier.Get().CreateSlider();
                sleekSlider.PositionOffset_Y = 40f;
                sleekSlider.SizeOffset_X = 200f;
                sleekSlider.SizeOffset_Y = 20f;
                sleekSlider.Orientation = ESleekOrientation.HORIZONTAL;
                sleekSlider.Value = animalTier.chance;
                sleekSlider.AddLabel(Mathf.RoundToInt(animalTier.chance * 100f) + "%", ESleekSide.LEFT);
                sleekSlider.OnValueChanged += onDraggedChanceSlider;
                sleekButton.AddChild(sleekSlider);
                tierButtons[j] = sleekButton;
            }
            tierNameField.PositionOffset_Y = 170 + tierButtons.Length * 70;
            addTierButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            removeTierButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 40;
            if (animalButtons != null)
            {
                for (int k = 0; k < animalButtons.Length; k++)
                {
                    spawnsScrollBox.RemoveChild(animalButtons[k]);
                }
            }
            if (selectedTier < animalTable.tiers.Count)
            {
                tierNameField.Text = animalTable.tiers[selectedTier].name;
                animalButtons = new ISleekButton[animalTable.tiers[selectedTier].table.Count];
                for (int l = 0; l < animalButtons.Length; l++)
                {
                    ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                    sleekButton2.PositionOffset_X = 240f;
                    sleekButton2.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + l * 40;
                    sleekButton2.SizeOffset_X = 200f;
                    sleekButton2.SizeOffset_Y = 30f;
                    AnimalAsset animalAsset = Assets.find(EAssetType.ANIMAL, animalTable.tiers[selectedTier].table[l].animal) as AnimalAsset;
                    string text = "?";
                    if (animalAsset != null)
                    {
                        text = ((!string.IsNullOrEmpty(animalAsset.animalName)) ? animalAsset.animalName : animalAsset.name);
                    }
                    sleekButton2.Text = animalTable.tiers[selectedTier].table[l].animal + " " + text;
                    sleekButton2.OnClicked += onClickAnimalButton;
                    spawnsScrollBox.AddChild(sleekButton2);
                    animalButtons[l] = sleekButton2;
                }
            }
            else
            {
                tierNameField.Text = "";
                animalButtons = new ISleekButton[0];
            }
            animalIDField.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + animalButtons.Length * 40;
            addAnimalButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + animalButtons.Length * 40 + 40;
            removeAnimalButton.PositionOffset_Y = 170 + tierButtons.Length * 70 + 80 + animalButtons.Length * 40 + 40;
            spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 170 + tierButtons.Length * 70 + 80 + animalButtons.Length * 40 + 70);
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
        if (animalButtons != null)
        {
            for (int n = 0; n < animalButtons.Length; n++)
            {
                spawnsScrollBox.RemoveChild(animalButtons[n]);
            }
        }
        animalButtons = null;
        animalIDField.PositionOffset_Y = 250f;
        addAnimalButton.PositionOffset_Y = 290f;
        removeAnimalButton.PositionOffset_Y = 290f;
        spawnsScrollBox.ContentSizeOffset = new Vector2(0f, 320f);
    }

    private static void onClickedTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal != (byte)(button.PositionOffset_Y / 40f))
        {
            EditorSpawns.selectedAnimal = (byte)(button.PositionOffset_Y / 40f);
            EditorSpawns.animalSpawn.GetComponent<Renderer>().material.color = LevelAnimals.tables[EditorSpawns.selectedAnimal].color;
        }
        else
        {
            EditorSpawns.selectedAnimal = byte.MaxValue;
            EditorSpawns.animalSpawn.GetComponent<Renderer>().material.color = Color.white;
        }
        updateSelection();
    }

    private static void onAnimalColorPicked(SleekColorPicker picker, Color color)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count)
        {
            LevelAnimals.tables[EditorSpawns.selectedAnimal].color = color;
        }
    }

    private static void onTableIDFieldTyped(ISleekUInt16Field field, ushort state)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count)
        {
            LevelAnimals.tables[EditorSpawns.selectedAnimal].tableID = state;
        }
    }

    private static void onClickedTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count)
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

    private static void onClickAnimalButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count)
        {
            selectAnimal = (byte)((button.PositionOffset_Y - 170f - (float)(tierButtons.Length * 70) - 80f) / 40f);
            updateSelection();
        }
    }

    private static void onDraggedChanceSlider(ISleekSlider slider, float state)
    {
        if (EditorSpawns.selectedAnimal >= LevelAnimals.tables.Count)
        {
            return;
        }
        int num = Mathf.FloorToInt((slider.Parent.PositionOffset_Y - 170f) / 70f);
        LevelAnimals.tables[EditorSpawns.selectedAnimal].updateChance(num, state);
        for (int i = 0; i < LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers.Count; i++)
        {
            AnimalTier animalTier = LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers[i];
            ISleekSlider sleekSlider = (ISleekSlider)tierButtons[i].GetChildAtIndex(0);
            if (i != num)
            {
                sleekSlider.Value = animalTier.chance;
            }
            sleekSlider.UpdateLabel(Mathf.RoundToInt(animalTier.chance * 100f) + "%");
        }
    }

    private static void onTypedNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count)
        {
            selectedBox.Text = state;
            LevelAnimals.tables[EditorSpawns.selectedAnimal].name = state;
            tableButtons[EditorSpawns.selectedAnimal].Text = EditorSpawns.selectedAnimal + " " + state;
        }
    }

    private static void onClickedAddTableButton(ISleekElement button)
    {
        if (tableNameField.Text != "")
        {
            LevelAnimals.addTable(tableNameField.Text);
            tableNameField.Text = "";
            updateTables();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onClickedRemoveTableButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count)
        {
            LevelAnimals.removeTable();
            updateTables();
            updateSelection();
            tableScrollBox.ScrollToBottom();
        }
    }

    private static void onTypedTierNameField(ISleekField field, string state)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count && selectedTier < LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers.Count)
        {
            LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers[selectedTier].name = state;
            tierButtons[selectedTier].Text = state;
        }
    }

    private static void onClickedAddTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count && tierNameField.Text != "")
        {
            LevelAnimals.tables[EditorSpawns.selectedAnimal].addTier(tierNameField.Text);
            tierNameField.Text = "";
            updateSelection();
        }
    }

    private static void onClickedRemoveTierButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count && selectedTier < LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers.Count)
        {
            LevelAnimals.tables[EditorSpawns.selectedAnimal].removeTier(selectedTier);
            updateSelection();
        }
    }

    private static void onClickedAddAnimalButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count && selectedTier < LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers.Count)
        {
            if (Assets.find(EAssetType.ANIMAL, animalIDField.Value) is AnimalAsset)
            {
                LevelAnimals.tables[EditorSpawns.selectedAnimal].addAnimal(selectedTier, animalIDField.Value);
                updateSelection();
                spawnsScrollBox.ScrollToBottom();
            }
            animalIDField.Value = 0;
        }
    }

    private static void onClickedRemoveAnimalButton(ISleekElement button)
    {
        if (EditorSpawns.selectedAnimal < LevelAnimals.tables.Count && selectedTier < LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers.Count && selectAnimal < LevelAnimals.tables[EditorSpawns.selectedAnimal].tiers[selectedTier].table.Count)
        {
            LevelAnimals.tables[EditorSpawns.selectedAnimal].removeAnimal(selectedTier, selectAnimal);
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
        EditorSpawns.spawnMode = ESpawnMode.ADD_ANIMAL;
    }

    private static void onClickedRemoveButton(ISleekElement button)
    {
        EditorSpawns.spawnMode = ESpawnMode.REMOVE_ANIMAL;
    }

    public EditorSpawnsAnimalsUI()
    {
        Local local = Localization.read("/Editor/EditorSpawnsAnimals.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorSpawnsAnimals/EditorSpawnsAnimals.unity3d");
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
        tableColorPicker.onColorPicked = onAnimalColorPicked;
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
        animalIDField = Glazier.Get().CreateUInt16Field();
        animalIDField.PositionOffset_X = 240f;
        animalIDField.SizeOffset_X = 200f;
        animalIDField.SizeOffset_Y = 30f;
        animalIDField.AddLabel(local.format("AnimalIDFieldLabelText"), ESleekSide.LEFT);
        spawnsScrollBox.AddChild(animalIDField);
        addAnimalButton = new SleekButtonIcon(bundle.load<Texture2D>("Add"));
        addAnimalButton.PositionOffset_X = 240f;
        addAnimalButton.SizeOffset_X = 95f;
        addAnimalButton.SizeOffset_Y = 30f;
        addAnimalButton.text = local.format("AddAnimalButtonText");
        addAnimalButton.tooltip = local.format("AddAnimalButtonTooltip");
        addAnimalButton.onClickedButton += onClickedAddAnimalButton;
        spawnsScrollBox.AddChild(addAnimalButton);
        removeAnimalButton = new SleekButtonIcon(bundle.load<Texture2D>("Remove"));
        removeAnimalButton.PositionOffset_X = 345f;
        removeAnimalButton.SizeOffset_X = 95f;
        removeAnimalButton.SizeOffset_Y = 30f;
        removeAnimalButton.text = local.format("RemoveAnimalButtonText");
        removeAnimalButton.tooltip = local.format("RemoveAnimalButtonTooltip");
        removeAnimalButton.onClickedButton += onClickedRemoveAnimalButton;
        spawnsScrollBox.AddChild(removeAnimalButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = -230f;
        selectedBox.PositionOffset_Y = 80f;
        selectedBox.PositionScale_X = 1f;
        selectedBox.SizeOffset_X = 230f;
        selectedBox.SizeOffset_Y = 30f;
        selectedBox.AddLabel(local.format("SelectionBoxLabelText"), ESleekSide.LEFT);
        container.AddChild(selectedBox);
        tierButtons = null;
        animalButtons = null;
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
