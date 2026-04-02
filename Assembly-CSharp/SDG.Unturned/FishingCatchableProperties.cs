using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Items that can be caught while fishing can optionally override these properties.
/// </summary>
public class FishingCatchableProperties
{
    /// <summary>
    /// Values that would be [0.0, 1.0] floats are fixed-point integers to ensure deterministic client/server results.
    /// </summary>
    public const int FIXED_POINT_SCALE = 10000;

    /// <summary>
    /// Time values are multiplied by this to allow for fishing rod capture time multipliers.
    /// </summary>
    public const int TIME_SCALE = 10000;

    public int minChangeTargetTicks;

    private const float DEFAULT_MIN_CHANGE_TARGET_INTERVAL = 1.5f;

    public int maxChangeTargetTicks;

    private const float DEFAULT_MAX_CHANGE_TARGET_INTERVAL = 2f;

    /// <summary>
    /// Upward acceleration cannot increase beyond this limit.
    /// </summary>
    public int maxUpwardAcceleration;

    private const float DEFAULT_MAX_UPWARD_ACCELERATION = 1.5f;

    /// <summary>
    /// Downward acceleration cannot increase beyond this limit.
    /// </summary>
    public int maxDownwardAcceleration;

    private const float DEFAULT_MAX_DOWNWARD_ACCELERATION = 1.2f;

    /// <summary>
    /// Upward speed cannot increase beyond this limit.
    /// </summary>
    public int maxUpwardSpeed;

    private const float DEFAULT_MAX_UPWARD_SPEED = 0.6f;

    /// <summary>
    /// Downward speed cannot increase beyond this limit.
    /// </summary>
    public int maxDownwardSpeed;

    private const float DEFAULT_MAX_DOWNWARD_SPEED = 0.45f;

    /// <summary>
    /// How much velocity to preserve when bouncing off the top.
    /// </summary>
    public int upperRestitution;

    private const float DEFAULT_UPPER_RESTITUTION = 0.6f;

    /// <summary>
    /// How much velocity to preserve when bouncing off the bottom.
    /// </summary>
    public int lowerRestitution;

    private const float DEFAULT_LOWER_RESTITUTION = 0.4f;

    /// <summary>
    /// When choosing a new position, it will be at least this far away.
    /// </summary>
    public int minTargetDelta;

    private const float DEFAULT_MIN_TARGET_DELTA = 0.3f;

    /// <summary>
    /// When choosing a new position, it will be at most this far away.
    /// </summary>
    public int maxTargetDelta;

    private const float DEFAULT_MAX_TARGET_DELTA = 0.4f;

    /// <summary>
    /// Minimum choosable position.
    /// </summary>
    public int minTargetPosition;

    private const float DEFAULT_MIN_TARGET_POSITION = 0.1f;

    /// <summary>
    /// Maximum choosable position.
    /// </summary>
    public int maxTargetPosition;

    private const float DEFAULT_MAX_TARGET_POSITION = 0.9f;

    /// <summary>
    /// How long before item is caught.
    /// </summary>
    public int captureTicks;

    private const float DEFAULT_CAPTURE_DURATION = 2f;

    /// <summary>
    /// How long before item gets away.
    /// </summary>
    public int escapeTicks;

    private const float DEFAULT_ESCAPE_DURATION = 2f;

    public int springStiffness;

    private const float DEFAULT_SPRING_STIFFNESS = 16f;

    public int springDamping;

    private const float DEFAULT_SPRING_DAMPING = 4f;

    public static FishingCatchableProperties Default = new FishingCatchableProperties
    {
        minChangeTargetTicks = Mathf.RoundToInt(1.5f * (float)PlayerInput.TOCK_PER_SECOND),
        maxChangeTargetTicks = Mathf.RoundToInt(2f * (float)PlayerInput.TOCK_PER_SECOND),
        maxUpwardAcceleration = Mathf.RoundToInt(15000f),
        maxDownwardAcceleration = Mathf.RoundToInt(12000f),
        maxUpwardSpeed = Mathf.RoundToInt(6000f),
        maxDownwardSpeed = Mathf.RoundToInt(4500f),
        upperRestitution = Mathf.RoundToInt(6000f),
        lowerRestitution = Mathf.RoundToInt(4000f),
        minTargetDelta = Mathf.RoundToInt(3000f),
        maxTargetDelta = Mathf.RoundToInt(4000f),
        minTargetPosition = Mathf.RoundToInt(1000f),
        maxTargetPosition = Mathf.RoundToInt(9000f),
        captureTicks = Mathf.RoundToInt(2f * (float)PlayerInput.TOCK_PER_SECOND * 10000f),
        escapeTicks = Mathf.RoundToInt(2f * (float)PlayerInput.TOCK_PER_SECOND * 10000f),
        springStiffness = Mathf.RoundToInt(160000f),
        springDamping = Mathf.RoundToInt(40000f)
    };

    public void Parse(IDatDictionary data)
    {
        minChangeTargetTicks = Mathf.RoundToInt(data.ParseFloat("Min_Relocate_Interval", 1.5f) * (float)PlayerInput.TOCK_PER_SECOND);
        maxChangeTargetTicks = Mathf.RoundToInt(data.ParseFloat("Max_Relocate_Interval", 2f) * (float)PlayerInput.TOCK_PER_SECOND);
        maxUpwardAcceleration = Mathf.RoundToInt(data.ParseFloat("Max_Upward_Acceleration", 1.5f) * 10000f);
        maxDownwardAcceleration = Mathf.RoundToInt(data.ParseFloat("Max_Downward_Acceleration", 1.2f) * 10000f);
        maxUpwardSpeed = Mathf.RoundToInt(data.ParseFloat("Max_Upward_Speed", 0.6f) * 10000f);
        maxDownwardSpeed = Mathf.RoundToInt(data.ParseFloat("Max_Downward_Speed", 0.45f) * 10000f);
        upperRestitution = Mathf.RoundToInt(data.ParseFloat("Upper_Restitution", 0.6f) * 10000f);
        lowerRestitution = Mathf.RoundToInt(data.ParseFloat("Lower_Restitution", 0.4f) * 10000f);
        minTargetDelta = Mathf.RoundToInt(data.ParseFloat("Min_Target_Delta", 0.3f) * 10000f);
        maxTargetDelta = Mathf.RoundToInt(data.ParseFloat("Max_Target_Delta", 0.4f) * 10000f);
        minTargetPosition = Mathf.RoundToInt(data.ParseFloat("Min_Target_Position", 0.1f) * 10000f);
        maxTargetPosition = Mathf.RoundToInt(data.ParseFloat("Max_Target_Position", 0.9f) * 10000f);
        captureTicks = Mathf.RoundToInt(data.ParseFloat("Capture_Duration", 2f) * (float)PlayerInput.TOCK_PER_SECOND * 10000f);
        escapeTicks = Mathf.RoundToInt(data.ParseFloat("Escape_Duration", 2f) * (float)PlayerInput.TOCK_PER_SECOND * 10000f);
        springStiffness = Mathf.RoundToInt(data.ParseFloat("Spring_Stiffness", 16f) * 10000f);
        springDamping = Mathf.RoundToInt(data.ParseFloat("Spring_Damping", 4f) * 10000f);
    }

    public override string ToString()
    {
        return $"(Change interval: {minChangeTargetTicks}-{maxChangeTargetTicks}, Accel: {maxUpwardAcceleration} up {maxDownwardAcceleration} down, Max speed: {maxUpwardSpeed} up {maxDownwardSpeed} down, Restitution: {upperRestitution} upper {lowerRestitution} lower, Delta: {minTargetDelta}-{maxTargetDelta}, Position: {minTargetPosition}-{maxTargetPosition}, Capture: {captureTicks}, Escape: {escapeTicks}, Stiffness: {springStiffness}, Damping: {springDamping})";
    }
}
