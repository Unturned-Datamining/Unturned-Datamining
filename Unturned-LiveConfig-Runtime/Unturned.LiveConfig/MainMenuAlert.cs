using System;

namespace Unturned.LiveConfig;

public struct MainMenuAlert
{
    public long Id;

    public string Header;

    public string Body;

    public string Color;

    public string Link;

    public string IconName;

    public string IconURL;

    public bool UseTimeWindow;

    public DateTime StartTime;

    public DateTime EndTime;
}
