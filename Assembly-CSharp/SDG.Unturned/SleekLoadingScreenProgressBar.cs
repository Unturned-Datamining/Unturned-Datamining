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
            foregroundImage.PositionScale_X = value;
            foregroundImage.SizeScale_X = 1f - value;
            percentageLabel.Text = value.ToString("P");
        }
    }

    public string DescriptionText
    {
        get
        {
            return label.Text;
        }
        set
        {
            label.Text = value;
        }
    }

    public SleekLoadingScreenProgressBar()
    {
        backgroundImage = Glazier.Get().CreateImage();
        backgroundImage.SizeScale_X = 1f;
        backgroundImage.SizeScale_Y = 1f;
        backgroundImage.Texture = (Texture2D)GlazierResources.PixelTexture;
        backgroundImage.TintColor = ESleekTint.FOREGROUND;
        AddChild(backgroundImage);
        foregroundImage = Glazier.Get().CreateImage();
        foregroundImage.SizeScale_X = 1f;
        foregroundImage.SizeScale_Y = 1f;
        foregroundImage.Texture = (Texture2D)GlazierResources.PixelTexture;
        foregroundImage.TintColor = new Color(0f, 0f, 0f, 0.75f);
        backgroundImage.AddChild(foregroundImage);
        label = Glazier.Get().CreateLabel();
        label.PositionOffset_X = 10f;
        label.PositionOffset_Y = -15f;
        label.PositionScale_Y = 0.5f;
        label.SizeOffset_X = -20f;
        label.SizeOffset_Y = 30f;
        label.SizeScale_X = 1f;
        label.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(label);
        percentageLabel = Glazier.Get().CreateLabel();
        percentageLabel.PositionOffset_X = -100f;
        percentageLabel.PositionOffset_Y = -15f;
        percentageLabel.PositionScale_X = 1f;
        percentageLabel.PositionScale_Y = 0.5f;
        percentageLabel.SizeOffset_X = 100f;
        percentageLabel.SizeOffset_Y = 30f;
        percentageLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        AddChild(percentageLabel);
    }
}
