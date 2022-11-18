using Steamworks;

namespace SDG.Unturned;

public class CommandWeather : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (string.Equals(parameter, "0"))
        {
            LightingManager.ResetScheduledWeather();
            CommandWindow.Log(localization.format("WeatherText", "null"));
            return;
        }
        if (AssetReference<WeatherAssetBase>.TryParse(parameter, out var result))
        {
            WeatherAssetBase weatherAssetBase = result.Find();
            if (weatherAssetBase != null)
            {
                if (!LightingManager.ForecastWeatherImmediately(weatherAssetBase))
                {
                    LightingManager.ActivatePerpetualWeather(weatherAssetBase);
                }
                CommandWindow.Log(localization.format("WeatherText", weatherAssetBase.name));
                return;
            }
        }
        string text = parameter.ToLower();
        if (text == localization.format("WeatherNone").ToLower())
        {
            LightingManager.ResetScheduledWeather();
        }
        else if (text == localization.format("WeatherDisable").ToLower())
        {
            LightingManager.DisableWeather();
        }
        else if (text == localization.format("WeatherStorm").ToLower())
        {
            WeatherAssetBase weatherAssetBase2 = WeatherAssetBase.DEFAULT_RAIN.Find();
            if (weatherAssetBase2 != null)
            {
                if (LightingManager.IsWeatherActive(weatherAssetBase2))
                {
                    LightingManager.ResetScheduledWeather();
                }
                else
                {
                    LightingManager.ForecastWeatherImmediately(weatherAssetBase2);
                }
            }
        }
        else
        {
            if (!(text == localization.format("WeatherBlizzard").ToLower()))
            {
                CommandWindow.LogError(localization.format("NoWeatherErrorText", text));
                return;
            }
            WeatherAssetBase weatherAssetBase3 = WeatherAssetBase.DEFAULT_SNOW.Find();
            if (weatherAssetBase3 != null)
            {
                if (LightingManager.IsWeatherActive(weatherAssetBase3))
                {
                    LightingManager.ResetScheduledWeather();
                }
                else
                {
                    LightingManager.ForecastWeatherImmediately(weatherAssetBase3);
                }
            }
        }
        CommandWindow.Log(localization.format("WeatherText", text));
    }

    public CommandWeather(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("WeatherCommandText");
        _info = localization.format("WeatherInfoText");
        _help = localization.format("WeatherHelpText");
    }
}
