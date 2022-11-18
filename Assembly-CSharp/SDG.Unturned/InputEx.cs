using UnityEngine;

namespace SDG.Unturned;

public static class InputEx
{
    private static OncePerFrameGuard keyGuard;

    public static Vector2 NormalizedMousePosition
    {
        get
        {
            Vector2 result = Input.mousePosition;
            result.x /= Screen.width;
            result.y /= Screen.height;
            return result;
        }
    }

    public static bool GetKey(KeyCode key)
    {
        if (Input.GetKey(key))
        {
            return Glazier.Get().ShouldGameProcessKeyDown;
        }
        return false;
    }

    public static bool GetKeyDown(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            return Glazier.Get().ShouldGameProcessKeyDown;
        }
        return false;
    }

    public static bool GetKeyUp(KeyCode key)
    {
        if (Input.GetKeyUp(key))
        {
            return Glazier.Get().ShouldGameProcessKeyDown;
        }
        return false;
    }

    public static bool ConsumeKeyDown(KeyCode key)
    {
        if (GetKeyDown(key))
        {
            return keyGuard.Consume();
        }
        return false;
    }
}
