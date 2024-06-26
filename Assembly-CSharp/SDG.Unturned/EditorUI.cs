using UnityEngine;

namespace SDG.Unturned;

public class EditorUI : MonoBehaviour
{
    public static readonly float MESSAGE_TIME = 2f;

    public static readonly float HINT_TIME = 0.15f;

    public static SleekWindow window;

    private static ISleekBox messageBox;

    private static float lastMessage;

    private static bool isMessaged;

    private static bool lastHinted;

    private static bool isHinted;

    internal static EditorUI instance;

    private EditorDashboardUI dashboardUI;

    public static void hint(EEditorMessage message, string text)
    {
        if (!isMessaged)
        {
            messageBox.IsVisible = true;
            lastHinted = true;
            isHinted = true;
            if (message == EEditorMessage.FOCUS)
            {
                messageBox.Text = text;
            }
        }
    }

    public static void message(EEditorMessage message)
    {
        if (OptionsSettings.hints)
        {
            messageBox.IsVisible = true;
            lastMessage = Time.realtimeSinceStartup;
            isMessaged = true;
            switch (message)
            {
            case EEditorMessage.HEIGHTS:
                messageBox.Text = EditorDashboardUI.localization.format("Heights", ControlsSettings.tool_2);
                break;
            case EEditorMessage.ROADS:
                messageBox.Text = EditorDashboardUI.localization.format("Roads", ControlsSettings.tool_2);
                break;
            case EEditorMessage.NAVIGATION:
                messageBox.Text = EditorDashboardUI.localization.format("Navigation", ControlsSettings.tool_2);
                break;
            case EEditorMessage.OBJECTS:
                messageBox.Text = EditorDashboardUI.localization.format("Objects", ControlsSettings.other, ControlsSettings.tool_2, ControlsSettings.tool_2);
                break;
            case EEditorMessage.NODES:
                messageBox.Text = EditorDashboardUI.localization.format("Nodes", ControlsSettings.tool_2);
                break;
            case EEditorMessage.VISIBILITY:
                messageBox.Text = EditorDashboardUI.localization.format("Visibility");
                break;
            }
        }
    }

    private void OnEnable()
    {
        instance = this;
    }

    internal void Editor_OnGUI()
    {
        if (window != null)
        {
            Glazier.Get().Root = window;
        }
    }

    private void Update()
    {
        if (window == null)
        {
            return;
        }
        if (EditorLevelVisibilityUI.active)
        {
            EditorLevelVisibilityUI.update();
        }
        if (InputEx.ConsumeKeyDown(KeyCode.Escape))
        {
            if (MenuConfigurationOptionsUI.active)
            {
                MenuConfigurationOptionsUI.close();
                EditorPauseUI.open();
            }
            else if (MenuConfigurationDisplayUI.active)
            {
                MenuConfigurationDisplayUI.close();
                EditorPauseUI.open();
            }
            else if (MenuConfigurationGraphicsUI.active)
            {
                MenuConfigurationGraphicsUI.close();
                EditorPauseUI.open();
            }
            else if (EditorPauseUI.audioMenu.active)
            {
                EditorPauseUI.audioMenu.close();
                EditorPauseUI.open();
            }
            else if (MenuConfigurationControlsUI.active)
            {
                MenuConfigurationControlsUI.close();
                EditorPauseUI.open();
            }
            else if (EditorPauseUI.active)
            {
                EditorPauseUI.close();
            }
            else
            {
                EditorPauseUI.open();
            }
        }
        if (window != null)
        {
            if (InputEx.GetKeyDown(ControlsSettings.screenshot))
            {
                Provider.RequestScreenshot();
            }
            if (InputEx.GetKeyDown(ControlsSettings.hud))
            {
                window.isEnabled = !window.isEnabled;
                window.drawCursorWhileDisabled = false;
            }
            InputEx.GetKeyDown(ControlsSettings.terminal);
        }
        if (InputEx.GetKeyDown(ControlsSettings.refreshAssets))
        {
            Assets.RequestReloadAllAssets();
        }
        if (EditorTerrainUI.active)
        {
            if (InputEx.ConsumeKeyDown(KeyCode.Alpha1))
            {
                dashboardUI.terrainMenu.GoToHeightsTab();
            }
            else if (InputEx.ConsumeKeyDown(KeyCode.Alpha2))
            {
                dashboardUI.terrainMenu.GoToMaterialsTab();
            }
            else if (InputEx.ConsumeKeyDown(KeyCode.Alpha3))
            {
                dashboardUI.terrainMenu.GoToFoliageTab();
            }
            else if (InputEx.ConsumeKeyDown(KeyCode.Alpha4))
            {
                dashboardUI.terrainMenu.GoToTilesTab();
            }
        }
        window.showCursor = !EditorInteract.isFlying;
        if (isMessaged)
        {
            if (Time.realtimeSinceStartup - lastMessage > MESSAGE_TIME)
            {
                isMessaged = false;
                if (!isHinted)
                {
                    messageBox.IsVisible = false;
                }
            }
        }
        else if (isHinted)
        {
            if (!lastHinted)
            {
                isHinted = false;
                messageBox.IsVisible = false;
            }
            lastHinted = false;
        }
    }

    private void Start()
    {
        window = new SleekWindow();
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        OptionsSettings.apply();
        GraphicsSettings.apply("editor loaded");
        dashboardUI = new EditorDashboardUI();
        messageBox = Glazier.Get().CreateBox();
        messageBox.PositionOffset_X = -150f;
        messageBox.PositionOffset_Y = -60f;
        messageBox.PositionScale_X = 0.5f;
        messageBox.PositionScale_Y = 1f;
        messageBox.SizeOffset_X = 300f;
        messageBox.SizeOffset_Y = 50f;
        messageBox.FontSize = ESleekFontSize.Medium;
        window.AddChild(messageBox);
        messageBox.IsVisible = false;
    }

    private void OnDestroy()
    {
        if (window != null)
        {
            dashboardUI.OnDestroy();
            if (!Provider.isApplicationQuitting)
            {
                window.InternalDestroy();
            }
            window = null;
        }
    }
}
