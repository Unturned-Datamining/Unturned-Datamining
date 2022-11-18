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
                capacityBox.text = localization.format("Capacity_Text", library.amount, library.capacity);
                walletBox.text = Player.player.skills.experience.ToString();
                amountField.state = 0u;
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
                tax = (uint)Math.Ceiling((double)amountField.state * ((double)(int)library.tax / 100.0));
                net = amountField.state - tax;
                yesButton.isClickable = amountField.state <= Player.player.skills.experience && net + library.amount <= library.capacity;
            }
            else
            {
                tax = 0u;
                net = amountField.state - tax;
                yesButton.isClickable = net <= library.amount;
            }
            ESleekTint eSleekTint = (yesButton.isClickable ? ESleekTint.FONT : ESleekTint.BAD);
            amountField.textColor = eSleekTint;
            taxBox.textColor = eSleekTint;
            netBox.textColor = eSleekTint;
        }
        taxBox.text = tax.ToString();
        netBox.text = net.ToString();
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
                if (amountField.state > Player.player.skills.experience || net + library.amount > library.capacity)
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
                library.ClientTransfer((byte)transactionButton.state, amountField.state);
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
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        library = null;
        capacityBox = Glazier.Get().CreateBox();
        capacityBox.positionOffset_X = -100;
        capacityBox.positionOffset_Y = -135;
        capacityBox.positionScale_X = 0.5f;
        capacityBox.positionScale_Y = 0.5f;
        capacityBox.sizeOffset_X = 200;
        capacityBox.sizeOffset_Y = 30;
        capacityBox.addLabel(localization.format("Capacity_Label"), ESleekSide.LEFT);
        container.AddChild(capacityBox);
        walletBox = Glazier.Get().CreateBox();
        walletBox.positionOffset_X = -100;
        walletBox.positionOffset_Y = -95;
        walletBox.positionScale_X = 0.5f;
        walletBox.positionScale_Y = 0.5f;
        walletBox.sizeOffset_X = 200;
        walletBox.sizeOffset_Y = 30;
        walletBox.addLabel(localization.format("Wallet_Label"), ESleekSide.LEFT);
        container.AddChild(walletBox);
        amountField = Glazier.Get().CreateUInt32Field();
        amountField.positionOffset_X = -100;
        amountField.positionOffset_Y = -15;
        amountField.positionScale_X = 0.5f;
        amountField.positionScale_Y = 0.5f;
        amountField.sizeOffset_X = 200;
        amountField.sizeOffset_Y = 30;
        amountField.addLabel(localization.format("Amount_Label"), ESleekSide.LEFT);
        amountField.onTypedUInt32 += onTypedAmountField;
        container.AddChild(amountField);
        transactionButton = new SleekButtonState(new GUIContent(localization.format("Deposit"), localization.format("Deposit_Tooltip")), new GUIContent(localization.format("Withdraw"), localization.format("Withdraw_Tooltip")));
        transactionButton.positionOffset_X = -100;
        transactionButton.positionOffset_Y = -55;
        transactionButton.positionScale_X = 0.5f;
        transactionButton.positionScale_Y = 0.5f;
        transactionButton.sizeOffset_X = 200;
        transactionButton.sizeOffset_Y = 30;
        transactionButton.addLabel(localization.format("Transaction_Label"), ESleekSide.LEFT);
        transactionButton.onSwappedState = onSwappedTransactionState;
        container.AddChild(transactionButton);
        taxBox = Glazier.Get().CreateBox();
        taxBox.positionOffset_X = -100;
        taxBox.positionOffset_Y = 25;
        taxBox.positionScale_X = 0.5f;
        taxBox.positionScale_Y = 0.5f;
        taxBox.sizeOffset_X = 200;
        taxBox.sizeOffset_Y = 30;
        taxBox.addLabel(localization.format("Tax_Label"), ESleekSide.LEFT);
        container.AddChild(taxBox);
        netBox = Glazier.Get().CreateBox();
        netBox.positionOffset_X = -100;
        netBox.positionOffset_Y = 65;
        netBox.positionScale_X = 0.5f;
        netBox.positionScale_Y = 0.5f;
        netBox.sizeOffset_X = 200;
        netBox.sizeOffset_Y = 30;
        netBox.addLabel(localization.format("Net_Label"), ESleekSide.LEFT);
        container.AddChild(netBox);
        yesButton = Glazier.Get().CreateButton();
        yesButton.positionOffset_X = -100;
        yesButton.positionOffset_Y = 105;
        yesButton.positionScale_X = 0.5f;
        yesButton.positionScale_Y = 0.5f;
        yesButton.sizeOffset_X = 95;
        yesButton.sizeOffset_Y = 30;
        yesButton.text = localization.format("Yes_Button");
        yesButton.tooltipText = localization.format("Yes_Button_Tooltip");
        yesButton.onClickedButton += onClickedYesButton;
        container.AddChild(yesButton);
        noButton = Glazier.Get().CreateButton();
        noButton.positionOffset_X = 5;
        noButton.positionOffset_Y = 105;
        noButton.positionScale_X = 0.5f;
        noButton.positionScale_Y = 0.5f;
        noButton.sizeOffset_X = 95;
        noButton.sizeOffset_Y = 30;
        noButton.text = localization.format("No_Button");
        noButton.tooltipText = localization.format("No_Button_Tooltip");
        noButton.onClickedButton += onClickedNoButton;
        container.AddChild(noButton);
    }
}
