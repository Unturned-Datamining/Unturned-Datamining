using System;

namespace SDG.Unturned;

public class DateTimeRange
{
    public DateTime start;

    public DateTime end;

    public DateTimeRange(DateTime start, DateTime end)
    {
        this.start = start;
        this.end = end;
        if (start.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTimeRange kind should be UTC", "start");
        }
        if (end.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTimeRange kind should be UTC", "end");
        }
        if (start > end)
        {
            throw new ArgumentException("DateTimeRange start and end are out of order");
        }
    }

    public bool isWithinRange(DateTime dateTime)
    {
        if (dateTime >= start)
        {
            return dateTime <= end;
        }
        return false;
    }

    public bool isNowWithinRange()
    {
        DateTime utcNow = DateTime.UtcNow;
        if (utcNow >= start)
        {
            return utcNow <= end;
        }
        return false;
    }

    public bool isBackendNowWithinRange()
    {
        DateTime backendRealtimeDate = Provider.backendRealtimeDate;
        if (backendRealtimeDate >= start)
        {
            return backendRealtimeDate <= end;
        }
        return false;
    }
}
