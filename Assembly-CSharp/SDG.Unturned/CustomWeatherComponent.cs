using UnityEngine;

namespace SDG.Unturned;

public class CustomWeatherComponent : WeatherComponentBase
{
    public WeatherAsset customAsset;

    private float staminaBuffer;

    private float healthBuffer;

    private float foodBuffer;

    private float waterBuffer;

    private float virusBuffer;

    public override void InitializeWeather()
    {
        base.InitializeWeather();
        customAsset = asset as WeatherAsset;
        overrideFog = customAsset.overrideFog;
        overrideAtmosphericFog = customAsset.overrideAtmosphericFog;
        overrideCloudColors = customAsset.overrideCloudColors;
        shadowStrengthMultiplier = customAsset.shadowStrengthMultiplier;
        fogBlendExponent = customAsset.fogBlendExponent;
        cloudBlendExponent = customAsset.cloudBlendExponent;
        windMain = customAsset.windMain;
    }

    public override void UpdateWeather()
    {
    }

    public override void UpdateLightingTime(int blendKey, int currentKey, float timeAlpha)
    {
        LightingInfo lightingInfo = LevelLighting.times[currentKey];
        LightingInfo levelValues = ((blendKey == -1) ? lightingInfo : LevelLighting.times[blendKey]);
        customAsset.getTimeValues(blendKey, currentKey, out var blendFrom, out var blendTo);
        Color a = blendFrom.fogColor.Evaluate(levelValues);
        Color b = blendTo.fogColor.Evaluate(lightingInfo);
        fogColor = Color.Lerp(a, b, timeAlpha);
        fogDensity = Mathf.Lerp(blendFrom.fogDensity, blendTo.fogDensity, timeAlpha);
        Color a2 = blendFrom.cloudColor.Evaluate(levelValues);
        Color b2 = blendTo.cloudColor.Evaluate(lightingInfo);
        cloudColor = Color.Lerp(a2, b2, timeAlpha);
        Color a3 = blendFrom.cloudRimColor.Evaluate(levelValues);
        Color b3 = blendTo.cloudRimColor.Evaluate(lightingInfo);
        cloudRimColor = Color.Lerp(a3, b3, timeAlpha);
        brightnessMultiplier = Mathf.Lerp(blendFrom.brightnessMultiplier, blendTo.brightnessMultiplier, timeAlpha);
    }

    public override void PreDestroyWeather()
    {
        base.PreDestroyWeather();
    }

    private void Update()
    {
        if (customAsset == null || !Provider.isServer)
        {
            return;
        }
        float num = Time.deltaTime * globalBlendAlpha;
        staminaBuffer += customAsset.staminaPerSecond * num;
        healthBuffer += customAsset.healthPerSecond * num;
        foodBuffer += customAsset.foodPerSecond * num;
        waterBuffer += customAsset.waterPerSecond * num;
        virusBuffer += customAsset.virusPerSecond * num;
        int num2 = MathfEx.TruncateToInt(staminaBuffer);
        if (num2 != 0)
        {
            staminaBuffer -= num2;
            foreach (Player item in EnumerateMaskedPlayers())
            {
                item.life.serverModifyStamina(num2);
            }
        }
        int num3 = MathfEx.TruncateToInt(healthBuffer);
        if (num3 != 0)
        {
            healthBuffer -= num3;
            foreach (Player item2 in EnumerateMaskedPlayers())
            {
                item2.life.serverModifyHealth(num3);
            }
        }
        int num4 = MathfEx.TruncateToInt(foodBuffer);
        if (num4 != 0)
        {
            foodBuffer -= num4;
            foreach (Player item3 in EnumerateMaskedPlayers())
            {
                item3.life.serverModifyFood(num4);
            }
        }
        int num5 = MathfEx.TruncateToInt(waterBuffer);
        if (num5 != 0)
        {
            waterBuffer -= num5;
            foreach (Player item4 in EnumerateMaskedPlayers())
            {
                item4.life.serverModifyWater(num5);
            }
        }
        int num6 = MathfEx.TruncateToInt(virusBuffer);
        if (num6 == 0)
        {
            return;
        }
        virusBuffer -= num6;
        foreach (Player item5 in EnumerateMaskedPlayers())
        {
            item5.life.serverModifyVirus(num6);
        }
    }
}
