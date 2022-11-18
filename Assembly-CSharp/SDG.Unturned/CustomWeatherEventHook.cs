using System;
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Custom Weather Event Hook")]
public class CustomWeatherEventHook : MonoBehaviour
{
    public string WeatherAssetGuid;

    public UnityEvent OnWeatherBeginTransitionIn;

    public UnityEvent OnWeatherEndTransitionIn;

    public UnityEvent OnWeatherBeginTransitionOut;

    public UnityEvent OnWeatherEndTransitionOut;

    protected Guid parsedGuid;

    protected void OnEnable()
    {
        if (Guid.TryParse(WeatherAssetGuid, out parsedGuid))
        {
            WeatherEventListenerManager.AddComponentListener(parsedGuid, this);
            return;
        }
        parsedGuid = Guid.Empty;
        UnturnedLog.warn("{0} unable to parse weather asset guid", base.transform.GetSceneHierarchyPath());
    }

    protected void OnDisable()
    {
        if (!parsedGuid.Equals(Guid.Empty))
        {
            WeatherEventListenerManager.RemoveComponentListener(parsedGuid, this);
        }
    }
}
