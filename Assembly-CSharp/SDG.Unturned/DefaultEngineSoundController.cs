using UnityEngine;

namespace SDG.Unturned;

internal class DefaultEngineSoundController : DefaultEngineSoundControllerBase
{
    protected override float DefaultPitch => vehicle.asset.pitchIdle;

    private void Update()
    {
        VehicleAsset asset = vehicle.asset;
        float deltaTime = Time.deltaTime;
        float num = ((Provider.preferenceData != null) ? Provider.preferenceData.Audio.Vehicle_Engine_Volume_Multiplier : 1f);
        float num2 = ((asset.engine != 0 && asset.engine != EEngine.BOAT) ? Mathf.Abs(vehicle.AnimatedVelocityInput) : Mathf.Abs(vehicle.AnimatedForwardVelocity));
        if (asset.engine == EEngine.HELICOPTER)
        {
            if (engineAudioSource != null)
            {
                engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, asset.pitchIdle + num2 * asset.pitchDrive, 2f * deltaTime);
                engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? (0.25f + num2 * 0.03f) : 0f, 0.125f * deltaTime) * num;
            }
            return;
        }
        if (asset.engine == EEngine.BLIMP)
        {
            if (engineAudioSource != null)
            {
                engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, asset.pitchIdle + num2 * asset.pitchDrive, 2f * deltaTime);
                engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? (0.25f + num2 * 0.1f) : 0f, 0.125f * deltaTime) * num;
            }
            return;
        }
        if (engineAudioSource != null)
        {
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, asset.pitchIdle + num2 * asset.pitchDrive, 2f * deltaTime);
            engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? 0.75f : 0f, 2f * deltaTime) * num;
        }
        if (engineAdditiveAudioSources == null)
        {
            return;
        }
        foreach (AudioSource engineAdditiveAudioSource in engineAdditiveAudioSources)
        {
            if (engineAdditiveAudioSource != null)
            {
                engineAdditiveAudioSource.pitch = Mathf.Lerp(engineAdditiveAudioSource.pitch, asset.pitchIdle + num2 * asset.pitchDrive, 2f * deltaTime);
                engineAdditiveAudioSource.volume = Mathf.Lerp(engineAdditiveAudioSource.volume, Mathf.Lerp(0f, 0.75f, num2 / 8f), 2f * deltaTime) * num;
            }
        }
    }
}
