using System;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerBarricadeLibraryUI
{
    private static SleekFullscreenBox container;

    private static Local localization;

    public static bool active;

    private static InteractableLibrary library;

    private static ISleekBox capacityBox;

    private static ISleekBox walletBox;

    private static ISleekUInt32Field amountField;

    private static SleekButtonState transactionButton;

    private static ISleekBox taxBox;

    private static ISleekBox netBox;

    private static uint tax;

    private static uint net;

    private static ISleekButton yesButton;

    private static ISleekButton noButton;

    public static void open(InteractableLibrary newLibrary)
    {
        if (!active)
        {
            active = true;
            library = newLibrary;
            if (library != null)
            {
                capacityBox.Text = localization.format("Capacity_Text", library.amount, library.capacity);
                walletBox.Text = Player.player.skills.experience.ToString();
                amountField.Value = 0u;
                updateTax();
            }
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            library = null;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void updateTax()
    {
        if (library != null)
        {
            if (transactionButton.state == 0)
            {
                tax = (uint)Math.Ceiling((double)amountField.Value * ((double)(int)library.tax / 100.0));
                net = amountField.Value - tax;
                yesButton.IsClickable = amountField.Value <= Player.player.skills.experience && net + library.amount <= library.capacity;
            }
            else
            {
                tax = 0u;
                net = amountField.Value - tax;
                yesButton.IsClickable = net <= library.amount;
            }
            ESleekTint eSleekTint = (yesButton.IsClickable ? ESleekTint.FONT : ESleekTint.BAD);
            amountField.TextColor = eSleekTint;
            taxBox.TextColor = eSleekTint;
            netBox.TextColor = eSleekTint;
        }
        taxBox.Text = tax.ToString();
        netBox.Text = net.ToString();
    }

    private static void onTypedAmountField(ISleekUInt32Field field, uint state)
    {
        updateTax();
    }

    private static void onSwappedTransactionState(SleekButtonState button, int index)
    {
        updateTax();
    }

    private static void onClickedYesButton(ISleekElement button)
    {
        if (library != null)
        {
            if (transactionButton.state == 0)
            {
                if (amountField.Value > Player.player.skills.experience || net + library.amount > library.capacity)
                {
                    return;
                }
            }
            else if (net > library.amount)
            {
                return;
            }
            if (net != 0)
            {
                library.ClientTransfer((byte)transactionButton.state, amountField.Value);
            }
        }
        PlayerLifeUI.open();
        close();
    }

    private static void onClickedNoButton(ISleekElement button)
    {
        PlayerLifeUI.open();
        close();
    }

    public PlayerBarricadeLibraryUI()
    {
        localization = Localization.read("/Player/PlayerBarricadeLibrary.dat");
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
        library = null;
        capacityBox = Glazier.Get().CreateBox();
        capacityBox.PositionOffset_X = -100f;
        capacityBox.PositionOffset_Y = -135f;
        capacityBox.PositionScale_X = 0.5f;
        capacityBox.PositionScale_Y = 0.5f;
        capacityBox.SizeOffset_X = 200f;
        capacityBox.SizeOffset_Y = 30f;
        capacityBox.AddLabel(localization.format("Capacity_Label"), ESleekSide.LEFT);
        container.AddChild(capacityBox);
        walletBox = Glazier.Get().CreateBox();
        walletBox.PositionOffset_X = -100f;
        walletBox.PositionOffset_Y = -95f;
        walletBox.PositionScale_X = 0.5f;
        walletBox.PositionScale_Y = 0.5f;
        walletBox.SizeOffset_X = 200f;
        walletBox.SizeOffset_Y = 30f;
        walletBox.AddLabel(localization.format("Wallet_Label"), ESleekSide.LEFT);
        container.AddChild(walletBox);
        amountField = Glazier.Get().CreateUInt32Field();
        amountField.PositionOffset_X = -100f;
        amountField.PositionOffset_Y = -15f;
        amountField.PositionScale_X = 0.5f;
        amountField.PositionScale_Y = 0.5f;
        amountField.SizeOffset_X = 200f;
        amountField.SizeOffset_Y = 30f;
        amountField.AddLabel(localization.format("Amount_Label"), ESleekSide.LEFT);
        amountField.OnValueChanged += onTypedAmountField;
        container.AddChild(amountField);
        transactionButton = new SleekButtonState(new GUIContent(localization.format("Deposit"), localization.format("Deposit_Tooltip")), new GUIContent(localization.format("Withdraw"), localization.format("Withdraw_Tooltip")));
        transactionButton.PositionOffset_X = -100f;
        transactionButton.PositionOffset_Y = -55f;
        transactionButton.PositionScale_X = 0.5f;
        transactionButton.PositionScale_Y = 0.5f;
        transactionButton.SizeOffset_X = 200f;
        transactionButton.SizeOffset_Y = 30f;
        transactionButton.AddLabel(localization.format("Transaction_Label"), ESleekSide.LEFT);
        transactionButton.onSwappedState = onSwappedTransactionState;
        container.AddChild(transactionButton);
        taxBox = Glazier.Get().CreateBox();
        taxBox.PositionOffset_X = -100f;
        taxBox.PositionOffset_Y = 25f;
        taxBox.PositionScale_X = 0.5f;
        taxBox.PositionScale_Y = 0.5f;
        taxBox.SizeOffset_X = 200f;
        taxBox.SizeOffset_Y = 30f;
        taxBox.AddLabel(localization.format("Tax_Label"), ESleekSide.LEFT);
        container.AddChild(taxBox);
        netBox = Glazier.Get().CreateBox();
        netBox.PositionOffset_X = -100f;
        netBox.PositionOffset_Y = 65f;
        netBox.PositionScale_X = 0.5f;
        netBox.PositionScale_Y = 0.5f;
        netBox.SizeOffset_X = 200f;
        netBox.SizeOffset_Y = 30f;
        netBox.AddLabel(localization.format("Net_Label"), ESleekSide.LEFT);
        container.AddChild(netBox);
        yesButton = Glazier.Get().CreateButton();
        yesButton.PositionOffset_X = -100f;
        yesButton.PositionOffset_Y = 105f;
        yesButton.PositionScale_X = 0.5f;
        yesButton.PositionScale_Y = 0.5f;
        yesButton.SizeOffset_X = 95f;
        yesButton.SizeOffset_Y = 30f;
        yesButton.Text = localization.format("Yes_Button");
        yesButton.TooltipText = localization.format("Yes_Button_Tooltip");
        yesButton.OnClicked += onClickedYesButton;
        container.AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.PositionOffset_X = 5f;
        noButton.PositionOffset_Y = 105f;
        noButton.PositionScale_X = 0.5f;
        noButton.PositionScale_Y = 0.5f;
        noButton.SizeOffset_X = 95f;
        noButton.SizeOffset_Y = 30f;
        noButton.Text = localization.format("No_Button");
        noButton.TooltipText = localization.format("No_Button_Tooltip");
        noButton.OnClicked += onClickedNoButton;
        container.AddChild(noButton);
    }
}
