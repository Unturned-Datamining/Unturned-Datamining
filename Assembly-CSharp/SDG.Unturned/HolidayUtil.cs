using System;

namespace SDG.Unturned;

public static class HolidayUtil
{
    private static DateTimeRange[] scheduledHolidays;

    private static CommandLineString clHolidayOverride;

    private static ENPCHoliday holidayOverride;

    public static bool isHolidayActive(ENPCHoliday holiday)
    {
        return holiday == Provider.authorityHoliday;
    }

    public static ENPCHoliday getActiveHoliday()
    {
        return Provider.authorityHoliday;
    }

    internal static bool BackendIsHolidayActive(ENPCHoliday holiday)
    {
        return holiday == BackendGetActiveHoliday();
    }

    internal static ENPCHoliday BackendGetActiveHoliday()
    {
        if (holidayOverride != 0)
        {
            return holidayOverride;
        }
        if (!Provider.isBackendRealtimeAvailable)
        {
            UnturnedLog.warn("getActiveHoliday called before backend realtime was available");
            return ENPCHoliday.NONE;
        }
        DateTime backendRealtimeDate = Provider.backendRealtimeDate;
        for (int i = 1; i < 4; i++)
        {
            DateTimeRange dateTimeRange = scheduledHolidays[i];
            if (dateTimeRange != null && dateTimeRange.isWithinRange(backendRealtimeDate))
            {
                return (ENPCHoliday)i;
            }
        }
        return ENPCHoliday.NONE;
    }

    private static void scheduleHoliday(ENPCHoliday holiday, DateTime start, DateTime end)
    {
        scheduledHolidays[(int)holiday] = new DateTimeRange(start, end);
    }

    public static void scheduleHolidays(HolidayStatusData data)
    {
        scheduleHoliday(ENPCHoliday.CHRISTMAS, data.ChristmasStart, data.ChristmasEnd);
        scheduleHoliday(ENPCHoliday.HALLOWEEN, data.HalloweenStart, data.HalloweenEnd);
        if (data.AprilFools_Start.Ticks > 0 && data.AprilFools_End.Ticks > 0)
        {
            scheduleHoliday(ENPCHoliday.APRIL_FOOLS, data.AprilFools_Start, data.AprilFools_End);
        }
    }

    static HolidayUtil()
    {
        clHolidayOverride = new CommandLineString("-Holiday");
        scheduledHolidays = new DateTimeRange[4];
        holidayOverride = ENPCHoliday.NONE;
        if (clHolidayOverride.hasValue)
        {
            string value = clHolidayOverride.value;
            if (string.Equals(value, "Halloween", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "HW", StringComparison.OrdinalIgnoreCase))
            {
                holidayOverride = ENPCHoliday.HALLOWEEN;
                return;
            }
            if (string.Equals(value, "Christmas", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "XMAS", StringComparison.OrdinalIgnoreCase))
            {
                holidayOverride = ENPCHoliday.CHRISTMAS;
                return;
            }
            if (string.Equals(value, "AprilFools", StringComparison.OrdinalIgnoreCase))
            {
                holidayOverride = ENPCHoliday.APRIL_FOOLS;
                return;
            }
            UnturnedLog.warn("Unknown holiday \"{0}\" requested by command-line override", value);
        }
    }
}
