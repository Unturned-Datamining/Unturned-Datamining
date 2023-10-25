using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Extensions to the built-in Input class.
/// </summary>
public static class InputEx
{
    private static OncePerFrameGuard keyGuard;

    /// <summary>
    /// Get mouse position in viewport coordinates where zero is the bottom left and one is the top right.
    /// </summary>
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

    /// <summary>
    /// Wrapper for Input.GetKey, but returns false while typing in a uGUI text field.
    /// </summary>
    public static bool GetKey(KeyCode key)
    {
        if (Input.GetKey(key))
        {
            return Glazier.Get().ShouldGameProcessKeyDown;
        }
        return false;
    }

    /// <summary>
    /// Wrapper for Input.GetKeyDown, but returns false while typing in a uGUI text field.
    /// </summary>
    public static bool GetKeyDown(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            return Glazier.Get().ShouldGameProcessKeyDown;
        }
        return false;
    }

    /// <summary>
    /// Wrapper for Input.GetKeyUp, but returns false while typing in a uGUI text field.
    /// </summary>
    public static bool GetKeyUp(KeyCode key)
    {
        if (Input.GetKeyUp(key))
        {
            return Glazier.Get().ShouldGameProcessKeyDown;
        }
        return false;
    }

    /// <summary>
    /// Should be used anywhere that Input.GetKeyDown opens a UI.
    ///
    /// Each frame one input event can be consumed. This is a hack to prevent multiple UI-related key presses from
    /// interfering during the same frame. Only the first input event proceeds, while the others are ignored.
    /// </summary>
    /// <returns>True if caller should proceed, false otherwise.</returns>
    public static bool ConsumeKeyDown(KeyCode key)
    {
        if (GetKeyDown(key))
        {
            return keyGuard.Consume();
        }
        return false;
    }
}
