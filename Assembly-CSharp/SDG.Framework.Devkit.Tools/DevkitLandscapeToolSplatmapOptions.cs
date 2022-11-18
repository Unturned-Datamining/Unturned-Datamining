using System.IO;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit.Tools;

public class DevkitLandscapeToolSplatmapOptions : IFormattedFileReadable, IFormattedFileWritable
{
    private static DevkitLandscapeToolSplatmapOptions _instance;

    protected static float _paintSensitivity;

    private float _brushRadius = 16f;

    public float brushFalloff = 0.5f;

    public float brushStrength = 1f;

    public float autoStrength = 1f;

    public float smoothStrength = 1f;

    public bool useWeightTarget;

    public float weightTarget;

    public uint maxPreviewSamples = 1024u;

    public EDevkitLandscapeToolSplatmapSmoothMethod smoothMethod = EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE;

    public EDevkitLandscapeToolSplatmapPreviewMethod previewMethod;

    public bool useAutoSlope;

    public float autoMinAngleBegin = 50f;

    public float autoMinAngleEnd = 70f;

    public float autoMaxAngleBegin = 90f;

    public float autoMaxAngleEnd = 90f;

    public bool useAutoFoundation;

    public float autoRayRadius = 1f;

    public float autoRayLength = 512f;

    public ERayMask autoRayMask = (ERayMask)RayMasks.BLOCK_GRASS;

    protected static bool wasLoaded;

    public static DevkitLandscapeToolSplatmapOptions instance => _instance;

    public static float paintSensitivity
    {
        get
        {
            return _paintSensitivity;
        }
        set
        {
            _paintSensitivity = value;
            UnturnedLog.info("Set paint_sensitivity to: " + paintSensitivity);
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
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Splatmap.tool");
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
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Splatmap.tool");
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
        autoStrength = reader.readValue<float>("Auto_Strength");
        smoothStrength = reader.readValue<float>("Smooth_Strength");
        useWeightTarget = reader.readValue<bool>("Use_Weight_Target");
        weightTarget = reader.readValue<float>("Weight_Target");
        maxPreviewSamples = reader.readValue<uint>("Max_Preview_Samples");
        smoothMethod = reader.readValue<EDevkitLandscapeToolSplatmapSmoothMethod>("Smooth_Method");
        previewMethod = reader.readValue<EDevkitLandscapeToolSplatmapPreviewMethod>("Preview_Method");
        useAutoSlope = reader.readValue<bool>("Use_Auto_Slope");
        autoMinAngleBegin = reader.readValue<float>("Auto_Min_Angle_Begin");
        autoMinAngleEnd = reader.readValue<float>("Auto_Min_Angle_End");
        autoMaxAngleBegin = reader.readValue<float>("Auto_Max_Angle_Begin");
        autoMaxAngleEnd = reader.readValue<float>("Auto_Max_Angle_End");
        useAutoFoundation = reader.readValue<bool>("Use_Auto_Foundation");
        autoRayRadius = reader.readValue<float>("Auto_Ray_Radius");
        autoRayLength = reader.readValue<float>("Auto_Ray_Length");
        autoRayMask = reader.readValue<ERayMask>("Auto_Ray_Mask");
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.writeValue("Brush_Radius", brushRadius);
        writer.writeValue("Brush_Falloff", brushFalloff);
        writer.writeValue("Brush_Strength", brushStrength);
        writer.writeValue("Auto_Strength", autoStrength);
        writer.writeValue("Smooth_Strength", smoothStrength);
        writer.writeValue("Use_Weight_Target", useWeightTarget);
        writer.writeValue("Weight_Target", weightTarget);
        writer.writeValue("Max_Preview_Samples", maxPreviewSamples);
        writer.writeValue("Smooth_Method", smoothMethod);
        writer.writeValue("Preview_Method", previewMethod);
        writer.writeValue("Use_Auto_Slope", useAutoSlope);
        writer.writeValue("Auto_Min_Angle_Begin", autoMinAngleBegin);
        writer.writeValue("Auto_Min_Angle_End", autoMinAngleEnd);
        writer.writeValue("Auto_Max_Angle_Begin", autoMaxAngleBegin);
        writer.writeValue("Auto_Max_Angle_End", autoMaxAngleEnd);
        writer.writeValue("Use_Auto_Foundation", useAutoFoundation);
        writer.writeValue("Auto_Ray_Radius", autoRayRadius);
        writer.writeValue("Auto_Ray_Length", autoRayLength);
        writer.writeValue("Auto_Ray_Mask", autoRayMask);
    }

    static DevkitLandscapeToolSplatmapOptions()
    {
        _paintSensitivity = 1f;
        _instance = new DevkitLandscapeToolSplatmapOptions();
        load();
    }
}
