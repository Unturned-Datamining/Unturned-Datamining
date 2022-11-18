using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class ReadMore : MonoBehaviour
{
    public Button targetButton;

    public GameObject targetContent;

    public string onText;

    public string offText;

    public void Refresh()
    {
        GetComponent<Text>().text = (targetContent.activeSelf ? offText : onText);
    }

    private void onClick()
    {
        targetContent.SetActive(!targetContent.activeSelf);
        Refresh();
    }

    private void Start()
    {
        targetButton.onClick.AddListener(onClick);
    }
}
