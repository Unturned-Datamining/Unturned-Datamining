namespace SDG.Unturned;

public class SleekFullscreenBox : SleekWrapper
{
    public void AnimateIntoView()
    {
        base.isVisible = true;
        lerpPositionScale(0f, 0f, ESleekLerp.EXPONENTIAL, 20f);
    }

    public void AnimateOutOfView(float x, float y)
    {
        base.isVisible = true;
        lerpPositionScale(x, y, ESleekLerp.EXPONENTIAL, 20f);
    }

    public override void OnUpdate()
    {
        if (!base.isAnimatingTransform)
        {
            float num = base.positionScale_X;
            float num2 = base.positionScale_Y;
            if (num > 0.999f || num2 > 0.999f || num + 1f < 0.001f || num2 + 1f < 0.001f)
            {
                base.isVisible = false;
            }
        }
    }
}
