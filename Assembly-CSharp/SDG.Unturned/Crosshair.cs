using System;
using UnityEngine;

namespace SDG.Unturned;

public class Crosshair : SleekWrapper
{
    private bool gameWantsCenterDotVisible;

    private bool pluginAllowsCenterDotVisible;

    private bool isGunCrosshairVisible;

    private Bundle icons;

    private ISleekImage crosshairLeftImage;

    private ISleekImage crosshairRightImage;

    private ISleekImage crosshairDownImage;

    private ISleekImage crosshairUpImage;

    private ISleekImage centerDotImage;

    /// <summary>
    /// Slightly interpolated copy of actual spread angle to smooth out sharp changes like crouch/prone.
    /// </summary>
    private float interpolatedSpread;

    /// <summary>
    /// Allows interpolatedSpread to snap to target value when crosshair becomes visible.
    /// </summary>
    private bool isInterpolatedSpreadValid;

    public void SetGameWantsCenterDotVisible(bool isVisible)
    {
        gameWantsCenterDotVisible = isVisible;
        centerDotImage.IsVisible = gameWantsCenterDotVisible && pluginAllowsCenterDotVisible;
    }

    public void SetPluginAllowsCenterDotVisible(bool isVisible)
    {
        pluginAllowsCenterDotVisible = isVisible;
        centerDotImage.IsVisible = gameWantsCenterDotVisible && pluginAllowsCenterDotVisible;
    }

    public void SetDirectionalArrowsVisible(bool isVisible)
    {
        isGunCrosshairVisible = isVisible;
        crosshairLeftImage.IsVisible = isVisible;
        crosshairRightImage.IsVisible = isVisible;
        crosshairDownImage.IsVisible = isVisible;
        crosshairUpImage.IsVisible = isVisible;
        isInterpolatedSpreadValid &= isGunCrosshairVisible;
    }

    public void SynchronizeCustomColors()
    {
        Color crosshairColor = OptionsSettings.crosshairColor;
        centerDotImage.TintColor = crosshairColor;
        crosshairLeftImage.TintColor = crosshairColor;
        crosshairRightImage.TintColor = crosshairColor;
        crosshairDownImage.TintColor = crosshairColor;
        crosshairUpImage.TintColor = crosshairColor;
    }

    public void SynchronizeImages()
    {
        if (OptionsSettings.crosshairShape == ECrosshairShape.Classic)
        {
            crosshairLeftImage.SizeOffset_X = 8f;
            crosshairLeftImage.Texture = icons.load<Texture>("Crosshair_Left_Square");
            crosshairRightImage.SizeOffset_X = 8f;
            crosshairRightImage.Texture = icons.load<Texture>("Crosshair_Right_Square");
            crosshairUpImage.SizeOffset_Y = 8f;
            crosshairUpImage.Texture = icons.load<Texture>("Crosshair_Up_Square");
            crosshairDownImage.SizeOffset_Y = 8f;
            crosshairDownImage.Texture = icons.load<Texture>("Crosshair_Down_Square");
        }
        else
        {
            crosshairLeftImage.SizeOffset_X = 16f;
            crosshairLeftImage.Texture = icons.load<Texture>("Crosshair_Left");
            crosshairRightImage.SizeOffset_X = 16f;
            crosshairRightImage.Texture = icons.load<Texture>("Crosshair_Right");
            crosshairUpImage.SizeOffset_Y = 16f;
            crosshairUpImage.Texture = icons.load<Texture>("Crosshair_Up");
            crosshairDownImage.SizeOffset_Y = 16f;
            crosshairDownImage.Texture = icons.load<Texture>("Crosshair_Down");
        }
        crosshairLeftImage.PositionOffset_X = 0f - crosshairLeftImage.SizeOffset_X;
        crosshairUpImage.PositionOffset_Y = 0f - crosshairUpImage.SizeOffset_Y;
    }

    public override void OnUpdate()
    {
        if (!isGunCrosshairVisible)
        {
            isInterpolatedSpreadValid = false;
            return;
        }
        UseableGun useableGun = Player.player.equipment.useable as UseableGun;
        if (useableGun == null)
        {
            isInterpolatedSpreadValid = false;
            return;
        }
        Camera instance = MainCamera.instance;
        if (instance == null)
        {
            isInterpolatedSpreadValid = false;
            return;
        }
        float fieldOfView = instance.fieldOfView;
        float num = MathF.PI / 180f * fieldOfView * 0.5f;
        if (num < 0.001f)
        {
            isInterpolatedSpreadValid = false;
            return;
        }
        Vector2 vector2;
        if (Player.player.look.perspective == EPlayerPerspective.FIRST)
        {
            Quaternion rotation = Player.player.look.aim.rotation;
            Quaternion quaternion = Quaternion.Euler(Player.player.animator.recoilViewmodelCameraRotation.currentPosition);
            Vector3 vector = rotation * quaternion * Vector3.forward;
            Vector2 viewportPosition = instance.WorldToViewportPoint(instance.transform.position + vector);
            vector2 = ViewportToNormalizedPosition(viewportPosition);
            vector2.x += base.Parent.PositionScale_X;
            vector2.y += base.Parent.PositionScale_Y;
        }
        else
        {
            vector2 = new Vector2(0.5f, 0.5f);
        }
        float b = useableGun.CalculateSpreadAngleRadians();
        if (isInterpolatedSpreadValid)
        {
            interpolatedSpread = Mathf.Lerp(interpolatedSpread, b, Time.deltaTime * 16f);
        }
        else
        {
            interpolatedSpread = b;
            isInterpolatedSpreadValid = true;
        }
        float num2 = Mathf.Tan(num);
        float num3 = num2 * instance.aspect;
        float num4 = Mathf.Tan(interpolatedSpread);
        float num5 = num4 / num3 * 0.5f;
        float num6 = num4 / num2 * 0.5f;
        if (OptionsSettings.useStaticCrosshair)
        {
            num5 = Mathf.Lerp(0.0025f, 0.05f, OptionsSettings.staticCrosshairSize);
            num6 = num5 * instance.aspect;
        }
        crosshairLeftImage.PositionScale_X = vector2.x - num5;
        crosshairLeftImage.PositionScale_Y = vector2.y;
        crosshairRightImage.PositionScale_X = vector2.x + num5;
        crosshairRightImage.PositionScale_Y = vector2.y;
        crosshairUpImage.PositionScale_X = vector2.x;
        crosshairUpImage.PositionScale_Y = vector2.y - num6;
        crosshairDownImage.PositionScale_X = vector2.x;
        crosshairDownImage.PositionScale_Y = vector2.y + num6;
    }

    public Crosshair(Bundle icons)
    {
        this.icons = icons;
        centerDotImage = Glazier.Get().CreateImage();
        centerDotImage.PositionOffset_X = -4f;
        centerDotImage.PositionOffset_Y = -4f;
        centerDotImage.PositionScale_X = 0.5f;
        centerDotImage.PositionScale_Y = 0.5f;
        centerDotImage.SizeOffset_X = 8f;
        centerDotImage.SizeOffset_Y = 8f;
        centerDotImage.Texture = icons.load<Texture>("Dot");
        AddChild(centerDotImage);
        gameWantsCenterDotVisible = true;
        crosshairLeftImage = Glazier.Get().CreateImage();
        crosshairLeftImage.PositionOffset_Y = -4f;
        crosshairLeftImage.PositionScale_X = 0.5f;
        crosshairLeftImage.PositionScale_Y = 0.5f;
        crosshairLeftImage.SizeOffset_Y = 8f;
        AddChild(crosshairLeftImage);
        crosshairLeftImage.IsVisible = false;
        crosshairRightImage = Glazier.Get().CreateImage();
        crosshairRightImage.PositionOffset_Y = -4f;
        crosshairRightImage.PositionScale_X = 0.5f;
        crosshairRightImage.PositionScale_Y = 0.5f;
        crosshairRightImage.SizeOffset_Y = 8f;
        AddChild(crosshairRightImage);
        crosshairRightImage.IsVisible = false;
        crosshairDownImage = Glazier.Get().CreateImage();
        crosshairDownImage.PositionOffset_X = -4f;
        crosshairDownImage.PositionScale_X = 0.5f;
        crosshairDownImage.PositionScale_Y = 0.5f;
        crosshairDownImage.SizeOffset_X = 8f;
        AddChild(crosshairDownImage);
        crosshairDownImage.IsVisible = false;
        crosshairUpImage = Glazier.Get().CreateImage();
        crosshairUpImage.PositionOffset_X = -4f;
        crosshairUpImage.PositionScale_X = 0.5f;
        crosshairUpImage.PositionScale_Y = 0.5f;
        crosshairUpImage.SizeOffset_X = 8f;
        AddChild(crosshairUpImage);
        crosshairUpImage.IsVisible = false;
        SynchronizeCustomColors();
        SynchronizeImages();
    }
}
