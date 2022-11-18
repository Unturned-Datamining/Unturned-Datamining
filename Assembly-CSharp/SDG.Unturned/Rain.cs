using UnityEngine;

namespace SDG.Unturned;

public class Rain : MonoBehaviour
{
    private int _Rain_Puddle_Map = -1;

    private int _Rain_Ripple_Map = -1;

    private int _Rain_Water_Level = -1;

    private int _Rain_Intensity = -1;

    private int _Rain_Min_Height = -1;

    public Texture2D Puddle_Map;

    public Texture2D Ripple_Map;

    public float Water_Level;

    public float Intensity;

    private bool isRaining;

    private bool needsIsRainingUpdate;
}
