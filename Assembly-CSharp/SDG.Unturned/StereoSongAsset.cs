using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class StereoSongAsset : Asset
{
    public string titleText;

    public ContentReference<AudioClip> songContentRef;

    public MasterBundleReference<AudioClip> songMbRef;

    public bool isLoop;

    public string linkURL { get; protected set; }

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        if (string.IsNullOrEmpty(titleText))
        {
            titleText = reader.readValue("Title");
        }
        songContentRef = reader.readValue<ContentReference<AudioClip>>("Song");
        songMbRef = reader.readValue<MasterBundleReference<AudioClip>>("Song");
        linkURL = reader.readValue("Link_URL");
        isLoop = reader.readValue<bool>("Is_Loop");
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Song", songContentRef);
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

    public StereoSongAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        construct();
        if (localization.has("Name"))
        {
            titleText = localization.read("Name");
        }
    }
}
