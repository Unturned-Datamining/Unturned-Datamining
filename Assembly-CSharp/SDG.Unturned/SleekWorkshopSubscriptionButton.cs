using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class SleekWorkshopSubscriptionButton : SleekWrapper
{
    public PublishedFileId_t fileId;

    public string subscribeText;

    public string unsubscribeText;

    public string subscribeTooltip;

    public string unsubscribeTooltip;

    private ISleekButton button;

    public void synchronizeText()
    {
        bool subscribed = Provider.provider.workshopService.getSubscribed(fileId.m_PublishedFileId);
        button.text = (subscribed ? unsubscribeText : subscribeText);
        button.tooltipText = (subscribed ? unsubscribeTooltip : subscribeTooltip);
    }

    protected void handleClickedButton(ISleekElement thisButton)
    {
        bool subscribe = !Provider.provider.workshopService.getSubscribed(fileId.m_PublishedFileId);
        Provider.provider.workshopService.setSubscribed(fileId.m_PublishedFileId, subscribe);
        synchronizeText();
    }

    public SleekWorkshopSubscriptionButton()
    {
        button = Glazier.Get().CreateButton();
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.fontAlignment = TextAnchor.MiddleCenter;
        button.onClickedButton += handleClickedButton;
        AddChild(button);
    }
}
