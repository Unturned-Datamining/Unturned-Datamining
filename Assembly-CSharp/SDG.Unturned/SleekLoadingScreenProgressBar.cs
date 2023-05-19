using UnityEngine;

namespace SDG.Unturned;

public class SleekLoadingScreenProgressBar : SleekWrapper
{
    private float _progressPercentage;

    private ISleekImage backgroundImage;

    private ISleekImage foregroundImage;

    private ISleekLabel label;

    private ISleekLabel percentageLabel;

    public float ProgressPercentage
    {
        get
        {
            return _progressPercentage;
        }
        set
        {
            _progressPercentage = value;
            foregroundImage.positionScale_X = value;
            foregroundImage.sizeScale_X = 1f - value;
            percentageLabel.text = value.ToString("P");
        }
    }

    public string DescriptionText
    {
        get
        {
            return label.text;
        }
        set
        {
            label.text = value;
        }
    }

    public SleekLoadingScreenProgressBar()
    {
        backgroundImage = Glazier.Get().CreateImage();
        backgroundImage.sizeScale_X = 1f;
        backgroundImage.sizeScale_Y = 1f;
        backgroundImage.texture = (Texture2D)GlazierResources.PixelTexture;
        backgroundImage.color = ESleekTint.FOREGROUND;
        AddChild(backgroundImage);
        foregroundImage = Glazier.Get().CreateImage();
        foregroundImage.sizeScale_X = 1f;
        foregroundImage.sizeScale_Y = 1f;
        foregroundImage.texture = (Texture2D)GlazierResources.PixelTexture;
        foregroundImage.color = new Color(0f, 0f, 0f, 0.75f);
        backgroundImage.AddChild(foregroundImage);
        label = Glazier.Get().CreateLabel();
        label.positionOffset_X = 10;
        label.positionOffset_Y = -15;
        label.positionScale_Y = 0.5f;
        label.sizeOffset_X = -20;
        label.sizeOffset_Y = 30;
        label.sizeScale_X = 1f;
        label.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        AddChild(label);
        percentageLabel = Glazier.Get().CreateLabel();
        percentageLabel.positionOffset_X = -100;
        percentageLabel.positionOffset_Y = -15;
        percentageLabel.positionScale_X = 1f;
        percentageLabel.positionScale_Y = 0.5f;
        percentageLabel.sizeOffset_X = 100;
        percentageLabel.sizeOffset_Y = 30;
        percentageLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        AddChild(percentageLabel);
    }
}
