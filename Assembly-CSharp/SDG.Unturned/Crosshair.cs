using System;
using UnityEngine;

namespace SDG.Unturned;

public class Crosshair : SleekWrapper
{
    private bool isGunCrosshairVisible;

    private ISleekImage crosshairLeftImage;

    private ISleekImage crosshairRightImage;

    private ISleekImage crosshairDownImage;

    private ISleekImage crosshairUpImage;

    private ISleekImage dotImage;

    public void SetDotVisible(bool isVisible)
    {
        dotImage.isVisible = isVisible;
    }

    public void SetDirectionalArrowsVisible(bool isVisible)
    {
        isGunCrosshairVisible = isVisible;
        crosshairLeftImage.isVisible = isVisible;
        crosshairRightImage.isVisible = isVisible;
        crosshairDownImage.isVisible = isVisible;
        crosshairUpImage.isVisible = isVisible;
    }

    public void SynchronizeCustomColors()
    {
        Color crosshairColor = OptionsSettings.crosshairColor;
        crosshairColor.a = 0.5f;
        dotImage.color = crosshairColor;
        crosshairLeftImage.color = crosshairColor;
        crosshairRightImage.color = crosshairColor;
        crosshairDownImage.color = crosshairColor;
        crosshairUpImage.color = crosshairColor;
    }

    public override void OnUpdate()
    {
        if (!isGunCrosshairVisible)
        {
            return;
        }
        UseableGun useableGun = Player.player.equipment.useable as UseableGun;
        if (useableGun == null)
        {
            return;
        }
        Camera instance = MainCamera.instance;
        if (instance == null)
        {
            return;
        }
        float fieldOfView = instance.fieldOfView;
        float num = (float)Math.PI / 180f * fieldOfView * 0.5f;
        if (!(num < 0.001f))
        {
            Vector2 vector2;
            if (Player.player.look.perspective == EPlayerPerspective.FIRST)
            {
                Quaternion rotation = Player.player.look.aim.rotation;
                Quaternion quaternion = Quaternion.Euler(Player.player.animator.recoilViewmodelCameraRotation.currentPosition);
                Vector3 vector = rotation * quaternion * Vector3.forward;
                Vector2 viewportPosition = instance.WorldToViewportPoint(instance.transform.position + vector);
                vector2 = ViewportToNormalizedPosition(viewportPosition);
            }
            else
            {
                vector2 = new Vector2(0.5f, 0.5f);
            }
            float f = useableGun.CalculateSpreadAngleRadians();
            float num2 = Mathf.Tan(num);
            float num3 = num2 * instance.aspect;
            float num4 = Mathf.Tan(f);
            float num5 = num4 / num3 * 0.5f;
            float num6 = num4 / num2 * 0.5f;
            crosshairLeftImage.positionScale_X = vector2.x - num5;
            crosshairLeftImage.positionScale_Y = vector2.y;
            crosshairRightImage.positionScale_X = vector2.x + num5;
            crosshairRightImage.positionScale_Y = vector2.y;
            crosshairUpImage.positionScale_X = vector2.x;
            crosshairUpImage.positionScale_Y = vector2.y - num6;
            crosshairDownImage.positionScale_X = vector2.x;
            crosshairDownImage.positionScale_Y = vector2.y + num6;
        }
    }

    public Crosshair(Bundle icons)
    {
        dotImage = Glazier.Get().CreateImage();
        dotImage.positionOffset_X = -4;
        dotImage.positionOffset_Y = -4;
        dotImage.positionScale_X = 0.5f;
        dotImage.positionScale_Y = 0.5f;
        dotImage.sizeOffset_X = 8;
        dotImage.sizeOffset_Y = 8;
        dotImage.texture = icons.load<Texture>("Dot");
        AddChild(dotImage);
        crosshairLeftImage = Glazier.Get().CreateImage();
        crosshairLeftImage.positionOffset_X = -16;
        crosshairLeftImage.positionOffset_Y = -4;
        crosshairLeftImage.positionScale_X = 0.5f;
        crosshairLeftImage.positionScale_Y = 0.5f;
        crosshairLeftImage.sizeOffset_X = 16;
        crosshairLeftImage.sizeOffset_Y = 8;
        crosshairLeftImage.texture = icons.load<Texture>("Crosshair_Left");
        AddChild(crosshairLeftImage);
        crosshairLeftImage.isVisible = false;
        crosshairRightImage = Glazier.Get().CreateImage();
        crosshairRightImage.positionOffset_X = 0;
        crosshairRightImage.positionOffset_Y = -4;
        crosshairRightImage.positionScale_X = 0.5f;
        crosshairRightImage.positionScale_Y = 0.5f;
        crosshairRightImage.sizeOffset_X = 16;
        crosshairRightImage.sizeOffset_Y = 8;
        crosshairRightImage.texture = icons.load<Texture>("Crosshair_Right");
        AddChild(crosshairRightImage);
        crosshairRightImage.isVisible = false;
        crosshairDownImage = Glazier.Get().CreateImage();
        crosshairDownImage.positionOffset_X = -4;
        crosshairDownImage.positionOffset_Y = 0;
        crosshairDownImage.positionScale_X = 0.5f;
        crosshairDownImage.positionScale_Y = 0.5f;
        crosshairDownImage.sizeOffset_X = 8;
        crosshairDownImage.sizeOffset_Y = 16;
        crosshairDownImage.texture = icons.load<Texture>("Crosshair_Down");
        AddChild(crosshairDownImage);
        crosshairDownImage.isVisible = false;
        crosshairUpImage = Glazier.Get().CreateImage();
        crosshairUpImage.positionOffset_X = -4;
        crosshairUpImage.positionOffset_Y = -16;
        crosshairUpImage.positionScale_X = 0.5f;
        crosshairUpImage.positionScale_Y = 0.5f;
        crosshairUpImage.sizeOffset_X = 8;
        crosshairUpImage.sizeOffset_Y = 16;
        crosshairUpImage.texture = icons.load<Texture>("Crosshair_Up");
        AddChild(crosshairUpImage);
        crosshairUpImage.isVisible = false;
        SynchronizeCustomColors();
    }
}
