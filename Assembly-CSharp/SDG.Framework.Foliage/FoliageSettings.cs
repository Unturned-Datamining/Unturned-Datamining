namespace SDG.Framework.Foliage;

public class FoliageSettings
{
    private static bool _enabled;

    private static bool _drawFocus;

    private static int _drawDistance;

    private static int _drawFocusDistance;

    private static float _instanceDensity;

    private static bool _forceInstancingOff;

    private static float _focusDistance;

    public static bool enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            _enabled = value;
        }
    }

    public static bool drawFocus
    {
        get
        {
            return _drawFocus;
        }
        set
        {
            _drawFocus = value;
        }
    }

    public static int drawDistance
    {
        get
        {
            return _drawDistance;
        }
        set
        {
            _drawDistance = value;
        }
    }

    public static int drawFocusDistance
    {
        get
        {
            return _drawFocusDistance;
        }
        set
        {
            _drawFocusDistance = value;
        }
    }

    public static float instanceDensity
    {
        get
        {
            return _instanceDensity;
        }
        set
        {
            _instanceDensity = value;
        }
    }

    public static bool forceInstancingOff
    {
        get
        {
            return _forceInstancingOff;
        }
        set
        {
            _forceInstancingOff = value;
        }
    }

    public static float focusDistance
    {
        get
        {
            return _focusDistance;
        }
        set
        {
            _focusDistance = value;
        }
    }
}
