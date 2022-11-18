using System.IO;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Unturned;
using Unturned.SystemEx;

namespace SDG.Framework.Devkit.Tools;

public class DevkitSelectionToolOptions : IFormattedFileReadable, IFormattedFileWritable
{
    private static DevkitSelectionToolOptions _instance;

    public float snapPosition = 1f;

    public float snapRotation = 15f;

    public float snapScale = 0.1f;

    public float surfaceOffset;

    public bool surfaceAlign;

    public bool localSpace;

    public bool lockHandles;

    public ERayMask selectionMask = ERayMask.LARGE | ERayMask.MEDIUM | ERayMask.SMALL | ERayMask.ENVIRONMENT | ERayMask.GROUND | ERayMask.CLIP | ERayMask.TRAP;

    protected static bool wasLoaded;

    public static DevkitSelectionToolOptions instance => _instance;

    public static void load()
    {
        wasLoaded = true;
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Selection.tool");
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
        string path = PathEx.Join(UnturnedPaths.RootDirectory, "Cloud", "Selection.tool");
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
        snapPosition = reader.readValue<float>("Snap_Position");
        snapRotation = reader.readValue<float>("Snap_Rotation");
        snapScale = reader.readValue<float>("Snap_Scale");
        surfaceOffset = reader.readValue<float>("Surface_Offset");
        surfaceAlign = reader.readValue<bool>("Surface_Align");
        localSpace = reader.readValue<bool>("Local_Space");
        lockHandles = reader.readValue<bool>("Lock_Handles");
        selectionMask = reader.readValue<ERayMask>("Selection_Mask");
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.writeValue("Snap_Position", snapPosition);
        writer.writeValue("Snap_Rotation", snapRotation);
        writer.writeValue("Snap_Scale", snapScale);
        writer.writeValue("Surface_Offset", surfaceOffset);
        writer.writeValue("Surface_Align", surfaceAlign);
        writer.writeValue("Local_Space", localSpace);
        writer.writeValue("Lock_Handles", lockHandles);
        writer.writeValue("Selection_Mask", selectionMask);
    }

    static DevkitSelectionToolOptions()
    {
        _instance = new DevkitSelectionToolOptions();
        load();
    }
}
