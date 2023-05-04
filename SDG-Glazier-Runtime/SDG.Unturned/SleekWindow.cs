using UnityEngine;

namespace SDG.Unturned;

public class SleekWindow : SleekWrapper
{
    public bool showCursor;

    public bool isEnabled;

    public bool drawCursorWhileDisabled;

    public bool showTooltips = true;

    private bool wasCursorLocked;

    private int cursorLockedFrameNumber;

    public bool hackSortOrder;

    public bool ShouldDrawCursor
    {
        get
        {
            if (showCursor)
            {
                if (!isEnabled)
                {
                    return drawCursorWhileDisabled;
                }
                return true;
            }
            return false;
        }
    }

    public bool ShouldDrawTooltip
    {
        get
        {
            if (showTooltips)
            {
                return ShouldDrawCursor;
            }
            return false;
        }
    }

    public bool isCursorLocked
    {
        get
        {
            if (!showCursor && wasCursorLocked)
            {
                return Time.frameCount > cursorLockedFrameNumber;
            }
            return false;
        }
    }

    public override void OnUpdate()
    {
        Cursor.visible = false;
        if (ShouldDrawCursor)
        {
            wasCursorLocked = false;
            if (Cursor.lockState != 0)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            return;
        }
        if (!wasCursorLocked)
        {
            wasCursorLocked = true;
            cursorLockedFrameNumber = Time.frameCount + 1;
        }
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public SleekWindow()
    {
        Cursor.visible = false;
        showCursor = true;
        isEnabled = true;
        drawCursorWhileDisabled = false;
        base.sizeScale_X = 1f;
        base.sizeScale_Y = 1f;
    }
}
