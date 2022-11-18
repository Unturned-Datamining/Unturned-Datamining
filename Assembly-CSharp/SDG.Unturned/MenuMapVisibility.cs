using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SDG.Unturned;

public class MenuMapVisibility : MonoBehaviour
{
    private static class HelperClass
    {
        public static CommandLineFlag clNoAdditiveMenu = new CommandLineFlag(defaultValue: false, "-NoAdditiveMenu");
    }

    public void Awake()
    {
        if ((bool)HelperClass.clNoAdditiveMenu)
        {
            UnturnedLog.info("Skipping loading of additive menu scenes");
            return;
        }
        bool flag = true;
        if (Provider.statusData != null && Provider.statusData.Menu != null && !string.IsNullOrEmpty(Provider.statusData.Menu.PromoLevel))
        {
            DateTime promoStart = Provider.statusData.Menu.PromoStart;
            DateTime promoEnd = Provider.statusData.Menu.PromoEnd;
            if (new DateTimeRange(promoStart, promoEnd).isNowWithinRange())
            {
                flag = false;
                UnturnedLog.info("Loading additive promo scene {0}", Provider.statusData.Menu.PromoLevel);
                SceneManager.LoadSceneAsync(Provider.statusData.Menu.PromoLevel, LoadSceneMode.Additive);
            }
        }
        if (flag)
        {
            UnturnedLog.info("Loading additive default menu");
            SceneManager.LoadSceneAsync("DefaultMenu", LoadSceneMode.Additive);
            if (Provider.isBackendRealtimeAvailable)
            {
                handleHolidayScenes();
            }
            else
            {
                Provider.onBackendRealtimeAvailable = (Provider.BackendRealtimeAvailableHandler)Delegate.Combine(Provider.onBackendRealtimeAvailable, new Provider.BackendRealtimeAvailableHandler(onBackendRealtimeAvailable));
            }
        }
    }

    protected void onBackendRealtimeAvailable()
    {
        Provider.onBackendRealtimeAvailable = (Provider.BackendRealtimeAvailableHandler)Delegate.Remove(Provider.onBackendRealtimeAvailable, new Provider.BackendRealtimeAvailableHandler(onBackendRealtimeAvailable));
        handleHolidayScenes();
    }

    protected void handleHolidayScenes()
    {
        switch (HolidayUtil.getActiveHoliday())
        {
        case ENPCHoliday.CHRISTMAS:
            UnturnedLog.info("Loading additive Christmas scene");
            SceneManager.LoadSceneAsync("ChristmasMenu", LoadSceneMode.Additive);
            break;
        case ENPCHoliday.HALLOWEEN:
            UnturnedLog.info("Loading additive Halloween scene");
            SceneManager.LoadSceneAsync("HalloweenMenu", LoadSceneMode.Additive);
            break;
        }
    }
}
