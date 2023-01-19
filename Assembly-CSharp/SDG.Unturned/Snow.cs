using UnityEngine;

namespace SDG.Unturned;

public class Snow : MonoBehaviour
{
    private int _Snow_Sparkle_Map = -1;

    public Texture2D Sparkle_Map;

    private void OnEnable()
    {
        if (!Dedicator.IsDedicatedServer && _Snow_Sparkle_Map == -1)
        {
            _Snow_Sparkle_Map = Shader.PropertyToID("_Snow_Sparkle_Map");
            Shader.SetGlobalTexture(_Snow_Sparkle_Map, Sparkle_Map);
        }
    }
}
