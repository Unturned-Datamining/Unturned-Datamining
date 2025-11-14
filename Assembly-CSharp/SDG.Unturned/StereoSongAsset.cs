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

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        titleText = p.localization.FormatOrEmpty("Name");
        if (string.IsNullOrEmpty(titleText))
        {
            titleText = p.data.GetString("Title");
        }
        songContentRef = p.data.ParseStruct<ContentReference<AudioClip>>("Song");
        songMbRef = p.data.ParseStruct<MasterBundleReference<AudioClip>>("Song");
        linkURL = p.data.GetString("Link_URL");
        isLoop = p.data.ParseBool("Is_Loop");
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
