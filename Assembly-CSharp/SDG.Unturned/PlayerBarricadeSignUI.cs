namespace SDG.Unturned;

public class PlayerBarricadeSignUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static InteractableSign sign;

    private static ISleekField textField;

    private static ISleekBox textBox;

    private static ISleekButton yesButton;

    private static ISleekButton noButton;

    public static void open(string newText)
    {
        if (!active)
        {
            active = true;
            sign = null;
            yesButton.isVisible = false;
            yesButton.isClickable = true;
            noButton.positionOffset_X = -200;
            noButton.sizeOffset_X = 400;
            string message = newText;
            if (OptionsSettings.filter)
            {
                ProfanityFilter.filter(ref message);
            }
            message = message.Replace("<name_char>", Player.player.channel.owner.playerID.characterName);
            textBox.text = message;
            textField.isVisible = false;
            textBox.isVisible = true;
            container.AnimateIntoView();
        }
    }

    public static void open(InteractableSign newSign)
    {
        if (active)
        {
            close();
            return;
        }
        active = true;
        sign = newSign;
        yesButton.isVisible = true;
        yesButton.isClickable = true;
        noButton.positionOffset_X = 5;
        noButton.sizeOffset_X = 195;
        textField.text = sign.DisplayText;
        textField.isVisible = true;
        textBox.isVisible = false;
        container.AnimateIntoView();
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            sign = null;
            textField.ClearFocus();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void onTypedSignText(ISleekField field, string text)
    {
        if (sign != null)
        {
            string text2 = sign.trimText(text);
            yesButton.isClickable = sign.isTextValid(text2);
        }
        else
        {
            yesButton.isClickable = false;
        }
    }

    private static void onClickedYesButton(ISleekElement button)
    {
        if (sign != null)
        {
            string newText = sign.trimText(textField.text);
            sign.ClientSetText(newText);
        }
        PlayerLifeUI.open();
        close();
    }

    private static void onClickedNoButton(ISleekElement button)
    {
        PlayerLifeUI.open();
        close();
    }

    public PlayerBarricadeSignUI()
    {
        Local local = Localization.read("/Player/PlayerBarricadeSign.dat");
        container = new SleekFullscreenBox();
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        sign = null;
        textField = Glazier.Get().CreateStringField();
        textField.positionOffset_X = -200;
        textField.positionScale_X = 0.5f;
        textField.positionScale_Y = 0.1f;
        textField.sizeOffset_X = 400;
        textField.sizeScale_Y = 0.8f;
        textField.maxLength = 200;
        textField.multiline = true;
        textField.onTyped += onTypedSignText;
        container.AddChild(textField);
        textBox = Glazier.Get().CreateBox();
        textBox.positionOffset_X = -200;
        textBox.positionScale_X = 0.5f;
        textBox.positionScale_Y = 0.1f;
        textBox.sizeOffset_X = 400;
        textBox.sizeScale_Y = 0.8f;
        textBox.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        textBox.enableRichText = true;
        container.AddChild(textBox);
        yesButton = Glazier.Get().CreateButton();
        yesButton.positionOffset_X = -200;
        yesButton.positionOffset_Y = 5;
        yesButton.positionScale_X = 0.5f;
        yesButton.positionScale_Y = 0.9f;
        yesButton.sizeOffset_X = 195;
        yesButton.sizeOffset_Y = 30;
        yesButton.text = local.format("Yes_Button");
        yesButton.tooltipText = local.format("Yes_Button_Tooltip");
        yesButton.onClickedButton += onClickedYesButton;
        container.AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.positionOffset_X = 5;
        noButton.positionOffset_Y = 5;
        noButton.positionScale_X = 0.5f;
        noButton.positionScale_Y = 0.9f;
        noButton.sizeOffset_X = 195;
        noButton.sizeOffset_Y = 30;
        noButton.text = local.format("No_Button");
        noButton.tooltipText = local.format("No_Button_Tooltip");
        noButton.onClickedButton += onClickedNoButton;
        container.AddChild(noButton);
    }
}
