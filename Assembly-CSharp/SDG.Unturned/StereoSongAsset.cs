using UnityEngine;

namespace SDG.Unturned;

public class StereoSongAsset : Asset
{
    /// <summary>
    /// Text from *.dat localization file.
    /// </summary>
    public string titleText;

    /// <summary>
    /// Older *.content asset bundle reference. 
    /// </summary>
    public ContentReference<AudioClip> songContentRef;

    /// <summary>
    /// Newer *.masterbundle reference.
    /// </summary>
    public MasterBundleReference<AudioClip> songMbRef;

    /// <summary>
    /// Whether audio source should loop.
    /// </summary>
    public bool isLoop;

    /// <summary>
    /// Optional URL to open in web browser.
    /// </summary>
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
