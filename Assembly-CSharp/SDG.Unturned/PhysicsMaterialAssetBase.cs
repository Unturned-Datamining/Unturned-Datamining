using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Properties common to asset and extensions. For example both can specify sounds.
/// </summary>
public class PhysicsMaterialAssetBase : Asset
{
    public Dictionary<string, MasterBundleReference<OneShotAudioDefinition>> audioDefs;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        DatDictionary dictionary = data.GetDictionary("AudioDefs");
        if (dictionary == null)
        {
            return;
        }
        audioDefs = new Dictionary<string, MasterBundleReference<OneShotAudioDefinition>>();
        foreach (KeyValuePair<string, IDatNode> item in dictionary)
        {
            audioDefs[item.Key] = item.Value.ParseStruct<MasterBundleReference<OneShotAudioDefinition>>();
        }
    }
}
