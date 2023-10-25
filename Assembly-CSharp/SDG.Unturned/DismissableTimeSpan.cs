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

    /// <summary>
    /// Is current UTC time within this time span, and player has not dismissed?
    /// </summary>
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

    /// <summary>
    /// Has the current time span been dismissed?
    /// For example, player may have dismissed a previous event but not this current one.
    /// </summary>
    public bool hasDismissedSpan()
    {
        if (!getDismissedTime(out var dismissedTime))
        {
            return false;
        }
        return dismissedTime >= range.start;
    }

    /// <summary>
    /// Is current UTC time within this time span?
    /// </summary>
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
