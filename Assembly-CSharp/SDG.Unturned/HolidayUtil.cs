using System;
using System.Globalization;

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

    internal static ENPCHoliday GetScheduledHoliday()
    {
        if (holidayOverride != 0)
        {
            return holidayOverride;
        }
        DateTime utcNow = DateTime.UtcNow;
        for (int i = 1; i < 7; i++)
        {
            DateTimeRange dateTimeRange = scheduledHolidays[i];
            if (dateTimeRange != null && dateTimeRange.isWithinRange(utcNow))
            {
                return (ENPCHoliday)i;
            }
        }
        return ENPCHoliday.NONE;
    }

    private static void scheduleHoliday(ENPCHoliday holiday, DateTime start, DateTime end)
    {
        DateTime dateTime = start.ToUniversalTime();
        DateTime dateTime2 = end.ToUniversalTime();
        UnturnedLog.info($"Scheduled {holiday} from {start} to {end} local time ({dateTime} to {dateTime2} UTC)");
        scheduledHolidays[(int)holiday] = new DateTimeRange(dateTime, dateTime2);
    }

    public static void scheduleHolidays(HolidayStatusData data)
    {
        DateTime now = DateTime.Now;
        int year = now.Year;
        int num = ((now.Month > 6) ? year : (year - 1));
        scheduleHoliday(ENPCHoliday.CHRISTMAS, new DateTime(num, 12, 7, 0, 0, 0, DateTimeKind.Local), new DateTime(num + 1, 1, 2, 12, 0, 0, DateTimeKind.Local));
        scheduleHoliday(ENPCHoliday.HALLOWEEN, new DateTime(year, 10, 20, 0, 0, 0, DateTimeKind.Local), new DateTime(year, 11, 1, 12, 0, 0, DateTimeKind.Local));
        scheduleHoliday(ENPCHoliday.VALENTINES, new DateTime(year, 2, 14, 0, 0, 0, DateTimeKind.Local), new DateTime(year, 2, 14, 23, 59, 59, DateTimeKind.Local));
        scheduleHoliday(ENPCHoliday.APRIL_FOOLS, new DateTime(year, 4, 1, 0, 0, 0, DateTimeKind.Local), new DateTime(year, 4, 1, 23, 59, 59, DateTimeKind.Local));
        scheduleHoliday(ENPCHoliday.PRIDE_MONTH, new DateTime(year, 6, 1, 0, 0, 0, DateTimeKind.Local), new DateTime(year, 6, 30, 23, 59, 59, DateTimeKind.Local));
        if (data.LunarNewYear_StartOverride.Ticks > 0 && data.LunarNewYear_EndOverride.Ticks > 0)
        {
            scheduleHoliday(ENPCHoliday.PRIDE_MONTH, data.LunarNewYear_StartOverride, data.LunarNewYear_EndOverride);
            return;
        }
        DateTime dateTime = new ChineseLunisolarCalendar().ToDateTime(year, 1, 1, 0, 0, 0, 0);
        DateTime dateTime2 = dateTime.AddDays(-1.0);
        DateTime dateTime3 = dateTime.AddDays(data.LunarNewYear_Days);
        scheduleHoliday(ENPCHoliday.LUNAR_NEW_YEAR, new DateTime(year, dateTime2.Month, dateTime2.Day, 0, 0, 0, DateTimeKind.Local), new DateTime(year, dateTime3.Month, dateTime3.Day, 23, 59, 59, DateTimeKind.Local));
    }

    static HolidayUtil()
    {
        clHolidayOverride = new CommandLineString("-Holiday");
        scheduledHolidays = new DateTimeRange[7];
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
            if (string.Equals(value, "Valentines", StringComparison.OrdinalIgnoreCase))
            {
                holidayOverride = ENPCHoliday.VALENTINES;
                return;
            }
            if (string.Equals(value, "PrideMonth", StringComparison.OrdinalIgnoreCase))
            {
                holidayOverride = ENPCHoliday.PRIDE_MONTH;
                return;
            }
            if (string.Equals(value, "LunarNewYear", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "LNY", StringComparison.OrdinalIgnoreCase))
            {
                holidayOverride = ENPCHoliday.LUNAR_NEW_YEAR;
                return;
            }
            UnturnedLog.warn("Unknown holiday \"{0}\" requested by command-line override", value);
        }
    }
}
