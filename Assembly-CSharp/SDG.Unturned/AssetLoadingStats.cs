using UnityEngine;

namespace SDG.Unturned;

internal class AssetLoadingStats
{
    public bool isLoadingAssetBundles;

    public int totalRegisteredSearchLocations;

    public int totalSearchLocationsFinishedSearching;

    public int totalMasterBundlesFound;

    public int totalMasterBundlesLoaded;

    public int totalFilesFound;

    public int totalFilesRead;

    public int totalFilesLoaded;

    private int baselineRegisteredSearchLocations;

    private int baselineSearchLocationsFinishedSearching;

    private int baselineMasterBundlesFound;

    private int baselineMasterBundlesLoaded;

    private int baselineFilesFound;

    private int baselineFilesRead;

    private int baselineFilesLoaded;

    public int RegisteredSearchLocations => totalRegisteredSearchLocations - baselineRegisteredSearchLocations;

    public int SearchLocationsFinishedSearching => totalSearchLocationsFinishedSearching - baselineSearchLocationsFinishedSearching;

    public int AssetBundlesFound => totalMasterBundlesFound - baselineMasterBundlesFound;

    public int AssetBundlesLoaded => totalMasterBundlesLoaded - baselineMasterBundlesLoaded;

    public int FilesFound => totalFilesFound - baselineFilesFound;

    public int FilesRead => totalFilesRead - baselineFilesRead;

    public int FilesLoaded => totalFilesLoaded - baselineFilesLoaded;

    public float EstimateAssetBundleProgressPercentage()
    {
        float num = ((RegisteredSearchLocations > 1) ? (1f / (float)RegisteredSearchLocations) : 1f);
        float num2 = ((SearchLocationsFinishedSearching > 0) ? ((float)SearchLocationsFinishedSearching * num) : num);
        return Mathf.Clamp01(((AssetBundlesFound > 1) ? ((float)AssetBundlesLoaded / (float)AssetBundlesFound) : 0f) * num2);
    }

    public float EstimateSearchProgressPercentage()
    {
        if (RegisteredSearchLocations <= 0)
        {
            return 0f;
        }
        return (float)SearchLocationsFinishedSearching / (float)RegisteredSearchLocations;
    }

    public float EstimateReadProgressPercentage()
    {
        float num = ((RegisteredSearchLocations > 1) ? (1f / (float)RegisteredSearchLocations) : 1f);
        float num2 = ((SearchLocationsFinishedSearching > 0) ? ((float)SearchLocationsFinishedSearching * num) : num);
        return Mathf.Clamp01(((FilesFound > 1) ? ((float)FilesRead / (float)FilesFound) : 0f) * num2);
    }

    public float EstimateFileProgressPercentage()
    {
        float num = ((RegisteredSearchLocations > 1) ? (1f / (float)RegisteredSearchLocations) : 1f);
        float num2 = ((SearchLocationsFinishedSearching > 0) ? ((float)SearchLocationsFinishedSearching * num) : num);
        return Mathf.Clamp01(((FilesFound > 1) ? ((float)FilesLoaded / (float)FilesFound) : 0f) * num2);
    }

    public void Reset()
    {
        baselineRegisteredSearchLocations = totalRegisteredSearchLocations;
        baselineSearchLocationsFinishedSearching = totalSearchLocationsFinishedSearching;
        baselineMasterBundlesFound = totalMasterBundlesFound;
        baselineMasterBundlesLoaded = totalMasterBundlesLoaded;
        baselineFilesFound = totalFilesFound;
        baselineFilesRead = totalFilesRead;
        baselineFilesLoaded = totalFilesLoaded;
    }
}
