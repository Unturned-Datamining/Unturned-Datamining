using UnityEngine;

namespace SDG.Unturned;

public class StereoSongAsset : Asset
{
    public string titleText;

    public ContentReference<AudioClip> songContentRef;

    public MasterBundleReference<AudioClip> songMbRef;

    public bool isLoop;

    public string linkURL { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (localization.has("Name"))
        {
            titleText = localization.read("Name");
        }
        if (string.IsNullOrEmpty(titleText))
        {
            titleText = data.GetString("Title");
        }
        songContentRef = data.ParseStruct<ContentReference<AudioClip>>("Song");
        songMbRef = data.ParseStruct<MasterBundleReference<AudioClip>>("Song");
        linkURL = data.GetString("Link_URL");
        isLoop = data.ParseBool("Is_Loop");
    }

    protected virtual void construct()
    {
        songContentRef = ContentReference<AudioClip>.invalid;
        songMbRef = MasterBundleReference<AudioClip>.invalid;
        linkURL = null;
    }

    public StereoSongAsset()
    {
        construct();
    }
}
