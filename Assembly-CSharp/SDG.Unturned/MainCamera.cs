using System.Collections;
using System.Runtime.CompilerServices;
using SDG.Framework.Rendering;
using UnityEngine;

namespace SDG.Unturned;

public class MainCamera : MonoBehaviour
{
    protected static Camera _instance;

    private static Transform instanceTransform;

    /// <summary>
    /// May be out of date by one frame.
    /// </summary>
    private static Vector3 recentIshPosition;

    protected static bool _isAvailable;

    protected static bool _isPositionFrozen;

    protected static Vector3 frozenPosition;

    public static Camera instance
    {
        get
        {
            return _instance;
        }
        protected set
        {
            if (instance != value)
            {
                _instance = value;
                triggerInstanceChanged();
            }
        }
    }

    public static bool isAvailable
    {
        get
        {
            return _isAvailable;
        }
        protected set
        {
            if (isAvailable != value)
            {
                _isAvailable = value;
                triggerAvailabilityChanged();
            }
        }
    }

    public static Vector3 RenderingPosition
    {
        get
        {
            if (!_isPositionFrozen)
            {
                return instanceTransform.position;
            }
            return frozenPosition;
        }
    }

    public static bool IsPositionFrozen
    {
        get
        {
            return _isPositionFrozen;
        }
        set
        {
            _isPositionFrozen = value;
            frozenPosition = instanceTransform.position;
        }
    }

    public static event MainCameraInstanceChangedHandler instanceChanged;

    public static event MainCameraAvailabilityChangedHandler availabilityChanged;

    /// <summary>
    /// Currently used by vehicles to deactivate some rendering features when outside rendering distance.
    /// Uses "frozen" position if applicable, otherwise the camera position from the most recent Update. This means
    /// it could be out-of-date, but for LOD purposes it should be "good enough."
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float SqrDistanceFromLodPosition(Vector3 position)
    {
        if (!_isPositionFrozen)
        {
            return (recentIshPosition - position).sqrMagnitude;
        }
        return (frozenPosition - position).sqrMagnitude;
    }

    public IEnumerator activate()
    {
        yield return new WaitForEndOfFrame();
        isAvailable = true;
    }

    protected static void triggerInstanceChanged()
    {
        MainCamera.instanceChanged?.Invoke();
    }

    protected static void triggerAvailabilityChanged()
    {
        MainCamera.availabilityChanged?.Invoke();
    }

    public void Awake()
    {
        isAvailable = false;
        instanceTransform = base.transform;
        instance = base.transform.GetComponent<Camera>();
        instance.eventMask = 0;
        _isPositionFrozen = false;
        StartCoroutine(activate());
        UnturnedPostProcess.instance.setBaseCamera(instance);
        base.gameObject.GetOrAddComponent<GLRenderer>();
    }

    public void Update()
    {
        recentIshPosition = base.transform.position;
    }
}
