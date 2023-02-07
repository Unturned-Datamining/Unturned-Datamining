using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public abstract class DevkitHierarchyItemBase : MonoBehaviour, IDevkitHierarchyItem, IFormattedFileReadable, IFormattedFileWritable
{
    public virtual uint instanceID { get; set; }

    public virtual GameObject areaSelectGameObject => base.gameObject;

    public virtual bool ShouldSave => true;

    public virtual bool CanBeSelected => true;

    public NetId GetNetIdFromInstanceId()
    {
        if (instanceID != 0)
        {
            return LevelNetIdRegistry.GetDevkitObjectNetId(instanceID);
        }
        return NetId.INVALID;
    }

    public abstract void read(IFormattedFileReader reader);

    public abstract void write(IFormattedFileWriter writer);
}
