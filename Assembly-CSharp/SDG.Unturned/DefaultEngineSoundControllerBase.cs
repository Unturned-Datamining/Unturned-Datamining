using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Code common to <see cref="T:SDG.Unturned.DefaultEngineSoundController" /> and <see cref="T:SDG.Unturned.RpmEngineSoundController" />.
/// </summary>
internal abstract class DefaultEngineSoundControllerBase : MonoBehaviour
{
    internal InteractableVehicle vehicle;

    protected bool wasDriven;

    protected AudioSource engineAudioSource;

    protected List<AudioSource> engineAdditiveAudioSources;

    protected abstract float DefaultPitch { get; }

    private void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        if (engineAudioSource != null)
        {
            engineAudioSource.maxDistance *= 2f;
        }
        Transform transform = base.transform.Find("Engine_Additive");
        if (transform != null)
        {
            AudioSource component = transform.GetComponent<AudioSource>();
            if (component != null)
            {
                component.maxDistance *= 2f;
                engineAdditiveAudioSources = new List<AudioSource>(1) { component };
            }
        }
    }

    protected virtual void Start()
    {
        vehicle.onPassengersUpdated += OnPassengersUpdated;
        vehicle.OnIsDrownedChanged += OnIsDrownedChanged;
        wasDriven = vehicle.isDriven;
        if (vehicle.trainCars != null)
        {
            for (int i = 1; i < vehicle.trainCars.Length; i++)
            {
                Transform transform = vehicle.trainCars[i].root.Find("Engine_Additive");
                if (transform != null)
                {
                    AudioSource component = transform.GetComponent<AudioSource>();
                    if (component != null)
                    {
                        engineAdditiveAudioSources.Add(component);
                    }
                }
            }
        }
        ResetPitchToDefault();
        SynchronizeEnabled();
    }

    private void OnDestroy()
    {
        vehicle.onPassengersUpdated -= OnPassengersUpdated;
        vehicle.OnIsDrownedChanged -= OnIsDrownedChanged;
    }

    private void OnPassengersUpdated()
    {
        bool isDriven = vehicle.isDriven;
        if (wasDriven != isDriven)
        {
            wasDriven = isDriven;
            if (isDriven)
            {
                ResetPitchToDefault();
            }
            SynchronizeEnabled();
        }
    }

    private void OnIsDrownedChanged()
    {
        SynchronizeEnabled();
    }

    private void ResetPitchToDefault()
    {
        if (engineAudioSource != null)
        {
            engineAudioSource.pitch = DefaultPitch;
        }
        if (engineAdditiveAudioSources == null)
        {
            return;
        }
        foreach (AudioSource engineAdditiveAudioSource in engineAdditiveAudioSources)
        {
            if (engineAdditiveAudioSource != null)
            {
                engineAdditiveAudioSource.pitch = DefaultPitch;
            }
        }
    }

    private void SynchronizeEnabled()
    {
        if (vehicle.isDriven && !vehicle.isDrowned)
        {
            if (engineAudioSource != null)
            {
                engineAudioSource.enabled = true;
            }
            if (engineAdditiveAudioSources != null)
            {
                foreach (AudioSource engineAdditiveAudioSource in engineAdditiveAudioSources)
                {
                    if (engineAdditiveAudioSource != null)
                    {
                        engineAdditiveAudioSource.enabled = true;
                    }
                }
            }
            base.enabled = true;
            return;
        }
        if (engineAudioSource != null)
        {
            engineAudioSource.volume = 0f;
            engineAudioSource.enabled = false;
        }
        if (engineAdditiveAudioSources != null)
        {
            foreach (AudioSource engineAdditiveAudioSource2 in engineAdditiveAudioSources)
            {
                if (engineAdditiveAudioSource2 != null)
                {
                    engineAdditiveAudioSource2.volume = 0f;
                    engineAdditiveAudioSource2.enabled = false;
                }
            }
        }
        base.enabled = false;
    }
}
