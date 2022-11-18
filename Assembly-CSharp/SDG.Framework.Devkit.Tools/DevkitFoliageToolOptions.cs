using System.IO;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Unturned;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit.Tools;

public class DevkitFoliageToolOptions : IFormattedFileReadable, IFormattedFileWritable
{
    private static DevkitFoliageToolOptions _instance;

    protected static float _addSensitivity;

    protected static float _removeSensitivity;

    public bool bakeInstancedMeshes = true;

    public bool bakeResources = true;

    public bool bakeObjects = true;

    public bool bakeClear;

    public bool bakeApplyScale;

    private float _brushRadius = 16f;

    public float brushFalloff = 0.5f;

    public float brushStrength = 0.05f;

    public float densityTarget = 1f;

    public ERayMask surfaceMask = ERayMask.LARGE | ERayMask.MEDIUM | ERayMask.SMALL | ERayMask.ENVIRONMENT | ERayMask.GROUND;

    public uint maxPreviewSamples = 1024u;

    protected static bool wasLoaded;

    public static DevkitFoliageToolOptions instance => _instance;

    public static float addSensitivity
    {
        get
        {
            return _addSensitivity;
        }
        set
        {
            _addSensitivity = value;
            UnturnedLog.info("Set add_sensitivity to: " + addSensitivity);
        }
    }

    public static float removeSensitivity
    {
        get
        {
            return _removeSensitivity;
        }
        set
        {
            _removeSensitivity = value;
            UnturnedLog.info("Set remove_sensitivity to: " + removeSensitivity);
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
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Foliage.tool");
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
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Foliage.tool");
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
        bakeInstancedMeshes = reader.readValue<bool>("Bake_Instanced_Meshes");
        bakeResources = reader.readValue<bool>("Bake_Resources");
        bakeObjects = reader.readValue<bool>("Bake_Objects");
        bakeClear = reader.readValue<bool>("Bake_Clear");
        bakeApplyScale = reader.readValue<bool>("Bake_Apply_Scale");
        brushRadius = reader.readValue<float>("Brush_Radius");
        brushFalloff = reader.readValue<float>("Brush_Falloff");
        brushStrength = reader.readValue<float>("Brush_Strength");
        densityTarget = reader.readValue<float>("Density_Target");
        surfaceMask = reader.readValue<ERayMask>("Surface_Mask");
        maxPreviewSamples = reader.readValue<uint>("Max_Preview_Samples");
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.writeValue("Bake_Instanced_Meshes", bakeInstancedMeshes);
        writer.writeValue("Bake_Resources", bakeResources);
        writer.writeValue("Bake_Objects", bakeObjects);
        writer.writeValue("Bake_Clear", bakeClear);
        writer.writeValue("Bake_Apply_Scale", bakeApplyScale);
        writer.writeValue("Brush_Radius", brushRadius);
        writer.writeValue("Brush_Falloff", brushFalloff);
        writer.writeValue("Brush_Strength", brushStrength);
        writer.writeValue("Density_Target", densityTarget);
        writer.writeValue("Surface_Mask", surfaceMask);
        writer.writeValue("Max_Preview_Samples", maxPreviewSamples);
    }

    static DevkitFoliageToolOptions()
    {
        _addSensitivity = 1f;
        _removeSensitivity = 1f;
        _instance = new DevkitFoliageToolOptions();
        load();
    }
}
