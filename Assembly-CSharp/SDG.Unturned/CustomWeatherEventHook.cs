using System;
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

/// <summary>
/// Can be added to any GameObject to receive weather events for a specific custom weather asset.
/// </summary>
[AddComponentMenu("Unturned/Custom Weather Event Hook")]
public class CustomWeatherEventHook : MonoBehaviour
{
    /// <summary>
    /// GUID of custom weather asset to listen for.
    /// </summary>
    public string WeatherAssetGuid;

    /// <summary>
    /// Invoked when custom weather is activated, or immediately if weather is fading in when registered.
    /// </summary>
    public UnityEvent OnWeatherBeginTransitionIn;

    /// <summary>
    /// Invoked when custom weather finishes fading in, or immediately if weather is already fully active when registered.
    /// </summary>
    public UnityEvent OnWeatherEndTransitionIn;

    /// <summary>
    /// Invoked when custom weather is deactivated and begins fading out.
    /// </summary>
    public UnityEvent OnWeatherBeginTransitionOut;

    /// <summary>
    /// Invoked when custom weather finishes fading out and is destroyed.
    /// </summary>
    public UnityEvent OnWeatherEndTransitionOut;

    /// <summary>
    /// GUID parsed from WeatherAssetGuid parameter.
    /// </summary>
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
