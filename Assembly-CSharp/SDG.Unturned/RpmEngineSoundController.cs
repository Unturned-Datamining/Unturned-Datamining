using UnityEngine;

namespace SDG.Unturned;

internal class RpmEngineSoundController : DefaultEngineSoundControllerBase
{
    private RpmEngineSoundConfiguration soundConfiguration;

    protected override float DefaultPitch => soundConfiguration.idlePitch;

    protected override void Start()
    {
        soundConfiguration = vehicle.asset.engineSoundConfiguration;
        base.Start();
    }

    private void Update()
    {
        VehicleAsset asset = vehicle.asset;
        float num = ((Provider.preferenceData != null) ? Provider.preferenceData.Audio.Vehicle_Engine_Volume_Multiplier : 1f);
        float t = Mathf.InverseLerp(asset.engineIdleRpm, asset.engineMaxRpm, vehicle.animatedEngineRpm);
        float pitch = Mathf.Lerp(soundConfiguration.idlePitch, soundConfiguration.maxPitch, t);
        float num2 = Mathf.Lerp(soundConfiguration.idleVolume, soundConfiguration.maxVolume, t);
        if (engineAudioSource != null)
        {
            engineAudioSource.pitch = pitch;
            engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, vehicle.isEnginePowered ? num2 : 0f, 2f * Time.deltaTime) * num;
        }
    }
}
