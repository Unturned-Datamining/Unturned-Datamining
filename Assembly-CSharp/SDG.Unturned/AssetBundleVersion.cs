namespace SDG.Unturned;

/// <summary>
/// Used to aid backwards compatibility as much as possible when transitioning Unity versions breaks asset bundles.
/// </summary>
public static class AssetBundleVersion
{
    /// <summary>
    /// Unity 5.5 and earlier per-asset .unity3d file.
    /// </summary>
    public const int UNITY_5 = 1;

    /// <summary>
    /// When "master bundles" were first introduced in order to convert older Unity 5.5 asset bundles in bulk.
    /// </summary>
    public const int UNITY_2017_LTS = 2;

    /// <summary>
    /// Unity 2018 needed a new version number in order to convert materials from 2017 LTS asset bundles. 2019 did not need a
    /// new version number, but in retrospect it seems unfortunate that we cannot distinguish them, so 2020 does have its own.
    /// </summary>
    public const int UNITY_2018_AND_2019_LTS = 3;

    public const int UNITY_2020_LTS = 4;

    /// <summary>
    /// 2021 LTS+
    /// </summary>
    public const int NEWEST = 5;
}
