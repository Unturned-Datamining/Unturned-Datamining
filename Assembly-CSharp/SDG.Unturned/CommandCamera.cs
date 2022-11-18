using Steamworks;

namespace SDG.Unturned;

public class CommandCamera : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        string text = parameter.ToLower();
        ECameraMode cameraMode;
        if (text == localization.format("CameraFirst").ToLower())
        {
            cameraMode = ECameraMode.FIRST;
        }
        else if (text == localization.format("CameraThird").ToLower())
        {
            cameraMode = ECameraMode.THIRD;
        }
        else if (text == localization.format("CameraBoth").ToLower())
        {
            cameraMode = ECameraMode.BOTH;
        }
        else
        {
            if (!(text == localization.format("CameraVehicle").ToLower()))
            {
                CommandWindow.LogError(localization.format("NoCameraErrorText", text));
                return;
            }
            cameraMode = ECameraMode.VEHICLE;
        }
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        Provider.cameraMode = cameraMode;
        CommandWindow.Log(localization.format("CameraText", text));
    }

    public CommandCamera(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("CameraCommandText");
        _info = localization.format("CameraInfoText");
        _help = localization.format("CameraHelpText");
    }
}
