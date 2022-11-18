using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class PluginButtonListener : MonoBehaviour
{
    public Button targetButton;

    protected void Start()
    {
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(onTargetButtonClicked);
        }
    }

    private void onTargetButtonClicked()
    {
        if (targetButton != null)
        {
            EffectManager.sendEffectClicked(targetButton.name);
        }
    }
}
