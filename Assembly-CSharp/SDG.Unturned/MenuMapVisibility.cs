using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SDG.Unturned;

/// <summary>
/// Component in the root Menu scene.
/// Additively loads decoration levels without modifying main scene.
/// </summary>
public class MenuMapVisibility : MonoBehaviour
{
    /// <summary>
    /// Prevents static member from being initialized during MonoBehaviour construction. (Unity warning)
    /// </summary>
    private static class HelperClass
    {
        public static CommandLineString clAdditiveMenuOverride = new CommandLineString("-AdditiveMenuOverride");

        public static CommandLineFlag clNoAdditiveMenu = new CommandLineFlag(defaultValue: false, "-NoAdditiveMenu");
    }

    public void Awake()
    {
        if ((bool)HelperClass.clNoAdditiveMenu)
        {
            UnturnedLog.info("Skipping loading of additive menu scenes");
            return;
        }
        string text = null;
        if (HelperClass.clAdditiveMenuOverride.hasValue)
        {
            text = HelperClass.clAdditiveMenuOverride.value;
        }
        else if (Provider.statusData != null && Provider.statusData.Menu != null && !string.IsNullOrEmpty(Provider.statusData.Menu.PromoLevel))
        {
            DateTime promoStart = Provider.statusData.Menu.PromoStart;
            DateTime promoEnd = Provider.statusData.Menu.PromoEnd;
            if (new DateTimeRange(promoStart, promoEnd).isNowWithinRange())
            {
                text = Provider.statusData.Menu.PromoLevel;
            }
        }
        if (!string.IsNullOrEmpty(text))
        {
            UnturnedLog.info("Loading promo menu scene {0}", text);
            SceneManager.LoadSceneAsync(text, LoadSceneMode.Additive);
            return;
        }
        SceneManager.LoadSceneAsync("Menu_Base", LoadSceneMode.Additive);
        switch (HolidayUtil.getActiveHoliday())
        {
        case ENPCHoliday.CHRISTMAS:
            UnturnedLog.info("Loading additive Christmas scene");
            SceneManager.LoadSceneAsync("Menu_Christmas", LoadSceneMode.Additive);
            break;
        case ENPCHoliday.HALLOWEEN:
            UnturnedLog.info("Loading additive Halloween scene");
            SceneManager.LoadSceneAsync("Menu_Halloween", LoadSceneMode.Additive);
            break;
        case ENPCHoliday.PRIDE_MONTH:
            UnturnedLog.info("Loading additive Pride Month scene");
            SceneManager.LoadSceneAsync("Menu_PrideMonth", LoadSceneMode.Additive);
            break;
        case ENPCHoliday.UNTURNED_ANNIVERSARY:
            UnturnedLog.info("Loading additive Unturned Anniversary scene");
            SceneManager.LoadSceneAsync("Menu_UnturnedAnniversary", LoadSceneMode.Additive);
            break;
        default:
            UnturnedLog.info("Loading additive default menu");
            SceneManager.LoadSceneAsync("Menu_NoHoliday", LoadSceneMode.Additive);
            break;
        }
    }
}
