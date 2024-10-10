using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

[RequireComponent(typeof(CanvasScaler))]
public class UnturnedCanvasScaler : MonoBehaviour
{
    public CanvasScaler scaler;

    private void Start()
    {
        if (scaler == null)
        {
            scaler = GetComponent<CanvasScaler>();
        }
    }

    private void Update()
    {
        if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
        {
            scaler.scaleFactor = GraphicsSettings.userInterfaceScale;
        }
    }
}
