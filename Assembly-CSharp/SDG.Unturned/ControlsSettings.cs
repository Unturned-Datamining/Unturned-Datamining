using UnityEngine;

namespace SDG.Unturned;

public class ControlsSettings
{
    private const byte SAVEDATA_VERSION_ADDED_SENSITIVITY_SCALING_MODE = 19;

    private const byte SAVEDATA_VERSION_ADDED_SCALING_COEFFICIENT = 20;

    private const byte SAVEDATA_VERSION_NEWEST = 20;

    public static readonly byte SAVEDATA_VERSION;

    public static readonly byte LEFT;

    public static readonly byte RIGHT;

    public static readonly byte UP;

    public static readonly byte DOWN;

    public static readonly byte JUMP;

    public static readonly byte LEAN_LEFT;

    public static readonly byte LEAN_RIGHT;

    public static readonly byte PRIMARY;

    public static readonly byte SECONDARY;

    public static readonly byte INTERACT;

    public static readonly byte CROUCH;

    public static readonly byte PRONE;

    public static readonly byte SPRINT;

    public static readonly byte RELOAD;

    public static readonly byte ATTACH;

    public static readonly byte FIREMODE;

    public static readonly byte DASHBOARD;

    public static readonly byte INVENTORY;

    public static readonly byte CRAFTING;

    public static readonly byte SKILLS;

    public static readonly byte MAP;

    public static readonly byte QUESTS;

    public static readonly byte PLAYERS;

    public static readonly byte VOICE;

    public static readonly byte MODIFY;

    public static readonly byte SNAP;

    public static readonly byte FOCUS;

    public static readonly byte ASCEND;

    public static readonly byte DESCEND;

    public static readonly byte TOOL_0;

    public static readonly byte TOOL_1;

    public static readonly byte TOOL_2;

    public static readonly byte TOOL_3;

    public static readonly byte TERMINAL;

    public static readonly byte SCREENSHOT;

    public static readonly byte REFRESH_ASSETS;

    public static readonly byte CLIPBOARD_DEBUG;

    public static readonly byte HUD;

    public static readonly byte OTHER;

    public static readonly byte GLOBAL;

    public static readonly byte LOCAL;

    public static readonly byte GROUP;

    public static readonly byte GESTURE;

    public static readonly byte VISION;

    public static readonly byte TACTICAL;

    public static readonly byte PERSPECTIVE;

    public static readonly byte DEQUIP;

    public static readonly byte STANCE;

    public static readonly byte ROLL_LEFT;

    public static readonly byte ROLL_RIGHT;

    public static readonly byte PITCH_UP;

    public static readonly byte PITCH_DOWN;

    public static readonly byte YAW_LEFT;

    public static readonly byte YAW_RIGHT;

    public static readonly byte THRUST_INCREASE;

    public static readonly byte THRUST_DECREASE;

    public static readonly byte LOCKER;

    public static readonly byte INSPECT;

    public static readonly byte ROTATE;

    public static readonly byte PLUGIN_0;

    public static readonly byte PLUGIN_1;

    public static readonly byte PLUGIN_2;

    public static readonly byte PLUGIN_3;

    public static readonly byte PLUGIN_4;

    public static readonly byte NUM_PLUGIN_KEYS;

    public static readonly string[] PLUGIN_KEY_TOKENS;

    public const byte ITEM_0 = 64;

    public const byte ITEM_1 = 65;

    public const byte ITEM_2 = 66;

    public const byte ITEM_3 = 67;

    public const byte ITEM_4 = 68;

    public const byte ITEM_5 = 69;

    public const byte ITEM_6 = 70;

    public const byte ITEM_7 = 71;

    public const byte ITEM_8 = 72;

    public const byte ITEM_9 = 73;

    public const byte NUM_ITEM_HOTBAR_KEYS = 10;

    public const byte CUSTOM_MODAL = 74;

    public static float mouseAimSensitivity;

    public static bool invert;

    public static bool invertFlight;

    public static EControlMode aiming;

    public static EControlMode crouching;

    public static EControlMode proning;

    public static EControlMode sprinting;

    public static EControlMode leaning;

    public static ESensitivityScalingMode sensitivityScalingMode;

    public static float projectionRatioCoefficient;

    private static ControlBinding[] _bindings;

    public static ControlBinding[] bindings => _bindings;

    public static KeyCode left => bindings[LEFT].key;

    public static KeyCode up => bindings[UP].key;

    public static KeyCode right => bindings[RIGHT].key;

    public static KeyCode down => bindings[DOWN].key;

    public static KeyCode jump => bindings[JUMP].key;

    public static KeyCode leanLeft => bindings[LEAN_LEFT].key;

    public static KeyCode leanRight => bindings[LEAN_RIGHT].key;

    public static KeyCode rollLeft => bindings[ROLL_LEFT].key;

    public static KeyCode rollRight => bindings[ROLL_RIGHT].key;

    public static KeyCode pitchUp => bindings[PITCH_UP].key;

    public static KeyCode pitchDown => bindings[PITCH_DOWN].key;

    public static KeyCode primary => bindings[PRIMARY].key;

    public static KeyCode yawLeft => bindings[YAW_LEFT].key;

    public static KeyCode yawRight => bindings[YAW_RIGHT].key;

    public static KeyCode thrustIncrease => bindings[THRUST_INCREASE].key;

    public static KeyCode thrustDecrease => bindings[THRUST_DECREASE].key;

    public static KeyCode locker => bindings[LOCKER].key;

    public static KeyCode secondary => bindings[SECONDARY].key;

    public static KeyCode reload => bindings[RELOAD].key;

    public static KeyCode attach => bindings[ATTACH].key;

    public static KeyCode firemode => bindings[FIREMODE].key;

    public static KeyCode dashboard => bindings[DASHBOARD].key;

    public static KeyCode inventory => bindings[INVENTORY].key;

    public static KeyCode crafting => bindings[CRAFTING].key;

    public static KeyCode skills => bindings[SKILLS].key;

    public static KeyCode map => bindings[MAP].key;

    public static KeyCode quests => bindings[QUESTS].key;

    public static KeyCode players => bindings[PLAYERS].key;

    public static KeyCode voice => bindings[VOICE].key;

    public static KeyCode interact => bindings[INTERACT].key;

    public static KeyCode crouch => bindings[CROUCH].key;

    public static KeyCode prone => bindings[PRONE].key;

    public static KeyCode stance => bindings[STANCE].key;

    public static KeyCode sprint => bindings[SPRINT].key;

    public static KeyCode modify => bindings[MODIFY].key;

    public static KeyCode snap => bindings[SNAP].key;

    public static KeyCode focus => bindings[FOCUS].key;

    public static KeyCode ascend => bindings[ASCEND].key;

    public static KeyCode descend => bindings[DESCEND].key;

    public static KeyCode tool_0 => bindings[TOOL_0].key;

    public static KeyCode tool_1 => bindings[TOOL_1].key;

    public static KeyCode tool_2 => bindings[TOOL_2].key;

    public static KeyCode tool_3 => bindings[TOOL_3].key;

    public static KeyCode terminal => bindings[TERMINAL].key;

    public static KeyCode screenshot => bindings[SCREENSHOT].key;

    public static KeyCode refreshAssets => bindings[REFRESH_ASSETS].key;

    public static KeyCode clipboardDebug => bindings[CLIPBOARD_DEBUG].key;

    public static KeyCode hud => bindings[HUD].key;

    public static KeyCode other => bindings[OTHER].key;

    public static KeyCode global => bindings[GLOBAL].key;

    public static KeyCode local => bindings[LOCAL].key;

    public static KeyCode group => bindings[GROUP].key;

    public static KeyCode gesture => bindings[GESTURE].key;

    public static KeyCode vision => bindings[VISION].key;

    public static KeyCode tactical => bindings[TACTICAL].key;

    public static KeyCode perspective => bindings[PERSPECTIVE].key;

    public static KeyCode dequip => bindings[DEQUIP].key;

    public static KeyCode inspect => bindings[INSPECT].key;

    public static KeyCode rotate => bindings[ROTATE].key;

    public static KeyCode CustomModal => bindings[74].key;

    public static void formatPluginHotkeysIntoText(ref string text)
    {
        for (int i = 0; i < NUM_PLUGIN_KEYS; i++)
        {
            KeyCode pluginKeyCode = getPluginKeyCode(i);
            string oldValue = PLUGIN_KEY_TOKENS[i];
            string keyCodeText = MenuConfigurationControlsUI.getKeyCodeText(pluginKeyCode);
            text = text.Replace(oldValue, keyCodeText);
        }
    }

    public static string getEquipmentHotkeyText(int index)
    {
        return MenuConfigurationControlsUI.getKeyCodeText(getEquipmentHotbarKeyCode(index));
    }

    public static KeyCode getPluginKeyCode(int index)
    {
        return bindings[PLUGIN_0 + index].key;
    }

    public static KeyCode getEquipmentHotbarKeyCode(int index)
    {
        return bindings[64 + index].key;
    }

    private static bool isTooImportantToMessUp(KeyCode key)
    {
        return key switch
        {
            KeyCode.Mouse0 => true, 
            KeyCode.Mouse1 => true, 
            _ => false, 
        };
    }

    public static void bind(byte index, KeyCode key)
    {
        if (index == HUD)
        {
            if (isTooImportantToMessUp(key))
            {
                key = KeyCode.Home;
            }
        }
        else if (index == OTHER)
        {
            if (isTooImportantToMessUp(key))
            {
                key = KeyCode.LeftControl;
            }
        }
        else if (index == TERMINAL)
        {
            if (isTooImportantToMessUp(key))
            {
                key = KeyCode.BackQuote;
            }
        }
        else if (index == REFRESH_ASSETS && isTooImportantToMessUp(key))
        {
            key = KeyCode.PageUp;
        }
        if (bindings[index] == null)
        {
            bindings[index] = new ControlBinding(key);
        }
        else
        {
            bindings[index].key = key;
        }
    }

    public static void restoreDefaults()
    {
        bind(LEFT, KeyCode.A);
        bind(RIGHT, KeyCode.D);
        bind(UP, KeyCode.W);
        bind(DOWN, KeyCode.S);
        bind(JUMP, KeyCode.Space);
        bind(LEAN_LEFT, KeyCode.Q);
        bind(LEAN_RIGHT, KeyCode.E);
        bind(PRIMARY, KeyCode.Mouse0);
        bind(SECONDARY, KeyCode.Mouse1);
        bind(INTERACT, KeyCode.F);
        bind(CROUCH, KeyCode.X);
        bind(PRONE, KeyCode.Z);
        bind(STANCE, KeyCode.O);
        bind(SPRINT, KeyCode.LeftShift);
        bind(RELOAD, KeyCode.R);
        bind(ATTACH, KeyCode.T);
        bind(FIREMODE, KeyCode.V);
        bind(DASHBOARD, KeyCode.Tab);
        bind(INVENTORY, KeyCode.G);
        bind(CRAFTING, KeyCode.Y);
        bind(SKILLS, KeyCode.U);
        bind(MAP, KeyCode.M);
        bind(QUESTS, KeyCode.I);
        bind(PLAYERS, KeyCode.P);
        bind(VOICE, KeyCode.LeftAlt);
        bind(MODIFY, KeyCode.LeftShift);
        bind(SNAP, KeyCode.LeftControl);
        bind(FOCUS, KeyCode.F);
        bind(ASCEND, KeyCode.Q);
        bind(DESCEND, KeyCode.E);
        bind(TOOL_0, KeyCode.Q);
        bind(TOOL_1, KeyCode.W);
        bind(TOOL_2, KeyCode.E);
        bind(TOOL_3, KeyCode.R);
        bind(TERMINAL, KeyCode.BackQuote);
        bind(SCREENSHOT, KeyCode.Insert);
        bind(REFRESH_ASSETS, KeyCode.PageUp);
        bind(CLIPBOARD_DEBUG, KeyCode.PageDown);
        bind(HUD, KeyCode.Home);
        bind(OTHER, KeyCode.LeftControl);
        bind(GLOBAL, KeyCode.J);
        bind(LOCAL, KeyCode.K);
        bind(GROUP, KeyCode.L);
        bind(GESTURE, KeyCode.C);
        bind(VISION, KeyCode.N);
        bind(TACTICAL, KeyCode.B);
        bind(PERSPECTIVE, KeyCode.H);
        bind(DEQUIP, KeyCode.CapsLock);
        bind(ROLL_LEFT, KeyCode.LeftArrow);
        bind(ROLL_RIGHT, KeyCode.RightArrow);
        bind(PITCH_UP, KeyCode.UpArrow);
        bind(PITCH_DOWN, KeyCode.DownArrow);
        bind(YAW_LEFT, KeyCode.A);
        bind(YAW_RIGHT, KeyCode.D);
        bind(THRUST_INCREASE, KeyCode.W);
        bind(THRUST_DECREASE, KeyCode.S);
        bind(LOCKER, KeyCode.O);
        bind(INSPECT, KeyCode.F);
        bind(ROTATE, KeyCode.R);
        bind(PLUGIN_0, KeyCode.Comma);
        bind(PLUGIN_1, KeyCode.Period);
        bind(PLUGIN_2, KeyCode.Slash);
        bind(PLUGIN_3, KeyCode.Semicolon);
        bind(PLUGIN_4, KeyCode.Quote);
        bind(64, KeyCode.Alpha1);
        bind(65, KeyCode.Alpha2);
        bind(66, KeyCode.Alpha3);
        bind(67, KeyCode.Alpha4);
        bind(68, KeyCode.Alpha5);
        bind(69, KeyCode.Alpha6);
        bind(70, KeyCode.Alpha7);
        bind(71, KeyCode.Alpha8);
        bind(72, KeyCode.Alpha9);
        bind(73, KeyCode.Alpha0);
        bind(74, KeyCode.Keypad0);
        aiming = EControlMode.HOLD;
        crouching = EControlMode.TOGGLE;
        proning = EControlMode.TOGGLE;
        sprinting = EControlMode.HOLD;
        leaning = EControlMode.HOLD;
        sensitivityScalingMode = ESensitivityScalingMode.ProjectionRatio;
        projectionRatioCoefficient = 1f;
        mouseAimSensitivity = 0.2f;
        invert = false;
        invertFlight = false;
    }

    public static void load()
    {
        restoreDefaults();
        if (!ReadWrite.fileExists("/Controls.dat", useCloud: true))
        {
            return;
        }
        Block block = ReadWrite.readBlock("/Controls.dat", useCloud: true, 0);
        if (block == null)
        {
            return;
        }
        byte b = block.readByte();
        if (b <= 10)
        {
            return;
        }
        mouseAimSensitivity = block.readSingle();
        if (b < 16)
        {
            mouseAimSensitivity = 0.2f;
        }
        else if (b < 18)
        {
            mouseAimSensitivity *= 0.2f;
        }
        invert = block.readBoolean();
        if (b > 13)
        {
            invertFlight = block.readBoolean();
        }
        else
        {
            invertFlight = false;
        }
        if (b > 11)
        {
            aiming = (EControlMode)block.readByte();
            crouching = (EControlMode)block.readByte();
            proning = (EControlMode)block.readByte();
        }
        else
        {
            aiming = EControlMode.HOLD;
            crouching = EControlMode.TOGGLE;
            proning = EControlMode.TOGGLE;
        }
        if (b > 12)
        {
            sprinting = (EControlMode)block.readByte();
        }
        else
        {
            sprinting = EControlMode.HOLD;
        }
        if (b > 14)
        {
            leaning = (EControlMode)block.readByte();
        }
        else
        {
            leaning = EControlMode.HOLD;
        }
        byte b2 = block.readByte();
        for (byte b3 = 0; b3 < b2; b3 = (byte)(b3 + 1))
        {
            if (b3 >= bindings.Length)
            {
                block.readByte();
            }
            else
            {
                ushort key = block.readUInt16();
                bind(b3, (KeyCode)key);
            }
        }
        if (b < 17)
        {
            bind(DEQUIP, KeyCode.CapsLock);
        }
        if (b < 19)
        {
            sensitivityScalingMode = ESensitivityScalingMode.ProjectionRatio;
        }
        else
        {
            sensitivityScalingMode = (ESensitivityScalingMode)block.readByte();
        }
        if (b < 20)
        {
            projectionRatioCoefficient = 1f;
        }
        else
        {
            projectionRatioCoefficient = block.readSingle();
        }
    }

    public static void save()
    {
        Block block = new Block();
        block.writeByte(20);
        block.writeSingle(mouseAimSensitivity);
        block.writeBoolean(invert);
        block.writeBoolean(invertFlight);
        block.writeByte((byte)aiming);
        block.writeByte((byte)crouching);
        block.writeByte((byte)proning);
        block.writeByte((byte)sprinting);
        block.writeByte((byte)leaning);
        block.writeByte((byte)bindings.Length);
        for (byte b = 0; b < bindings.Length; b = (byte)(b + 1))
        {
            ControlBinding controlBinding = bindings[b];
            block.writeUInt16((ushort)controlBinding.key);
        }
        block.writeByte((byte)sensitivityScalingMode);
        block.writeSingle(projectionRatioCoefficient);
        ReadWrite.writeBlock("/Controls.dat", useCloud: true, block);
    }

    static ControlsSettings()
    {
        SAVEDATA_VERSION = 20;
        LEFT = 0;
        RIGHT = 1;
        UP = 2;
        DOWN = 3;
        JUMP = 4;
        LEAN_LEFT = 5;
        LEAN_RIGHT = 6;
        PRIMARY = 7;
        SECONDARY = 8;
        INTERACT = 9;
        CROUCH = 10;
        PRONE = 11;
        SPRINT = 12;
        RELOAD = 13;
        ATTACH = 14;
        FIREMODE = 15;
        DASHBOARD = 16;
        INVENTORY = 17;
        CRAFTING = 18;
        SKILLS = 19;
        MAP = 20;
        QUESTS = 54;
        PLAYERS = 21;
        VOICE = 22;
        MODIFY = 23;
        SNAP = 24;
        FOCUS = 25;
        ASCEND = 51;
        DESCEND = 52;
        TOOL_0 = 26;
        TOOL_1 = 27;
        TOOL_2 = 28;
        TOOL_3 = 50;
        TERMINAL = 55;
        SCREENSHOT = 56;
        REFRESH_ASSETS = 57;
        CLIPBOARD_DEBUG = 58;
        HUD = 29;
        OTHER = 30;
        GLOBAL = 31;
        LOCAL = 32;
        GROUP = 33;
        GESTURE = 34;
        VISION = 35;
        TACTICAL = 36;
        PERSPECTIVE = 37;
        DEQUIP = 38;
        STANCE = 39;
        ROLL_LEFT = 40;
        ROLL_RIGHT = 41;
        PITCH_UP = 42;
        PITCH_DOWN = 43;
        YAW_LEFT = 44;
        YAW_RIGHT = 45;
        THRUST_INCREASE = 46;
        THRUST_DECREASE = 47;
        LOCKER = 53;
        INSPECT = 48;
        ROTATE = 49;
        PLUGIN_0 = 59;
        PLUGIN_1 = 60;
        PLUGIN_2 = 61;
        PLUGIN_3 = 62;
        PLUGIN_4 = 63;
        NUM_PLUGIN_KEYS = 5;
        _bindings = new ControlBinding[75];
        for (int i = 0; i < bindings.Length; i++)
        {
            bindings[i] = new ControlBinding(KeyCode.F);
        }
        PLUGIN_KEY_TOKENS = new string[NUM_PLUGIN_KEYS];
        for (int j = 0; j < NUM_PLUGIN_KEYS; j++)
        {
            string text = $"<plugin_{j}/>";
            PLUGIN_KEY_TOKENS[j] = text;
        }
    }
}
