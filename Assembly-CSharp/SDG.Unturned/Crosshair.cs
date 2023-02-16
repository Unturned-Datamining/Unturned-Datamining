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

    private float interpolatedSpread;

    private bool isInterpolatedSpreadValid;

    public void SetGameWantsCenterDotVisible(bool isVisible)
    {
        gameWantsCenterDotVisible = isVisible;
        centerDotImage.isVisible = gameWantsCenterDotVisible && pluginAllowsCenterDotVisible;
    }

    public void SetPluginAllowsCenterDotVisible(bool isVisible)
    {
        pluginAllowsCenterDotVisible = isVisible;
        centerDotImage.isVisible = gameWantsCenterDotVisible && pluginAllowsCenterDotVisible;
    }

    public void SetDirectionalArrowsVisible(bool isVisible)
    {
        isGunCrosshairVisible = isVisible;
        crosshairLeftImage.isVisible = isVisible;
        crosshairRightImage.isVisible = isVisible;
        crosshairDownImage.isVisible = isVisible;
        crosshairUpImage.isVisible = isVisible;
        isInterpolatedSpreadValid &= isGunCrosshairVisible;
    }

    public void SynchronizeCustomColors()
    {
        Color crosshairColor = OptionsSettings.crosshairColor;
        crosshairColor.a = 0.5f;
        centerDotImage.color = crosshairColor;
        crosshairLeftImage.color = crosshairColor;
        crosshairRightImage.color = crosshairColor;
        crosshairDownImage.color = crosshairColor;
        crosshairUpImage.color = crosshairColor;
    }

    public void SynchronizeImages()
    {
        if (OptionsSettings.crosshairShape == ECrosshairShape.Classic)
        {
            crosshairLeftImage.sizeOffset_X = 8;
            crosshairLeftImage.texture = icons.load<Texture>("Crosshair_Left_Square");
            crosshairRightImage.sizeOffset_X = 8;
            crosshairRightImage.texture = icons.load<Texture>("Crosshair_Right_Square");
            crosshairUpImage.sizeOffset_Y = 8;
            crosshairUpImage.texture = icons.load<Texture>("Crosshair_Up_Square");
            crosshairDownImage.sizeOffset_Y = 8;
            crosshairDownImage.texture = icons.load<Texture>("Crosshair_Down_Square");
        }
        else
        {
            crosshairLeftImage.sizeOffset_X = 16;
            crosshairLeftImage.texture = icons.load<Texture>("Crosshair_Left");
            crosshairRightImage.sizeOffset_X = 16;
            crosshairRightImage.texture = icons.load<Texture>("Crosshair_Right");
            crosshairUpImage.sizeOffset_Y = 16;
            crosshairUpImage.texture = icons.load<Texture>("Crosshair_Up");
            crosshairDownImage.sizeOffset_Y = 16;
            crosshairDownImage.texture = icons.load<Texture>("Crosshair_Down");
        }
        crosshairLeftImage.positionOffset_X = -crosshairLeftImage.sizeOffset_X;
        crosshairUpImage.positionOffset_Y = -crosshairUpImage.sizeOffset_Y;
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
        float num = (float)Math.PI / 180f * fieldOfView * 0.5f;
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
            vector2.x += base.parent.positionScale_X;
            vector2.y += base.parent.positionScale_Y;
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
        crosshairLeftImage.positionScale_X = vector2.x - num5;
        crosshairLeftImage.positionScale_Y = vector2.y;
        crosshairRightImage.positionScale_X = vector2.x + num5;
        crosshairRightImage.positionScale_Y = vector2.y;
        crosshairUpImage.positionScale_X = vector2.x;
        crosshairUpImage.positionScale_Y = vector2.y - num6;
        crosshairDownImage.positionScale_X = vector2.x;
        crosshairDownImage.positionScale_Y = vector2.y + num6;
    }

    public Crosshair(Bundle icons)
    {
        this.icons = icons;
        centerDotImage = Glazier.Get().CreateImage();
        centerDotImage.positionOffset_X = -4;
        centerDotImage.positionOffset_Y = -4;
        centerDotImage.positionScale_X = 0.5f;
        centerDotImage.positionScale_Y = 0.5f;
        centerDotImage.sizeOffset_X = 8;
        centerDotImage.sizeOffset_Y = 8;
        centerDotImage.texture = icons.load<Texture>("Dot");
        AddChild(centerDotImage);
        gameWantsCenterDotVisible = true;
        crosshairLeftImage = Glazier.Get().CreateImage();
        crosshairLeftImage.positionOffset_Y = -4;
        crosshairLeftImage.positionScale_X = 0.5f;
        crosshairLeftImage.positionScale_Y = 0.5f;
        crosshairLeftImage.sizeOffset_Y = 8;
        AddChild(crosshairLeftImage);
        crosshairLeftImage.isVisible = false;
        crosshairRightImage = Glazier.Get().CreateImage();
        crosshairRightImage.positionOffset_Y = -4;
        crosshairRightImage.positionScale_X = 0.5f;
        crosshairRightImage.positionScale_Y = 0.5f;
        crosshairRightImage.sizeOffset_Y = 8;
        AddChild(crosshairRightImage);
        crosshairRightImage.isVisible = false;
        crosshairDownImage = Glazier.Get().CreateImage();
        crosshairDownImage.positionOffset_X = -4;
        crosshairDownImage.positionScale_X = 0.5f;
        crosshairDownImage.positionScale_Y = 0.5f;
        crosshairDownImage.sizeOffset_X = 8;
        AddChild(crosshairDownImage);
        crosshairDownImage.isVisible = false;
        crosshairUpImage = Glazier.Get().CreateImage();
        crosshairUpImage.positionOffset_X = -4;
        crosshairUpImage.positionScale_X = 0.5f;
        crosshairUpImage.positionScale_Y = 0.5f;
        crosshairUpImage.sizeOffset_X = 8;
        AddChild(crosshairUpImage);
        crosshairUpImage.isVisible = false;
        SynchronizeCustomColors();
        SynchronizeImages();
    }
}
