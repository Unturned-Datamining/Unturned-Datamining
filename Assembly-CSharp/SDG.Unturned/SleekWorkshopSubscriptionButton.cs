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

    public override bool UseManualLayout
    {
        set
        {
            base.UseManualLayout = value;
            button.UseManualLayout = value;
            button.UseChildAutoLayout = ((!value) ? ESleekChildLayout.Horizontal : ESleekChildLayout.None);
            button.ExpandChildren = !value;
        }
    }

    public void synchronizeText()
    {
        bool subscribed = Provider.provider.workshopService.getSubscribed(fileId.m_PublishedFileId);
        button.Text = (subscribed ? unsubscribeText : subscribeText);
        button.TooltipText = (subscribed ? unsubscribeTooltip : subscribeTooltip);
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
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.TextAlignment = TextAnchor.MiddleCenter;
        button.OnClicked += handleClickedButton;
        AddChild(button);
    }
}
