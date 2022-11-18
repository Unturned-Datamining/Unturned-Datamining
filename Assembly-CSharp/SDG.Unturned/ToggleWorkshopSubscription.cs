using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class ToggleWorkshopSubscription : MonoBehaviour
{
    public PublishedFileId_t parentFileId;

    public PublishedFileId_t[] childFileIds;

    public Button targetButton;

    public Text targetLabel;

    public string subscribeText;

    public string unsubscribeText;

    public void Refresh()
    {
        bool subscribed = Provider.provider.workshopService.getSubscribed(parentFileId.m_PublishedFileId);
        targetLabel.text = (subscribed ? unsubscribeText : subscribeText);
    }

    private void onClick()
    {
        bool flag = !Provider.provider.workshopService.getSubscribed(parentFileId.m_PublishedFileId);
        Provider.provider.workshopService.setSubscribed(parentFileId.m_PublishedFileId, flag);
        if (childFileIds != null && childFileIds.Length != 0 && flag)
        {
            PublishedFileId_t[] array = childFileIds;
            for (int i = 0; i < array.Length; i++)
            {
                PublishedFileId_t publishedFileId_t = array[i];
                Provider.provider.workshopService.setSubscribed(publishedFileId_t.m_PublishedFileId, flag);
            }
        }
        Refresh();
    }

    private void Start()
    {
        targetButton.onClick.AddListener(onClick);
    }
}
