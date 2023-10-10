using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class DevkitHierarchyWorldItem : DevkitHierarchyItemBase
{
    public Vector3 inspectablePosition
    {
        get
        {
            return base.transform.localPosition;
        }
        set
        {
            base.transform.position = value;
        }
    }

    public Quaternion inspectableRotation
    {
        get
        {
            return base.transform.localRotation;
        }
        set
        {
            base.transform.rotation = value;
        }
    }

    public Vector3 inspectableScale
    {
        get
        {
            return base.transform.localScale;
        }
        set
        {
            base.transform.localScale = value;
        }
    }

    public override void read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        if (reader != null)
        {
            readHierarchyItem(reader);
        }
    }

    protected virtual void readHierarchyItem(IFormattedFileReader reader)
    {
        base.transform.position = reader.readValue<Vector3>("Position");
        base.transform.SetRotation_RoundIfNearlyAxisAligned(reader.readValue<Quaternion>("Rotation"));
        Vector3 localScale = reader.readValue<Vector3>("Scale");
        localScale.x = Mathf.Clamp(localScale.x, -100000f, 100000f);
        localScale.y = Mathf.Clamp(localScale.y, -100000f, 100000f);
        localScale.z = Mathf.Clamp(localScale.z, -100000f, 100000f);
        base.transform.SetLocalScale_RoundIfNearlyEqualToOne(localScale);
    }

    public override void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writeHierarchyItem(writer);
        writer.endObject();
    }

    protected virtual void writeHierarchyItem(IFormattedFileWriter writer)
    {
        writer.writeValue("Position", base.transform.position);
        writer.writeValue("Rotation", base.transform.rotation);
        writer.writeValue("Scale", base.transform.localScale);
    }
}
