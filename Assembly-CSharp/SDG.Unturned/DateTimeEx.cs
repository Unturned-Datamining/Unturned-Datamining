using System;

namespace SDG.Unturned;

public static class DateTimeEx
{
    public static DateTime FromUtcUnixTimeSeconds(uint time)
    {
        return DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;
    }
}
