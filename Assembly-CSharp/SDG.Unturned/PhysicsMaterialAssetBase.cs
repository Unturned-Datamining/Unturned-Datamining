using System.Collections.Generic;
using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public class PhysicsMaterialAssetBase : Asset
{
    public Dictionary<string, MasterBundleReference<OneShotAudioDefinition>> audioDefs;

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        IFormattedFileReader formattedFileReader = reader.readObject("AudioDefs");
        if (formattedFileReader == null)
        {
            return;
        }
        audioDefs = new Dictionary<string, MasterBundleReference<OneShotAudioDefinition>>();
        foreach (string key in formattedFileReader.getKeys())
        {
            audioDefs[key] = formattedFileReader.readValue<MasterBundleReference<OneShotAudioDefinition>>(key);
        }
    }

    public PhysicsMaterialAssetBase(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
