using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Framework.Devkit;

public interface IDevkitHierarchyItem : IFormattedFileReadable, IFormattedFileWritable
{
    uint instanceID { get; set; }

    GameObject areaSelectGameObject { get; }

    bool ShouldSave { get; }
}
