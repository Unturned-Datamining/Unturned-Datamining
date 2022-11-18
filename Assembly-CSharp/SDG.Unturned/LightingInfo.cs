using UnityEngine;

namespace SDG.Unturned;

public class LightingInfo
{
    private Color[] _colors;

    private float[] _singles;

    public Color[] colors => _colors;

    public float[] singles => _singles;

    public LightingInfo(Color[] newColors, float[] newSingles)
    {
        _colors = newColors;
        _singles = newSingles;
    }
}
