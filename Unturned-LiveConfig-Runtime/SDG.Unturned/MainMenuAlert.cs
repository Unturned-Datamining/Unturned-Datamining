using System;

namespace SDG.Unturned;

public class MainMenuAlert
{
    public long id;

    public string header;

    public string body;

    public string color;

    public string link;

    public string iconName;

    public string iconURL;

    public bool shouldTintIcon;

    public bool useTimeWindow;

    public DateTime startTime;

    public DateTime endTime;

    public void Parse(DatDictionary data)
    {
        id = data.ParseInt64("Id", 0L);
        header = data.GetString("Header");
        body = data.GetString("Body");
        color = data.GetString("Color");
        link = data.GetString("Link");
        iconName = data.GetString("IconName");
        iconURL = data.GetString("IconURL");
        shouldTintIcon = data.ParseBool("TintIcon");
        useTimeWindow = data.ParseBool("UseTimeWindow");
        startTime = data.ParseDateTimeUtc("StartTime");
        endTime = data.ParseDateTimeUtc("EndTime");
    }
}
