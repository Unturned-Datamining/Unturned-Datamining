using UnityEngine;

namespace SDG.Unturned;

public class GraphicsSettingsResolution
{
    public int Width { get; set; }

    public int Height { get; set; }

    public GraphicsSettingsResolution(Resolution resolution)
    {
        Width = resolution.width;
        Height = resolution.height;
    }

    public GraphicsSettingsResolution()
    {
        Width = 0;
        Height = 0;
    }
}
