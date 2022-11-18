using TMPro;
using UnityEngine;

namespace SDG.Unturned;

public class TMP_PluginInputFieldListener : MonoBehaviour
{
    public TMP_InputField targetInputField;

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
