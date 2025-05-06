namespace SDG.Unturned;

internal readonly struct PopulateConditionParameters
{
    public readonly ENPCConditionType conditionType;

    public readonly IDatDictionary data;

    public readonly Local localization;

    public readonly IAssetErrorContext errorContext;

    public readonly string errorAdditionalInfo;

    /// <summary>
    /// Should only be used by <see cref="M:SDG.Unturned.INPCCondition.PopulateLegacy(SDG.Unturned.PopulateConditionParameters@)" />.
    /// For example: "Condition_##" where ## is an index.
    /// </summary>
    public readonly string legacyPrefix;

    /// <summary>
    /// Nelson 2025-03-11: not *super* happy about having this in here. Needed for UI_Requirements.
    /// </summary>
    public readonly int conditionIndex;

    /// <summary>
    /// Nelson 2025-03-11: not *super* happy about having this in here. Needed for UI_Requirements.
    /// </summary>
    public readonly int conditionsLength;

    public void ReportError(string message)
    {
        if (!string.IsNullOrEmpty(errorAdditionalInfo))
        {
            errorContext.ReportAssetError($"{errorAdditionalInfo} ({conditionType} condition) {message}");
        }
        else if (!string.IsNullOrEmpty(legacyPrefix))
        {
            errorContext.ReportAssetError($"{legacyPrefix} ({conditionType} condition) {message}");
        }
        else
        {
            errorContext.ReportAssetError($"({conditionType} condition) {message}");
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

    public PopulateConditionParameters(ENPCConditionType conditionType, IDatDictionary data, Local localization, IAssetErrorContext errorContext, string errorInfo, string legacyPrefix, int conditionIndex, int conditionsLength)
    {
        this.conditionType = conditionType;
        this.data = data;
        this.localization = localization;
        this.errorContext = errorContext;
        errorAdditionalInfo = errorInfo;
        this.legacyPrefix = legacyPrefix;
        this.conditionIndex = conditionIndex;
        this.conditionsLength = conditionsLength;
    }
}
