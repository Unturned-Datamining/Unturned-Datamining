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
        neImage.texture = texture;
        neImage.color = color;
        seImage.texture = texture;
        seImage.color = color;
        swImage.texture = texture;
        swImage.color = color;
        nwImage.texture = texture;
        nwImage.color = color;
    }

    public void PlayAnimation()
    {
        float num = UnityEngine.Random.Range(-3f, 3f);
        neImage.angle = num;
        seImage.angle = 90f + num;
        swImage.angle = 180f + num;
        nwImage.angle = 270f + num;
        float f = (num - 45f) * ((float)Math.PI / 180f);
        float num2 = Mathf.Cos(f);
        float num3 = Mathf.Sin(f);
        float num4 = 0f - num3;
        float num5 = num2;
        float num6 = 0f - num5;
        float num7 = num4;
        float num8 = 0f - num7;
        float num9 = num6;
        neImage.positionScale_X = 0.5f + num2 * 0.1f;
        neImage.positionScale_Y = 0.5f + num3 * 0.1f;
        seImage.positionScale_X = 0.5f + num4 * 0.1f;
        seImage.positionScale_Y = 0.5f + num5 * 0.1f;
        swImage.positionScale_X = 0.5f + num6 * 0.1f;
        swImage.positionScale_Y = 0.5f + num7 * 0.1f;
        nwImage.positionScale_X = 0.5f + num8 * 0.1f;
        nwImage.positionScale_Y = 0.5f + num9 * 0.1f;
        neImage.positionOffset_X = -8;
        neImage.positionOffset_Y = -8;
        seImage.positionOffset_X = -8;
        seImage.positionOffset_Y = -8;
        swImage.positionOffset_X = -8;
        swImage.positionOffset_Y = -8;
        nwImage.positionOffset_X = -8;
        nwImage.positionOffset_Y = -8;
        neImage.lerpPositionScale(0.5f + num2 * 0.5f, 0.5f + num3 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
        seImage.lerpPositionScale(0.5f + num4 * 0.5f, 0.5f + num5 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
        swImage.lerpPositionScale(0.5f + num6 * 0.5f, 0.5f + num7 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
        nwImage.lerpPositionScale(0.5f + num8 * 0.5f, 0.5f + num9 * 0.5f, ESleekLerp.LINEAR, PlayerUI.HIT_TIME);
    }

    public void ApplyClassicPositions()
    {
        neImage.angle = 0f;
        seImage.angle = 90f;
        swImage.angle = 180f;
        nwImage.angle = 270f;
        neImage.positionScale_X = 0.5f;
        neImage.positionScale_Y = 0.5f;
        seImage.positionScale_X = 0.5f;
        seImage.positionScale_Y = 0.5f;
        swImage.positionScale_X = 0.5f;
        swImage.positionScale_Y = 0.5f;
        nwImage.positionScale_X = 0.5f;
        nwImage.positionScale_Y = 0.5f;
        neImage.positionOffset_X = 8;
        neImage.positionOffset_Y = -24;
        seImage.positionOffset_X = 8;
        seImage.positionOffset_Y = 8;
        swImage.positionOffset_X = -24;
        swImage.positionOffset_Y = 8;
        nwImage.positionOffset_X = -24;
        nwImage.positionOffset_Y = -24;
    }

    public SleekHitmarker()
    {
        neImage = Glazier.Get().CreateImage();
        neImage.positionOffset_X = -8;
        neImage.positionOffset_Y = -8;
        neImage.positionScale_X = 0.5f;
        neImage.positionScale_Y = 0.5f;
        neImage.sizeOffset_X = 16;
        neImage.sizeOffset_Y = 16;
        neImage.isAngled = true;
        AddChild(neImage);
        seImage = Glazier.Get().CreateImage();
        seImage.positionOffset_X = -8;
        seImage.positionOffset_Y = -8;
        seImage.positionScale_X = 0.5f;
        seImage.positionScale_Y = 0.5f;
        seImage.sizeOffset_X = 16;
        seImage.sizeOffset_Y = 16;
        seImage.angle = 90f;
        seImage.isAngled = true;
        AddChild(seImage);
        swImage = Glazier.Get().CreateImage();
        swImage.positionOffset_X = -8;
        swImage.positionOffset_Y = -8;
        swImage.positionScale_X = 0.5f;
        swImage.positionScale_Y = 0.5f;
        swImage.sizeOffset_X = 16;
        swImage.sizeOffset_Y = 16;
        swImage.angle = 180f;
        swImage.isAngled = true;
        AddChild(swImage);
        nwImage = Glazier.Get().CreateImage();
        nwImage.positionOffset_X = -8;
        nwImage.positionOffset_Y = -8;
        nwImage.positionScale_X = 0.5f;
        nwImage.positionScale_Y = 0.5f;
        nwImage.sizeOffset_X = 16;
        nwImage.sizeOffset_Y = 16;
        nwImage.angle = 270f;
        nwImage.isAngled = true;
        AddChild(nwImage);
    }
}
