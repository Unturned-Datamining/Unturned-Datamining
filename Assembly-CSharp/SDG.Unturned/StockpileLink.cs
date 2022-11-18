using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class StockpileLink : MonoBehaviour
{
    public Button targetButton;

    public int itemdefid;

    private void onClick()
    {
        ItemStore.Get().ViewItem(itemdefid);
    }

    private void Start()
    {
        targetButton.onClick.AddListener(onClick);
    }
}
