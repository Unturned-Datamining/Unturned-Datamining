namespace SDG.Unturned;

/// <summary>
/// Almost every menu has a container element for its contents which spans the entire screen. This element is then
/// animated into and out of view. In the IMGUI implementation this was fine because containers off-screen were not
/// processed, but with uGUI they were still considered active. To solve the uGUI performance overhead this class
/// was introduced to disable visibility after animating out of view.
/// </summary>
public class SleekFullscreenBox : SleekWrapper
{
    public void AnimateIntoView()
    {
        base.IsVisible = true;
        AnimatePositionScale(0f, 0f, ESleekLerp.EXPONENTIAL, 20f);
    }

    public void AnimateOutOfView(float x, float y)
    {
        base.IsVisible = true;
        AnimatePositionScale(x, y, ESleekLerp.EXPONENTIAL, 20f);
    }

    public override void OnUpdate()
    {
        if (!base.IsAnimatingTransform)
        {
            float positionScale_X = base.PositionScale_X;
            float positionScale_Y = base.PositionScale_Y;
            if (positionScale_X > 0.999f || positionScale_Y > 0.999f || positionScale_X + 1f < 0.001f || positionScale_Y + 1f < 0.001f)
            {
                base.IsVisible = false;
            }
        }
    }
}
