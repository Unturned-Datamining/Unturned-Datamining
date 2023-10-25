using System;

namespace SDG.Unturned;

public class HolidayStatusData
{
    /// <summary>
    /// Inclusive start date of Halloween event.
    /// Halloween event begins as soon as UTC day matches.
    /// </summary>
    public DateTime HalloweenStart;

    /// <summary>
    /// Inclusive end date of Halloween event.
    /// Halloween event ends as soon as UTC day no longer matches.
    /// </summary>
    public DateTime HalloweenEnd;

    /// <summary>
    /// Inclusive start date of Christmas event.
    /// Christmas event begins as soon as UTC day matches.
    /// </summary>
    public DateTime ChristmasStart;

    /// <summary>
    /// Inclusive end date of Christmas event.
    /// Christmas event ends as soon as UTC day no longer matches.
    /// </summary>
    public DateTime ChristmasEnd;

    /// <summary>
    /// Inclusive start date of April Fools event.
    /// April Fools event begins as soon as UTC day matches.
    /// </summary>
    public DateTime AprilFools_Start;

    /// <summary>
    /// Inclusive end date of April Fools event.
    /// April Fools event ends as soon as UTC day no longer matches.
    /// </summary>
    public DateTime AprilFools_End;

    /// <summary>
    /// Inclusive start date of Valentine's Day event.
    /// Valentine's event begins as soon as UTC day matches.
    /// </summary>
    public DateTime ValentinesStart;

    /// <summary>
    /// Inclusive end date of Valentine's Day event.
    /// Valentine's event ends as soon as UTC day no longer matches.
    /// </summary>
    public DateTime ValentinesEnd;
}
