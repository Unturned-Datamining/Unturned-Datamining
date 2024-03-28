using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SDG.Unturned;

internal abstract class GlazierBase : MonoBehaviour
{
    private StringBuilder debugBuilder;

    private int fps;

    private float lastFrame;

    private int frames;

    private CommandLineFlag shouldShowTimeOverlay;

    private static CommandLineFloat clScrollViewSensitivityMultiplier = new CommandLineFloat("-ScrollViewSensitivity");

    public bool ShouldGameProcessInput
    {
        get
        {
            if (GUIUtility.hotControl == 0)
            {
                return !EventSystem.current.IsPointerOverGameObject();
            }
            return false;
        }
    }

    /// <summary>
    /// Originally this was only in the uGUI implementation, but plugins can create uGUI text fields
    /// regardless of which glazier is used.
    /// </summary>
    public virtual bool ShouldGameProcessKeyDown
    {
        get
        {
            GameObject gameObject = EventSystem.current?.currentSelectedGameObject;
            if (gameObject == null)
            {
                return true;
            }
            InputField component = gameObject.GetComponent<InputField>();
            if (component != null)
            {
                return !component.isFocused;
            }
            TMP_InputField component2 = gameObject.GetComponent<TMP_InputField>();
            if (component2 != null)
            {
                return !component2.isFocused;
            }
            return true;
        }
    }

    protected Color debugStringColor { get; private set; }

    protected string debugString => debugBuilder.ToString();

    public static float ScrollViewSensitivityMultiplier
    {
        get
        {
            if (!clScrollViewSensitivityMultiplier.hasValue)
            {
                return 1f;
            }
            return clScrollViewSensitivityMultiplier.value;
        }
    }

    protected void UpdateDebugStats()
    {
        frames++;
        if (Time.realtimeSinceStartup - lastFrame > 1f)
        {
            fps = (int)((float)frames / (Time.realtimeSinceStartup - lastFrame));
            lastFrame = Time.realtimeSinceStartup;
            frames = 0;
        }
    }

    protected void UpdateDebugString()
    {
        debugStringColor = Color.green;
        debugBuilder.Length = 0;
        Local localization = Provider.localization;
        if (Provider.isConnected)
        {
            if (!Provider.isServer && Time.realtimeSinceStartup - Provider.timeLastPacketWasReceivedFromServer > 3f)
            {
                debugStringColor = Color.red;
                int num = (int)(Time.realtimeSinceStartup - Provider.timeLastPacketWasReceivedFromServer);
                int num2 = Provider.CLIENT_TIMEOUT - num;
                debugBuilder.AppendFormat(localization.format("HUD_DC"), num, num2);
            }
            else
            {
                debugBuilder.AppendFormat(localization.format("HUD_FPS"), fps);
                debugBuilder.Append(' ');
                debugBuilder.AppendFormat(localization.format("HUD_Ping"), (int)(Provider.ping * 1000f));
                debugBuilder.Append(' ');
                debugBuilder.Append(Provider.APP_VERSION);
                if (Player.player != null && Player.player.look.canUseFreecam)
                {
                    debugBuilder.Append(' ');
                    debugBuilder.Append(Player.player.look.isOrbiting ? localization.format("HUD_Freecam_Orbiting") : "F1");
                    debugBuilder.Append(' ');
                    debugBuilder.Append(Player.player.look.isTracking ? localization.format("HUD_Freecam_Tracking") : "F2");
                    debugBuilder.Append(' ');
                    debugBuilder.Append(Player.player.look.isLocking ? localization.format("HUD_Freecam_Locking") : "F3");
                    debugBuilder.Append(' ');
                    debugBuilder.Append(Player.player.look.isFocusing ? localization.format("HUD_Freecam_Focusing") : "F4");
                    debugBuilder.Append(' ');
                    debugBuilder.Append(Player.player.look.isSmoothing ? localization.format("HUD_Freecam_Smoothing") : "F5");
                    debugBuilder.Append(' ');
                    debugBuilder.Append(Player.player.workzone.isBuilding ? localization.format("HUD_Freecam_Building") : "F6");
                    debugBuilder.Append(' ');
                    debugBuilder.Append(Player.player.look.areSpecStatsVisible ? localization.format("HUD_Freecam_Spectating") : "F7");
                }
                if (Assets.isLoading)
                {
                    debugBuilder.Append(" Assets");
                }
                if (Provider.isLoadingInventory)
                {
                    debugBuilder.Append(" Economy");
                }
                if (Provider.isLoadingUGC)
                {
                    debugBuilder.Append(" Workshop");
                }
                if (Level.isLoadingContent)
                {
                    debugBuilder.Append(" Content");
                }
                if (Level.isLoadingLighting)
                {
                    debugBuilder.Append(" Lighting");
                }
                if (Level.isLoadingVehicles)
                {
                    debugBuilder.Append(" Vehicles");
                }
                if (Level.isLoadingBarricades)
                {
                    debugBuilder.Append(" Barricades");
                }
                if (Level.isLoadingStructures)
                {
                    debugBuilder.Append(" Structures");
                }
                if (Level.isLoadingArea)
                {
                    debugBuilder.Append(" Area");
                }
                if (Player.isLoadingInventory)
                {
                    debugBuilder.Append(" Inventory");
                }
                if (Player.isLoadingLife)
                {
                    debugBuilder.Append(" Life");
                }
                if (Player.isLoadingClothing)
                {
                    debugBuilder.Append(" Clothing");
                }
            }
        }
        else
        {
            debugBuilder.AppendFormat(localization.format("HUD_FPS"), fps);
        }
        if ((bool)shouldShowTimeOverlay)
        {
            debugBuilder.AppendFormat("\n{0:N3} s", Time.realtimeSinceStartupAsDouble);
        }
    }

    protected virtual void OnEnable()
    {
        debugBuilder = new StringBuilder(512);
        fps = 0;
        frames = 0;
        lastFrame = Time.realtimeSinceStartup;
        shouldShowTimeOverlay = new CommandLineFlag(defaultValue: false, "-TimeOverlay");
    }
}
