using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuOverridableObjects : MonoBehaviour
{
    [Tooltip("Point of view when menu first loads. Blends into Title Camera.")]
    public Camera initialCamera;

    [Tooltip("Point of view with the game name and news feed.")]
    public Camera titleCamera;

    public Camera playCamera;

    public Camera survivorsCamera;

    public Camera optionsCamera;

    public Camera workshopCamera;

    public Transform playerCharacterTransform;

    private static MenuOverridableObjects destinationInstance;

    internal static event Action<MenuOverridableObjects> OnMenuOverridesApplied;

    private void Awake()
    {
        if ((bool)GetComponent<MenuStartup>())
        {
            destinationInstance = this;
        }
        else if (destinationInstance != null)
        {
            ApplyMenuOverrides(this, destinationInstance);
            MenuOverridableObjects.OnMenuOverridesApplied?.Invoke(this);
        }
        else
        {
            UnturnedLog.warn("MenuOverridableObjects without destination");
        }
    }

    private void OnDestroy()
    {
        if (destinationInstance == this)
        {
            destinationInstance = null;
        }
    }

    private void ApplyMenuOverrides(MenuOverridableObjects source, MenuOverridableObjects destination)
    {
        ApplyOverride(source.initialCamera, destination.initialCamera);
        ApplyOverride(source.titleCamera, destination.titleCamera);
        ApplyOverride(source.playCamera, destination.playCamera);
        ApplyOverride(source.survivorsCamera, destination.survivorsCamera);
        ApplyOverride(source.optionsCamera, destination.optionsCamera);
        ApplyOverride(source.workshopCamera, destination.workshopCamera);
        ApplyOverride(source.playerCharacterTransform, destination.playerCharacterTransform);
    }

    private void ApplyOverride(Camera sourceCamera, Camera destinationCamera)
    {
        sourceCamera.enabled = false;
        ApplyOverride(sourceCamera.transform, destinationCamera.transform);
    }

    private void ApplyOverride(Transform sourceTransform, Transform destinationTransform)
    {
        sourceTransform.gameObject.SetActive(value: false);
        sourceTransform.GetPositionAndRotation(out var position, out var rotation);
        destinationTransform.SetPositionAndRotation(position, rotation);
    }
}
