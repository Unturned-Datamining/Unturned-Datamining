using System;
using UnityEngine;

namespace SDG.Unturned;

public class SleekHitmarker : SleekWrapper
{
    private const int IMAGE_SIZE = 16;

    private const int HALF_IMAGE_SIZE = 8;

    private ISleekImage neImage;

    private ISleekImage seImage;

    private ISleekImage swImage;

    private ISleekImage nwImage;

    private static StaticResourceRef<Texture2D> hitEntityTexture = new StaticResourceRef<Texture2D>("Bundles/Textures/Player/Icons/PlayerLife/Hit_Entity");

    private static StaticResourceRef<Texture2D> hitBuildTexture = new StaticResourceRef<Texture2D>("Bundles/Textures/Player/Icons/PlayerLife/Hit_Build");

    private static StaticResourceRef<Texture2D> hitGhostTexture = new StaticResourceRef<Texture2D>("Bundles/Textures/Player/Icons/PlayerLife/Hit_Ghost");

    public void SetStyle(EPlayerHit hit)
    {
        Texture2D texture;
        Color color;
        switch (hit)
        {
        default:
            return;
        case EPlayerHit.ENTITIY:
            texture = hitEntityTexture;
            color = OptionsSettings.hitmarkerColor;
            break;
        case EPlayerHit.CRITICAL:
            texture = hitEntityTexture;
            color = OptionsSettings.criticalHitmarkerColor;
            break;
        case EPlayerHit.BUILD:
            texture = hitBuildTexture;
            color = OptionsSettings.hitmarkerColor;
            break;
        case EPlayerHit.GHOST:
            texture = hitGhostTexture;
            color = OptionsSettings.hitmarkerColor;
            break;
        case EPlayerHit.NONE:
            return;
        }
        neImage.Texture = texture;
        neImage.TintColor = color;
        seImage.Texture = texture;
        seImage.TintColor = color;
        swImage.Texture = texture;
        swImage.TintColor = color;
        nwImage.Texture = texture;
        nwImage.TintColor = color;
    }

    public void PlayAnimation()
    {
        float num = UnityEngine.Random.Range(-3f, 3f);
        neImage.RotationAngle = num;
        seImage.RotationAngle = 90f + num;
        swImage.RotationAngle = 180f + num;
        nwImage.RotationAngle = 270f + num;
        float f = (num - 45f) * (MathF.PI / 180f);
        float num2 = Mathf.Cos(f);
        float num3 = Mathf.Sin(f);
        float num4 = 0f - num3;
        float num5 = num2;
        float num6 = 0f - num5;
        float num7 = num4;
        float num8 = 0f - num7;
        float num9 = num6;
        neImage.PositionScale_X = 0.5f + num2 * 0.1f;
        neImage.PositionScale_Y = 0.5f + num3 * 0.1f;
        seImage.PositionScale_X = 0.5f + num4 * 0.1f;
        seImage.PositionScale_Y = 0.5f + num5 * 0.1f;
        swImage.PositionScale_X = 0.5f + num6 * 0.1f;
        swImage.PositionScale_Y = 0.5f + num7 * 0.1f;
        nwImage.PositionScale_X = 0.5f + num8 * 0.1f;
        nwImage.PositionScale_Y = 0.5f + num9 * 0.1f;
        neImage.PositionOffset_X = -8f;
        neImage.PositionOffset_Y = -8f;
        seImage.PositionOffset_X = -8f;
        seImage.PositionOffset_Y = -8f;
        swImage.PositionOffset_X = -8f;
        swImage.PositionOffset_Y = -8f;
        nwImage.PositionOffset_X = -8f;
        nwImage.PositionOffset_Y = -8f;
        neImage.AnimatePositionScale(0.5f + num2 * 0.5f, 0.5f + num3 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
        seImage.AnimatePositionScale(0.5f + num4 * 0.5f, 0.5f + num5 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
        swImage.AnimatePositionScale(0.5f + num6 * 0.5f, 0.5f + num7 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
        nwImage.AnimatePositionScale(0.5f + num8 * 0.5f, 0.5f + num9 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
    }

    public void ApplyClassicPositions()
    {
        neImage.RotationAngle = 0f;
        seImage.RotationAngle = 90f;
        swImage.RotationAngle = 180f;
        nwImage.RotationAngle = 270f;
        neImage.PositionScale_X = 0.5f;
        neImage.PositionScale_Y = 0.5f;
        seImage.PositionScale_X = 0.5f;
        seImage.PositionScale_Y = 0.5f;
        swImage.PositionScale_X = 0.5f;
        swImage.PositionScale_Y = 0.5f;
        nwImage.PositionScale_X = 0.5f;
        nwImage.PositionScale_Y = 0.5f;
        neImage.PositionOffset_X = 8f;
        neImage.PositionOffset_Y = -24f;
        seImage.PositionOffset_X = 8f;
        seImage.PositionOffset_Y = 8f;
        swImage.PositionOffset_X = -24f;
        swImage.PositionOffset_Y = 8f;
        nwImage.PositionOffset_X = -24f;
        nwImage.PositionOffset_Y = -24f;
    }

    public SleekHitmarker()
    {
        neImage = Glazier.Get().CreateImage();
        neImage.PositionOffset_X = -8f;
        neImage.PositionOffset_Y = -8f;
        neImage.PositionScale_X = 0.5f;
        neImage.PositionScale_Y = 0.5f;
        neImage.SizeOffset_X = 16f;
        neImage.SizeOffset_Y = 16f;
        neImage.CanRotate = true;
        AddChild(neImage);
        seImage = Glazier.Get().CreateImage();
        seImage.PositionOffset_X = -8f;
        seImage.PositionOffset_Y = -8f;
        seImage.PositionScale_X = 0.5f;
        seImage.PositionScale_Y = 0.5f;
        seImage.SizeOffset_X = 16f;
        seImage.SizeOffset_Y = 16f;
        seImage.RotationAngle = 90f;
        seImage.CanRotate = true;
        AddChild(seImage);
        swImage = Glazier.Get().CreateImage();
        swImage.PositionOffset_X = -8f;
        swImage.PositionOffset_Y = -8f;
        swImage.PositionScale_X = 0.5f;
        swImage.PositionScale_Y = 0.5f;
        swImage.SizeOffset_X = 16f;
        swImage.SizeOffset_Y = 16f;
        swImage.RotationAngle = 180f;
        swImage.CanRotate = true;
        AddChild(swImage);
        nwImage = Glazier.Get().CreateImage();
        nwImage.PositionOffset_X = -8f;
        nwImage.PositionOffset_Y = -8f;
        nwImage.PositionScale_X = 0.5f;
        nwImage.PositionScale_Y = 0.5f;
        nwImage.SizeOffset_X = 16f;
        nwImage.SizeOffset_Y = 16f;
        nwImage.RotationAngle = 270f;
        nwImage.CanRotate = true;
        AddChild(nwImage);
    }
}
