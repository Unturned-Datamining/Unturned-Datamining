using System;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Framework.Devkit;

[Serializable]
internal struct AmbianceVolumeTimeOfDaySettings
{
    public Color fogColor;

    public float fogIntensity;

    public AmbianceVolumeTimeOfDaySettings(IFormattedFileReader reader)
    {
        if (reader == null)
        {
            fogColor = Color.white;
            fogIntensity = 1f;
        }
        else
        {
            fogColor = reader.readValue<Color>("Fog_Color");
            fogIntensity = reader.readValue<float>("Fog_Intensity");
        }
    }

    public void Write(IFormattedFileWriter writer)
    {
        writer.writeValue("Fog_Color", fogColor);
        writer.writeValue("Fog_Intensity", fogIntensity);
    }
}
