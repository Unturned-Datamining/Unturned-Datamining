namespace SDG.Unturned;

internal readonly struct PopulateRewardParameters
{
    public readonly ENPCRewardType rewardType;

    public readonly IDatDictionary data;

    public readonly Local localization;

    public readonly IAssetErrorContext errorContext;

    public readonly string errorAdditionalInfo;

    /// <summary>
    /// Should only be used by <see cref="M:SDG.Unturned.INPCReward.PopulateLegacy(SDG.Unturned.PopulateRewardParameters@)" />.
    /// For example: "Condition_##" where ## is an index.
    /// </summary>
    public readonly string legacyPrefix;

    public void ReportError(string message)
    {
        if (!string.IsNullOrEmpty(errorAdditionalInfo))
        {
            errorContext.ReportAssetError($"{errorAdditionalInfo} ({rewardType} reward) {message}");
        }
        else if (!string.IsNullOrEmpty(legacyPrefix))
        {
            errorContext.ReportAssetError($"{legacyPrefix} ({rewardType} reward) {message}");
        }
        else
        {
            errorContext.ReportAssetError($"({rewardType} reward) {message}");
        }
    }

    public void ReportRequiredOptionInvalid(string key)
    {
        if (!string.IsNullOrEmpty(legacyPrefix))
        {
            key = legacyPrefix + "_" + key;
        }
        if (data.ContainsKey(key))
        {
            ReportError("unable to parse " + key + " from \"" + data.GetString(key) + "\"");
        }
        else
        {
            ReportError("requires " + key);
        }
    }

    public PopulateRewardParameters(ENPCRewardType rewardType, IDatDictionary data, Local localization, IAssetErrorContext errorContext, string errorInfo, string legacyPrefix)
    {
        this.rewardType = rewardType;
        this.data = data;
        this.localization = localization;
        this.errorContext = errorContext;
        errorAdditionalInfo = errorInfo;
        this.legacyPrefix = legacyPrefix;
    }
}
