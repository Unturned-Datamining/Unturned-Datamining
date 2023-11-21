using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationControlsUI
{
    private static byte[][] layouts = new byte[10][]
    {
        new byte[6]
        {
            ControlsSettings.UP,
            ControlsSettings.DOWN,
            ControlsSettings.LEFT,
            ControlsSettings.RIGHT,
            ControlsSettings.JUMP,
            ControlsSettings.SPRINT
        },
        new byte[7]
        {
            ControlsSettings.CROUCH,
            ControlsSettings.PRONE,
            ControlsSettings.STANCE,
            ControlsSettings.LEAN_LEFT,
            ControlsSettings.LEAN_RIGHT,
            ControlsSettings.PERSPECTIVE,
            ControlsSettings.GESTURE
        },
        new byte[3]
        {
            ControlsSettings.INTERACT,
            ControlsSettings.PRIMARY,
            ControlsSettings.SECONDARY
        },
        new byte[8]
        {
            ControlsSettings.RELOAD,
            ControlsSettings.ATTACH,
            ControlsSettings.FIREMODE,
            ControlsSettings.TACTICAL,
            ControlsSettings.VISION,
            ControlsSettings.INSPECT,
            ControlsSettings.ROTATE,
            ControlsSettings.DEQUIP
        },
        new byte[4]
        {
            ControlsSettings.VOICE,
            ControlsSettings.GLOBAL,
            ControlsSettings.LOCAL,
            ControlsSettings.GROUP
        },
        new byte[9]
        {
            ControlsSettings.HUD,
            ControlsSettings.OTHER,
            ControlsSettings.DASHBOARD,
            ControlsSettings.INVENTORY,
            ControlsSettings.CRAFTING,
            ControlsSettings.SKILLS,
            ControlsSettings.MAP,
            ControlsSettings.QUESTS,
            ControlsSettings.PLAYERS
        },
        new byte[9]
        {
            ControlsSettings.LOCKER,
            ControlsSettings.ROLL_LEFT,
            ControlsSettings.ROLL_RIGHT,
            ControlsSettings.PITCH_UP,
            ControlsSettings.PITCH_DOWN,
            ControlsSettings.YAW_LEFT,
            ControlsSettings.YAW_RIGHT,
            ControlsSettings.THRUST_INCREASE,
            ControlsSettings.THRUST_DECREASE
        },
        new byte[13]
        {
            ControlsSettings.MODIFY,
            ControlsSettings.SNAP,
            ControlsSettings.FOCUS,
            ControlsSettings.ASCEND,
            ControlsSettings.DESCEND,
            ControlsSettings.TOOL_0,
            ControlsSettings.TOOL_1,
            ControlsSettings.TOOL_2,
            ControlsSettings.TOOL_3,
            ControlsSettings.TERMINAL,
            ControlsSettings.SCREENSHOT,
            ControlsSettings.REFRESH_ASSETS,
            ControlsSettings.CLIPBOARD_DEBUG
        },
        new byte[6]
        {
            ControlsSettings.PLUGIN_0,
            ControlsSettings.PLUGIN_1,
            ControlsSettings.PLUGIN_2,
            ControlsSettings.PLUGIN_3,
            ControlsSettings.PLUGIN_4,
            74
        },
        new byte[10] { 64, 65, 66, 67, 68, 69, 70, 71, 72, 73 }
    };

    private static Local localization;

    private static Local localizationKeyCodes;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekButton defaultButton;

    private static ISleekFloat32Field sensitivityField;

    private static SleekButtonState sensitivityScalingModeButton;

    private static ISleekFloat32Field projectionRatioCoefficientField;

    private static ISleekToggle invertToggle;

    private static ISleekToggle invertFlightToggle;

    private static ISleekScrollView controlsBox;

    private static ISleekButton[] buttons;

    private static SleekButtonState aimingButton;

    private static SleekButtonState crouchingButton;

    private static SleekButtonState proningButton;

    private static SleekButtonState sprintingButton;

    private static SleekButtonState leaningButton;

    public static byte binding = byte.MaxValue;

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
            binding = byte.MaxValue;
            MenuSettings.SaveControlsIfLoaded();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static void cancel()
    {
        binding = byte.MaxValue;
    }

    public static void bind(KeyCode key)
    {
        ControlsSettings.bind(binding, key);
        updateButton(binding);
        cancel();
    }

    public static string getKeyCodeText(KeyCode key)
    {
        if (localizationKeyCodes == null)
        {
            localizationKeyCodes = Localization.read("/Shared/KeyCodes.dat");
        }
        string text = key.ToString();
        if (localizationKeyCodes.has(text))
        {
            text = localizationKeyCodes.format(text);
        }
        return text;
    }

    public static void updateButton(byte index)
    {
        string keyCodeText = getKeyCodeText(ControlsSettings.bindings[index].key);
        buttons[index].Text = localization.format("Key_" + index + "_Button", keyCodeText);
    }

    private static void onTypedSensitivityField(ISleekFloat32Field field, float state)
    {
        ControlsSettings.mouseAimSensitivity = state;
    }

    private static void onTypedProjectionRatioCoefficientField(ISleekFloat32Field field, float state)
    {
        ControlsSettings.projectionRatioCoefficient = state;
    }

    private static void onToggledInvertToggle(ISleekToggle toggle, bool state)
    {
        ControlsSettings.invert = state;
    }

    private static void onToggledInvertFlightToggle(ISleekToggle toggle, bool state)
    {
        ControlsSettings.invertFlight = state;
    }

    private static void onSwappedAimingState(SleekButtonState button, int index)
    {
        ControlsSettings.aiming = (EControlMode)index;
    }

    private static void onSwappedCrouchingState(SleekButtonState button, int index)
    {
        ControlsSettings.crouching = (EControlMode)index;
    }

    private static void onSwappedProningState(SleekButtonState button, int index)
    {
        ControlsSettings.proning = (EControlMode)index;
    }

    private static void onSwappedSprintingState(SleekButtonState button, int index)
    {
        ControlsSettings.sprinting = (EControlMode)index;
    }

    private static void onSwappedLeaningState(SleekButtonState button, int index)
    {
        ControlsSettings.leaning = (EControlMode)index;
    }

    private static void OnSwappedSensitivityScalingMode(SleekButtonState button, int index)
    {
        ControlsSettings.sensitivityScalingMode = (ESensitivityScalingMode)index;
    }

    private static void onClickedKeyButton(ISleekElement button)
    {
        binding = 0;
        while (binding < buttons.Length && buttons[binding] != button)
        {
            binding++;
        }
        (button as ISleekButton).Text = localization.format("Key_" + binding + "_Button", "?");
    }

    public static void bindOnGUI()
    {
        if (binding == byte.MaxValue)
        {
            return;
        }
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Backspace)
            {
                updateButton(binding);
                cancel();
            }
            else if (Event.current.keyCode != KeyCode.Escape && Event.current.keyCode != KeyCode.Insert)
            {
                bind(Event.current.keyCode);
            }
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            if (Event.current.button == 0)
            {
                bind(KeyCode.Mouse0);
            }
            else if (Event.current.button == 1)
            {
                bind(KeyCode.Mouse1);
            }
            else if (Event.current.button == 2)
            {
                bind(KeyCode.Mouse2);
            }
            else if (Event.current.button == 3)
            {
                bind(KeyCode.Mouse3);
            }
            else if (Event.current.button == 4)
            {
                bind(KeyCode.Mouse4);
            }
            else if (Event.current.button == 5)
            {
                bind(KeyCode.Mouse5);
            }
            else if (Event.current.button == 6)
            {
                bind(KeyCode.Mouse6);
            }
        }
        else if (Event.current.shift)
        {
            bind(KeyCode.LeftShift);
        }
    }

    public static void bindUpdate()
    {
        if (binding != byte.MaxValue)
        {
            if (InputEx.GetKeyDown(KeyCode.Mouse3))
            {
                bind(KeyCode.Mouse3);
            }
            else if (InputEx.GetKeyDown(KeyCode.Mouse4))
            {
                bind(KeyCode.Mouse4);
            }
            else if (InputEx.GetKeyDown(KeyCode.Mouse5))
            {
                bind(KeyCode.Mouse5);
            }
            else if (InputEx.GetKeyDown(KeyCode.Mouse6))
            {
                bind(KeyCode.Mouse6);
            }
        }
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        if (Player.player != null)
        {
            PlayerPauseUI.open();
        }
        else if (Level.isEditor)
        {
            EditorPauseUI.open();
        }
        else
        {
            MenuConfigurationUI.open();
        }
        close();
    }

    private static void onClickedDefaultButton(ISleekElement button)
    {
        ControlsSettings.restoreDefaults();
        updateAll();
    }

    private static void updateAll()
    {
        for (byte b = 0; b < layouts.Length; b++)
        {
            for (byte b2 = 0; b2 < layouts[b].Length; b2++)
            {
                updateButton(layouts[b][b2]);
            }
        }
        leaningButton.state = (int)ControlsSettings.leaning;
        sprintingButton.state = (int)ControlsSettings.sprinting;
        proningButton.state = (int)ControlsSettings.proning;
        crouchingButton.state = (int)ControlsSettings.crouching;
        aimingButton.state = (int)ControlsSettings.aiming;
        sensitivityField.Value = ControlsSettings.mouseAimSensitivity;
        projectionRatioCoefficientField.Value = ControlsSettings.projectionRatioCoefficient;
        invertToggle.Value = ControlsSettings.invert;
        invertFlightToggle.Value = ControlsSettings.invert;
        sensitivityScalingModeButton.state = (int)ControlsSettings.sensitivityScalingMode;
    }

    public MenuConfigurationControlsUI()
    {
        localization = Localization.read("/Menu/Configuration/MenuConfigurationControls.dat");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        if (Provider.isConnected)
        {
            PlayerUI.container.AddChild(container);
        }
        else if (Level.isEditor)
        {
            EditorUI.window.AddChild(container);
        }
        else
        {
            MenuUI.container.AddChild(container);
        }
        active = false;
        binding = byte.MaxValue;
        controlsBox = Glazier.Get().CreateScrollView();
        controlsBox.PositionOffset_X = -200f;
        controlsBox.PositionOffset_Y = 100f;
        controlsBox.PositionScale_X = 0.5f;
        controlsBox.SizeOffset_X = 430f;
        controlsBox.SizeOffset_Y = -200f;
        controlsBox.SizeScale_Y = 1f;
        controlsBox.ScaleContentToWidth = true;
        container.AddChild(controlsBox);
        int num = 0;
        invertToggle = Glazier.Get().CreateToggle();
        invertToggle.PositionOffset_Y = num;
        invertToggle.SizeOffset_X = 40f;
        invertToggle.SizeOffset_Y = 40f;
        invertToggle.AddLabel(localization.format("Invert_Toggle_Label"), ESleekSide.RIGHT);
        invertToggle.OnValueChanged += onToggledInvertToggle;
        controlsBox.AddChild(invertToggle);
        num += 50;
        invertFlightToggle = Glazier.Get().CreateToggle();
        invertFlightToggle.PositionOffset_Y = num;
        invertFlightToggle.SizeOffset_X = 40f;
        invertFlightToggle.SizeOffset_Y = 40f;
        invertFlightToggle.AddLabel(localization.format("Invert_Flight_Toggle_Label"), ESleekSide.RIGHT);
        invertFlightToggle.OnValueChanged += onToggledInvertFlightToggle;
        controlsBox.AddChild(invertFlightToggle);
        num += 50;
        sensitivityField = Glazier.Get().CreateFloat32Field();
        sensitivityField.PositionOffset_Y = num;
        sensitivityField.SizeOffset_X = 200f;
        sensitivityField.SizeOffset_Y = 30f;
        sensitivityField.AddLabel(localization.format("Sensitivity_Field_Label"), ESleekSide.RIGHT);
        sensitivityField.OnValueChanged += onTypedSensitivityField;
        controlsBox.AddChild(sensitivityField);
        num += 40;
        sensitivityScalingModeButton = new SleekButtonState(new GUIContent(localization.format("SensitivityScalingMode_ProjectionRatio"), localization.format("SensitivityScalingMode_ProjectionRatio_Tooltip")), new GUIContent(localization.format("SensitivityScalingMode_ZoomFactor"), localization.format("SensitivityScalingMode_ZoomFactor_Tooltip")), new GUIContent(localization.format("SensitivityScalingMode_Legacy"), localization.format("SensitivityScalingMode_Legacy_Tooltip")), new GUIContent(localization.format("SensitivityScalingMode_None"), localization.format("SensitivityScalingMode_None_Tooltip")));
        sensitivityScalingModeButton.PositionOffset_Y = num;
        sensitivityScalingModeButton.SizeOffset_X = 200f;
        sensitivityScalingModeButton.SizeOffset_Y = 30f;
        sensitivityScalingModeButton.AddLabel(localization.format("SensitivityScalingMode_Label"), ESleekSide.RIGHT);
        sensitivityScalingModeButton.onSwappedState = OnSwappedSensitivityScalingMode;
        sensitivityScalingModeButton.UseContentTooltip = true;
        controlsBox.AddChild(sensitivityScalingModeButton);
        num += 40;
        projectionRatioCoefficientField = Glazier.Get().CreateFloat32Field();
        projectionRatioCoefficientField.PositionOffset_Y = num;
        projectionRatioCoefficientField.SizeOffset_X = 200f;
        projectionRatioCoefficientField.SizeOffset_Y = 30f;
        projectionRatioCoefficientField.TooltipText = localization.format("ProjectionRatioCoefficient_Tooltip");
        projectionRatioCoefficientField.AddLabel(localization.format("ProjectionRatioCoefficient_Label"), ESleekSide.RIGHT);
        projectionRatioCoefficientField.OnValueChanged += onTypedProjectionRatioCoefficientField;
        controlsBox.AddChild(projectionRatioCoefficientField);
        num += 40;
        aimingButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
        aimingButton.PositionOffset_Y = num;
        aimingButton.SizeOffset_X = 200f;
        aimingButton.SizeOffset_Y = 30f;
        aimingButton.AddLabel(localization.format("Aiming_Label"), ESleekSide.RIGHT);
        aimingButton.onSwappedState = onSwappedAimingState;
        controlsBox.AddChild(aimingButton);
        num += 40;
        crouchingButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
        crouchingButton.PositionOffset_Y = num;
        crouchingButton.SizeOffset_X = 200f;
        crouchingButton.SizeOffset_Y = 30f;
        crouchingButton.AddLabel(localization.format("Crouching_Label"), ESleekSide.RIGHT);
        crouchingButton.onSwappedState = onSwappedCrouchingState;
        controlsBox.AddChild(crouchingButton);
        num += 40;
        proningButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
        proningButton.PositionOffset_Y = num;
        proningButton.SizeOffset_X = 200f;
        proningButton.SizeOffset_Y = 30f;
        proningButton.AddLabel(localization.format("Proning_Label"), ESleekSide.RIGHT);
        proningButton.onSwappedState = onSwappedProningState;
        controlsBox.AddChild(proningButton);
        num += 40;
        sprintingButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
        sprintingButton.PositionOffset_Y = num;
        sprintingButton.SizeOffset_X = 200f;
        sprintingButton.SizeOffset_Y = 30f;
        sprintingButton.AddLabel(localization.format("Sprinting_Label"), ESleekSide.RIGHT);
        sprintingButton.onSwappedState = onSwappedSprintingState;
        controlsBox.AddChild(sprintingButton);
        num += 40;
        leaningButton = new SleekButtonState(new GUIContent(localization.format("Hold")), new GUIContent(localization.format("Toggle")));
        leaningButton.PositionOffset_Y = num;
        leaningButton.SizeOffset_X = 200f;
        leaningButton.SizeOffset_Y = 30f;
        leaningButton.AddLabel(localization.format("Leaning_Label"), ESleekSide.RIGHT);
        leaningButton.onSwappedState = onSwappedLeaningState;
        controlsBox.AddChild(leaningButton);
        num += 40;
        buttons = new ISleekButton[ControlsSettings.bindings.Length];
        for (byte b = 0; b < layouts.Length; b++)
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.PositionOffset_Y = num;
            sleekBox.SizeOffset_Y = 30f;
            sleekBox.SizeScale_X = 1f;
            sleekBox.Text = localization.format("Layout_" + b);
            controlsBox.AddChild(sleekBox);
            num += 40;
            for (byte b2 = 0; b2 < layouts[b].Length; b2++)
            {
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.PositionOffset_Y = 40 + b2 * 30;
                sleekButton.SizeOffset_Y = 30f;
                sleekButton.SizeScale_X = 1f;
                sleekButton.OnClicked += onClickedKeyButton;
                sleekBox.AddChild(sleekButton);
                num += 30;
                buttons[layouts[b][b2]] = sleekButton;
            }
            num += 10;
        }
        controlsBox.ContentSizeOffset = new Vector2(0f, num - 10);
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
        defaultButton = Glazier.Get().CreateButton();
        defaultButton.PositionOffset_X = -200f;
        defaultButton.PositionOffset_Y = -50f;
        defaultButton.PositionScale_X = 1f;
        defaultButton.PositionScale_Y = 1f;
        defaultButton.SizeOffset_X = 200f;
        defaultButton.SizeOffset_Y = 50f;
        defaultButton.Text = MenuPlayConfigUI.localization.format("Default");
        defaultButton.TooltipText = MenuPlayConfigUI.localization.format("Default_Tooltip");
        defaultButton.OnClicked += onClickedDefaultButton;
        defaultButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(defaultButton);
        updateAll();
    }
}
