using UnityEngine;

namespace SDG.Unturned;

internal class SleekItemStorePriceBox : SleekWrapper
{
    private ISleekBox backdropBox;

    private ISleekLabel basePriceLabel;

    private ISleekLabel currentPriceLabel;

    private ISleekImage discountStrikethrough;

    private ISleekLabel percentageLabel;

    public void SetPrice(ulong basePrice, ulong currentPrice, int quantity)
    {
        uint num = (uint)Mathf.Max(quantity, 1);
        if (currentPrice == basePrice)
        {
            basePriceLabel.IsVisible = false;
            discountStrikethrough.IsVisible = false;
            percentageLabel.IsVisible = false;
            currentPriceLabel.PositionScale_X = 0f;
            currentPriceLabel.PositionScale_Y = 0f;
            currentPriceLabel.SizeScale_X = 1f;
            currentPriceLabel.SizeScale_Y = 1f;
            currentPriceLabel.Text = ItemStore.Get().FormatPrice(currentPrice * num);
            if (quantity > 1)
            {
                backdropBox.TooltipText = $"{ItemStore.Get().FormatPrice(currentPrice)} x {quantity} = {currentPriceLabel.Text}";
            }
            else
            {
                backdropBox.TooltipText = currentPriceLabel.Text;
            }
            return;
        }
        basePriceLabel.IsVisible = true;
        discountStrikethrough.IsVisible = true;
        percentageLabel.IsVisible = true;
        currentPriceLabel.PositionScale_X = 0.5f;
        currentPriceLabel.PositionScale_Y = 0.5f;
        currentPriceLabel.SizeScale_X = 0.5f;
        currentPriceLabel.SizeScale_Y = 0.5f;
        ulong num2 = basePrice * num;
        ulong num3 = currentPrice * num;
        basePriceLabel.Text = ItemStore.Get().FormatPrice(num2);
        currentPriceLabel.Text = ItemStore.Get().FormatPrice(num3);
        percentageLabel.Text = ItemStore.Get().FormatDiscount(num3, num2);
        if (quantity > 1)
        {
            string text = $"{ItemStore.Get().FormatPrice(basePrice)} x {quantity} = {basePriceLabel.Text}";
            string text2 = $"{ItemStore.Get().FormatPrice(currentPrice)} x {quantity} = {currentPriceLabel.Text}";
            backdropBox.TooltipText = RichTextUtil.wrapWithColor(text, Color.gray) + "\n" + RichTextUtil.wrapWithColor(percentageLabel.Text, Color.green) + "\n" + RichTextUtil.wrapWithColor(text2, ItemStore.PremiumColor);
        }
        else
        {
            backdropBox.TooltipText = RichTextUtil.wrapWithColor(basePriceLabel.Text, Color.gray) + "\n" + RichTextUtil.wrapWithColor(percentageLabel.Text, Color.green) + "\n" + RichTextUtil.wrapWithColor(currentPriceLabel.Text, ItemStore.PremiumColor);
        }
    }

    public SleekItemStorePriceBox()
    {
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.SizeScale_X = 1f;
        backdropBox.SizeScale_Y = 1f;
        backdropBox.TextColor = ItemStore.PremiumColor;
        AddChild(backdropBox);
        basePriceLabel = Glazier.Get().CreateLabel();
        basePriceLabel.PositionScale_X = 0.5f;
        basePriceLabel.SizeScale_X = 0.5f;
        basePriceLabel.SizeScale_Y = 0.5f;
        basePriceLabel.FontSize = ESleekFontSize.Medium;
        basePriceLabel.TextColor = Color.gray;
        AddChild(basePriceLabel);
        discountStrikethrough = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        discountStrikethrough.PositionScale_X = 0.5f;
        discountStrikethrough.PositionScale_Y = 0.25f;
        discountStrikethrough.PositionOffset_Y = -1f;
        discountStrikethrough.SizeOffset_Y = 1f;
        discountStrikethrough.SizeScale_X = 0.5f;
        discountStrikethrough.CanRotate = true;
        discountStrikethrough.RotationAngle = -15f;
        discountStrikethrough.TintColor = Palette.COLOR_R;
        AddChild(discountStrikethrough);
        currentPriceLabel = Glazier.Get().CreateLabel();
        currentPriceLabel.SizeScale_X = 1f;
        currentPriceLabel.FontSize = ESleekFontSize.Medium;
        currentPriceLabel.TextColor = ItemStore.PremiumColor;
        AddChild(currentPriceLabel);
        percentageLabel = Glazier.Get().CreateLabel();
        percentageLabel.SizeScale_X = 0.5f;
        percentageLabel.SizeScale_Y = 1f;
        percentageLabel.FontSize = ESleekFontSize.Medium;
        percentageLabel.TextColor = Color.green;
        AddChild(percentageLabel);
    }
}
