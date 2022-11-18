using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SDG.Unturned;

internal abstract class GlazierBase : MonoBehaviour
{
    private StringBuilder debugBuilder;

    private int fps;

    private float lastFrame;

    private int frames;

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

    protected Color debugStringColor { get; private set; }

    protected string debugString => debugBuilder.ToString();

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
                return;
            }
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
        else
        {
            debugBuilder.AppendFormat(localization.format("HUD_FPS"), fps);
        }
    }

    protected virtual void OnEnable()
    {
        debugBuilder = new StringBuilder(512);
        fps = 0;
        frames = 0;
        lastFrame = Time.realtimeSinceStartup;
    }
}
