using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class DismissWorkshopArticle : MonoBehaviour
{
    public ulong id;

    public Button targetButton;

    public GameObject article;

    private void onClick()
    {
        article.SetActive(value: false);
        LocalNews.dismissWorkshopItem(id);
    }

    private void Start()
    {
        targetButton.onClick.AddListener(onClick);
    }
}
