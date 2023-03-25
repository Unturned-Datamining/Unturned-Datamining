using System;

namespace SDG.Unturned;

public static class DateTimeEx
{
    public static DateTime FromUtcUnixTimeSeconds(uint time)
    {
        return DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;
    }

    public static DateTime FromUtcUnixTimeSeconds(long time)
    {
        return DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;
    }

    public static long ToUnixTimeSeconds(this DateTime time)
    {
        return new DateTimeOffset(time).ToUnixTimeSeconds();
    }
}
