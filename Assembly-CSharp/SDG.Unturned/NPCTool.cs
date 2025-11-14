using System;

namespace SDG.Unturned;

public class NPCTool
{
    internal static Type[] conditionTypes;

    internal static Type[] rewardTypes;

    /// <summary>
    /// Was redirected to HolidayUtil but kept for plugin backwards compatibility.
    /// Refer to HolidayUtil for explanation of this weird situation.
    /// </summary>
    public static ENPCHoliday getActiveHoliday()
    {
        return Provider.authorityHoliday;
    }

    /// <summary>
    /// Was redirected to HolidayUtil but kept for plugin backwards compatibility.
    /// Refer to HolidayUtil for explanation of this weird situation.
    /// </summary>
    public static bool isHolidayActive(ENPCHoliday holiday)
    {
        return holiday == Provider.authorityHoliday;
    }

    public static bool doesLogicPass<T>(ENPCLogicType logicType, T a, T b) where T : IComparable
    {
        int num = a.CompareTo(b);
        return logicType switch
        {
            ENPCLogicType.LESS_THAN => num < 0, 
            ENPCLogicType.LESS_THAN_OR_EQUAL_TO => num <= 0, 
            ENPCLogicType.EQUAL => num == 0, 
            ENPCLogicType.NOT_EQUAL => num != 0, 
            ENPCLogicType.GREATER_THAN_OR_EQUAL_TO => num >= 0, 
            ENPCLogicType.GREATER_THAN => num > 0, 
            _ => false, 
        };
    }

    [Obsolete("NPCConditionsList.Parse should be used instead")]
    public static void readConditions(IDatDictionary data, Local localization, string prefix, INPCCondition[] conditions, Asset assetContext)
    {
        for (int i = 0; i < conditions.Length; i++)
        {
            string text = prefix + i;
            string text2 = text + "_Type";
            if (!data.ContainsKey(text2))
            {
                throw new NotSupportedException("Missing condition " + text2);
            }
            ENPCConditionType eNPCConditionType = data.ParseEnum(text2, ENPCConditionType.NONE);
            if (eNPCConditionType == ENPCConditionType.NONE)
            {
                assetContext.ReportAssetError(text2 + " unknown type");
                continue;
            }
            Type type = conditionTypes[(int)eNPCConditionType];
            if (type == null)
            {
                assetContext.ReportAssetError(text2 + " unable to create type");
                break;
            }
            INPCCondition iNPCCondition;
            try
            {
                iNPCCondition = Activator.CreateInstance(type) as INPCCondition;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, $"Caught exception instantiating {type}:");
                assetContext.ReportAssetError(text2 + " error creating type");
                break;
            }
            PopulateConditionParameters p = new PopulateConditionParameters(eNPCConditionType, data, localization, assetContext, null, text, i, conditions.Length);
            try
            {
                iNPCCondition.PopulateLegacy(in p);
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2, $"Caught exception populating condition {type}:");
            }
            conditions[i] = iNPCCondition;
        }
    }

    [Obsolete("NPCRewardsList.Parse should be used instead")]
    public static void readRewards(IDatDictionary data, Local localization, string prefix, INPCReward[] rewards, Asset assetContext)
    {
        for (int i = 0; i < rewards.Length; i++)
        {
            string text = prefix + i;
            string text2 = text + "_Type";
            if (!data.ContainsKey(text2))
            {
                throw new NotSupportedException("Missing reward " + text2);
            }
            ENPCRewardType eNPCRewardType = data.ParseEnum(text2, ENPCRewardType.NONE);
            if (eNPCRewardType == ENPCRewardType.NONE)
            {
                assetContext.ReportAssetError(text2 + " unknown type");
                continue;
            }
            Type type = rewardTypes[(int)eNPCRewardType];
            if (type == null)
            {
                assetContext.ReportAssetError(text2 + " unable to create type");
                break;
            }
            INPCReward iNPCReward;
            try
            {
                iNPCReward = Activator.CreateInstance(type) as INPCReward;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, $"Caught exception instantiating {type}:");
                assetContext.ReportAssetError(text2 + " error creating type");
                break;
            }
            PopulateRewardParameters p = new PopulateRewardParameters(eNPCRewardType, data, localization, assetContext, null, text);
            try
            {
                iNPCReward.PopulateLegacy(in p);
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2, $"Caught exception populating reward {type}:");
            }
            rewards[i] = iNPCReward;
        }
    }

    static NPCTool()
    {
        conditionTypes = new Type[28]
        {
            null,
            typeof(NPCExperienceCondition),
            typeof(NPCReputationCondition),
            typeof(NPCBoolFlagCondition),
            typeof(NPCShortFlagCondition),
            typeof(NPCQuestCondition),
            typeof(NPCSkillsetCondition),
            typeof(NPCItemCondition),
            typeof(NPCZombieKillsCondition),
            typeof(NPCHordeKillsCondition),
            typeof(NPCAnimalKillsCondition),
            typeof(NPCCompareFlagsCondition),
            typeof(NPCTimeOfDayCondition),
            typeof(NPCPlayerLifeHealthCondition),
            typeof(NPCPlayerLifeFoodCondition),
            typeof(NPCPlayerLifeWaterCondition),
            typeof(NPCPlayerLifeVirusCondition),
            typeof(NPCHolidayCondition),
            typeof(NPCPlayerKillsCondition),
            typeof(NPCObjectKillsCondition),
            typeof(NPCCurrencyCondition),
            typeof(NPCTreeKillsCondition),
            typeof(NPCWeatherStatusCondition),
            typeof(NPCWeatherBlendAlphaCondition),
            typeof(NPCIsFullMoonCondition),
            typeof(NPCDateCounterCondition),
            typeof(NPCPlayerLifeStaminaCondition),
            typeof(NPCVolumeOverlapCondition)
        };
        rewardTypes = new Type[28]
        {
            null,
            typeof(NPCExperienceReward),
            typeof(NPCReputationReward),
            typeof(NPCBoolFlagReward),
            typeof(NPCShortFlagReward),
            typeof(NPCRandomShortFlagReward),
            typeof(NPCQuestReward),
            typeof(NPCItemReward),
            typeof(NPCRandomItemReward),
            typeof(NPCAchievementReward),
            typeof(NPCVehicleReward),
            typeof(NPCTeleportReward),
            typeof(NPCEventReward),
            typeof(NPCFlagMathReward),
            typeof(NPCCurrencyReward),
            typeof(NPCHintReward),
            typeof(NPCPlayerSpawnpointReward),
            typeof(NPCPlayerLifeHealthReward),
            typeof(NPCPlayerLifeFoodReward),
            typeof(NPCPlayerLifeWaterReward),
            typeof(NPCPlayerLifeVirusReward),
            typeof(NPCRewardsListAssetReward),
            typeof(NPCCutsceneModeReward),
            typeof(NPCPlayerLifeStaminaReward),
            typeof(NPCEffectReward),
            typeof(NPCAirdropReward),
            typeof(NPCZombieReward),
            typeof(NPCRemoveZombieReward)
        };
    }
}
