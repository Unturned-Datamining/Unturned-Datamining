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
            yesButton.IsVisible = false;
            yesButton.IsClickable = true;
            noButton.PositionOffset_X = -200f;
            noButton.SizeOffset_X = 400f;
            string message = newText;
            ProfanityFilter.ApplyFilter(OptionsSettings.filter, ref message);
            message = message.Replace("<name_char>", Player.player.channel.owner.playerID.characterName);
            textBox.Text = message;
            textField.IsVisible = false;
            textBox.IsVisible = true;
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
        yesButton.IsVisible = true;
        yesButton.IsClickable = true;
        noButton.PositionOffset_X = 5f;
        noButton.SizeOffset_X = 195f;
        textField.Text = sign.DisplayText;
        textField.IsVisible = true;
        textBox.IsVisible = false;
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
            yesButton.IsClickable = sign.isTextValid(text2);
        }
        else
        {
            yesButton.IsClickable = false;
        }
    }

    private static void onClickedYesButton(ISleekElement button)
    {
        if (sign != null)
        {
            string newText = sign.trimText(textField.Text);
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
        container.PositionScale_Y = 1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        sign = null;
        textField = Glazier.Get().CreateStringField();
        textField.PositionOffset_X = -200f;
        textField.PositionScale_X = 0.5f;
        textField.PositionScale_Y = 0.1f;
        textField.SizeOffset_X = 400f;
        textField.SizeScale_Y = 0.8f;
        textField.MaxLength = 200;
        textField.IsMultiline = true;
        textField.OnTextChanged += onTypedSignText;
        container.AddChild(textField);
        textBox = Glazier.Get().CreateBox();
        textBox.PositionOffset_X = -200f;
        textBox.PositionScale_X = 0.5f;
        textBox.PositionScale_Y = 0.1f;
        textBox.SizeOffset_X = 400f;
        textBox.SizeScale_Y = 0.8f;
        textBox.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        textBox.AllowRichText = true;
        container.AddChild(textBox);
        yesButton = Glazier.Get().CreateButton();
        yesButton.PositionOffset_X = -200f;
        yesButton.PositionOffset_Y = 5f;
        yesButton.PositionScale_X = 0.5f;
        yesButton.PositionScale_Y = 0.9f;
        yesButton.SizeOffset_X = 195f;
        yesButton.SizeOffset_Y = 30f;
        yesButton.Text = local.format("Yes_Button");
        yesButton.TooltipText = local.format("Yes_Button_Tooltip");
        yesButton.OnClicked += onClickedYesButton;
        container.AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.PositionOffset_X = 5f;
        noButton.PositionOffset_Y = 5f;
        noButton.PositionScale_X = 0.5f;
        noButton.PositionScale_Y = 0.9f;
        noButton.SizeOffset_X = 195f;
        noButton.SizeOffset_Y = 30f;
        noButton.Text = local.format("No_Button");
        noButton.TooltipText = local.format("No_Button_Tooltip");
        noButton.OnClicked += onClickedNoButton;
        container.AddChild(noButton);
    }
}
