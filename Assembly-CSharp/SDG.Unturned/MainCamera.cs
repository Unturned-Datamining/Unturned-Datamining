using System.Collections;
using SDG.Framework.Rendering;
using UnityEngine;

namespace SDG.Unturned;

public class MainCamera : MonoBehaviour
{
    protected static Camera _instance;

    protected static bool _isAvailable;

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

    public static event MainCameraInstanceChangedHandler instanceChanged;

    public static event MainCameraAvailabilityChangedHandler availabilityChanged;

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
        instance = base.transform.GetComponent<Camera>();
        instance.eventMask = 0;
        StartCoroutine(activate());
        UnturnedPostProcess.instance.setBaseCamera(instance);
        base.gameObject.GetOrAddComponent<GLRenderer>();
    }
}
