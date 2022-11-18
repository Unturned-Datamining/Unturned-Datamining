using System.IO;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit.Tools;

public class DevkitLandscapeToolHeightmapOptions : IFormattedFileReadable, IFormattedFileWritable
{
    private static DevkitLandscapeToolHeightmapOptions _instance;

    protected static float _adjustSensitivity;

    protected static float _flattenSensitivity;

    private float _brushRadius = 16f;

    public float brushFalloff = 0.5f;

    public float brushStrength = 0.05f;

    public float flattenStrength = 1f;

    public float smoothStrength = 1f;

    public float flattenTarget;

    public uint maxPreviewSamples = 1024u;

    public EDevkitLandscapeToolHeightmapSmoothMethod smoothMethod = EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE;

    public EDevkitLandscapeToolHeightmapFlattenMethod flattenMethod;

    protected static bool wasLoaded;

    public static DevkitLandscapeToolHeightmapOptions instance => _instance;

    public static float adjustSensitivity
    {
        get
        {
            return _adjustSensitivity;
        }
        set
        {
            _adjustSensitivity = value;
            UnturnedLog.info("Set adjust_sensitivity to: " + adjustSensitivity);
        }
    }

    public static float flattenSensitivity
    {
        get
        {
            return _flattenSensitivity;
        }
        set
        {
            _flattenSensitivity = value;
            UnturnedLog.info("Set flatten_sensitivity to: " + flattenSensitivity);
        }
    }

    public float brushRadius
    {
        get
        {
            return _brushRadius;
        }
        set
        {
            _brushRadius = Mathf.Clamp(value, 0f, 2048f);
        }
    }

    public static void load()
    {
        wasLoaded = true;
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Heightmap.tool");
        string directoryName = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        if (!File.Exists(path))
        {
            return;
        }
        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader input = new StreamReader(stream);
        IFormattedFileReader reader = new KeyValueTableReader(input);
        instance.read(reader);
    }

    public static void save()
    {
        if (!wasLoaded)
        {
            return;
        }
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Heightmap.tool");
        string directoryName = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        using StreamWriter writer = new StreamWriter(path);
        ((IFormattedFileWriter)new KeyValueTableWriter(writer)).writeValue(instance);
    }

    public void read(IFormattedFileReader reader)
    {
        brushRadius = reader.readValue<float>("Brush_Radius");
        brushFalloff = reader.readValue<float>("Brush_Falloff");
        brushStrength = reader.readValue<float>("Brush_Strength");
        flattenStrength = reader.readValue<float>("Flatten_Strength");
        smoothStrength = reader.readValue<float>("Smooth_Strength");
        flattenTarget = reader.readValue<float>("Flatten_Target");
        maxPreviewSamples = reader.readValue<uint>("Max_Preview_Samples");
        smoothMethod = reader.readValue<EDevkitLandscapeToolHeightmapSmoothMethod>("Smooth_Method");
        flattenMethod = reader.readValue<EDevkitLandscapeToolHeightmapFlattenMethod>("Flatten_Method");
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.writeValue("Brush_Radius", brushRadius);
        writer.writeValue("Brush_Falloff", brushFalloff);
        writer.writeValue("Brush_Strength", brushStrength);
        writer.writeValue("Flatten_Strength", flattenStrength);
        writer.writeValue("Smooth_Strength", smoothStrength);
        writer.writeValue("Flatten_Target", flattenTarget);
        writer.writeValue("Max_Preview_Samples", maxPreviewSamples);
        writer.writeValue("Smooth_Method", smoothMethod);
        writer.writeValue("Flatten_Method", flattenMethod);
    }

    static DevkitLandscapeToolHeightmapOptions()
    {
        _adjustSensitivity = 0.1f;
        _flattenSensitivity = 1f;
        _instance = new DevkitLandscapeToolHeightmapOptions();
        load();
    }
}
