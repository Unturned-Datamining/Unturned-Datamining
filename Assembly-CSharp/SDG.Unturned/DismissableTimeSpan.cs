using System;

namespace SDG.Unturned;

public class DismissableTimeSpan
{
    private DateTimeRange range;

    private string key;

    public DismissableTimeSpan(DateTime start, DateTime end, string key)
    {
        range = new DateTimeRange(start, end);
        this.key = key;
    }

    public bool isRelevant()
    {
        if (isNowWithinSpan())
        {
            if (hasDismissedSpan())
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public bool hasDismissedSpan()
    {
        if (!getDismissedTime(out var dismissedTime))
        {
            return false;
        }
        return dismissedTime >= range.start;
    }

    public bool isNowWithinSpan()
    {
        return range.isNowWithinRange();
    }

    public bool getDismissedTime(out DateTime dismissedTime)
    {
        return ConvenientSavedata.get().read(key, out dismissedTime);
    }

    public void dismiss()
    {
        DateTime utcNow = DateTime.UtcNow;
        ConvenientSavedata.get().write(key, utcNow);
    }
}
