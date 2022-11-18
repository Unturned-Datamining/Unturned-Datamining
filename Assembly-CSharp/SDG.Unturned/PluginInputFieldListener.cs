using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

public class PluginInputFieldListener : MonoBehaviour
{
    public InputField targetInputField;

    protected void Start()
    {
        if (targetInputField != null)
        {
            targetInputField.onEndEdit.AddListener(onEndEdit);
        }
    }

    private void onEndEdit(string text)
    {
        if (targetInputField != null)
        {
            EffectManager.sendEffectTextCommitted(targetInputField.name, text);
        }
    }
}
