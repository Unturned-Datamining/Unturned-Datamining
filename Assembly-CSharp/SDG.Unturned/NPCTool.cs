using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCTool
{
    public static ENPCHoliday getActiveHoliday()
    {
        return Provider.authorityHoliday;
    }

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

    public static void readConditions(DatDictionary data, Local localization, string prefix, INPCCondition[] conditions, Asset assetContext)
    {
        for (int i = 0; i < conditions.Length; i++)
        {
            string text = prefix + i + "_Type";
            if (!data.ContainsKey(text))
            {
                throw new NotSupportedException("Missing condition " + text);
            }
            ENPCConditionType eNPCConditionType = data.ParseEnum(text, ENPCConditionType.NONE);
            if (eNPCConditionType == ENPCConditionType.NONE)
            {
                Assets.reportError(assetContext, "{0} unknown type", text);
                continue;
            }
            string desc = localization.read(prefix + i);
            desc = ItemTool.filterRarityRichText(desc);
            bool flag = data.ContainsKey(prefix + i + "_Reset");
            ENPCLogicType eNPCLogicType = data.ParseEnum(prefix + i + "_Logic", eNPCConditionType switch
            {
                ENPCConditionType.ITEM => ENPCLogicType.GREATER_THAN_OR_EQUAL_TO, 
                ENPCConditionType.HOLIDAY => ENPCLogicType.EQUAL, 
                _ => ENPCLogicType.NONE, 
            });
            INPCCondition iNPCCondition = null;
            switch (eNPCConditionType)
            {
            case ENPCConditionType.EXPERIENCE:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Experience condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCExperienceCondition(data.ParseUInt32(prefix + i + "_Value"), eNPCLogicType, desc, flag);
                break;
            case ENPCConditionType.REPUTATION:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Reputation condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCReputationCondition(data.ParseInt32(prefix + i + "_Value"), eNPCLogicType, desc);
                break;
            case ENPCConditionType.FLAG_BOOL:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Bool flag condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Bool flag condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCBoolFlagCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseBool(prefix + i + "_Value"), data.ContainsKey(prefix + i + "_Allow_Unset"), eNPCLogicType, desc, flag);
                break;
            case ENPCConditionType.FLAG_SHORT:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Short flag condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Short flag condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCShortFlagCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), data.ContainsKey(prefix + i + "_Allow_Unset"), eNPCLogicType, desc, flag);
                break;
            case ENPCConditionType.QUEST:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Quest condition " + prefix + i + " missing _ID");
                }
                ENPCQuestStatus eNPCQuestStatus = ENPCQuestStatus.NONE;
                string key9 = prefix + i + "_Status";
                if (!data.ContainsKey(key9))
                {
                    Assets.reportError(assetContext, "Quest condition " + prefix + i + " missing _Status");
                }
                else
                {
                    eNPCQuestStatus = data.ParseEnum(key9, ENPCQuestStatus.NONE);
                    if (eNPCQuestStatus == ENPCQuestStatus.NONE && flag)
                    {
                        Assets.reportError(assetContext, "Quest condition " + prefix + i + " has Reset enabled with Status None (probably accidental)");
                    }
                }
                data.ParseGuidOrLegacyId(prefix + i + "_ID", out var guid2, out var legacyId2);
                iNPCCondition = new NPCQuestCondition(guid2, legacyId2, eNPCQuestStatus, data.ContainsKey(prefix + i + "_Ignore_NPC"), eNPCLogicType, desc, flag);
                break;
            }
            case ENPCConditionType.SKILLSET:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Skillset condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCSkillsetCondition(data.ParseEnum(prefix + i + "_Value", EPlayerSkillset.NONE), eNPCLogicType, desc);
                break;
            case ENPCConditionType.ITEM:
            {
                string key6 = prefix + i + "_ID";
                if (!data.ContainsKey(key6))
                {
                    Assets.reportError(assetContext, "Item condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Amount"))
                {
                    Assets.reportError(assetContext, "Item condition " + prefix + i + " missing _Amount");
                }
                if (flag && eNPCLogicType != ENPCLogicType.GREATER_THAN_OR_EQUAL_TO)
                {
                    eNPCLogicType = ENPCLogicType.GREATER_THAN_OR_EQUAL_TO;
                    Assets.reportError(assetContext, "Resetting item condition only compatible with >= comparison. If you have a use in mind feel free to email Nelson.");
                }
                data.ParseGuidOrLegacyId(key6, out var guid, out var legacyId);
                iNPCCondition = new NPCItemCondition(guid, legacyId, data.ParseUInt16(prefix + i + "_Amount", 0), eNPCLogicType, desc, flag);
                break;
            }
            case ENPCConditionType.KILLS_ZOMBIE:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Zombie kills condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Zombie kills condition " + prefix + i + " missing _Value");
                }
                EZombieSpeciality newZombie = EZombieSpeciality.NONE;
                if (data.ContainsKey(prefix + i + "_Zombie"))
                {
                    newZombie = data.ParseEnum(prefix + i + "_Zombie", EZombieSpeciality.NONE);
                }
                else
                {
                    Assets.reportError(assetContext, "Zombie kills condition " + prefix + i + " missing _Zombie");
                }
                int newSpawnQuantity = 1;
                if (data.ContainsKey(prefix + i + "_Spawn_Quantity"))
                {
                    newSpawnQuantity = data.ParseInt32(prefix + i + "_Spawn_Quantity");
                }
                byte newNav2 = data.ParseUInt8(prefix + i + "_Nav", byte.MaxValue);
                float newRadius = data.ParseFloat(prefix + i + "_Radius", 512f);
                float newMinRadius = data.ParseFloat(prefix + i + "_MinRadius");
                iNPCCondition = new NPCZombieKillsCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), newZombie, data.ContainsKey(prefix + i + "_Spawn"), newSpawnQuantity, newNav2, newRadius, newMinRadius, desc, flag);
                break;
            }
            case ENPCConditionType.KILLS_HORDE:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Horde kills condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Horde kills condition " + prefix + i + " missing _Value");
                }
                if (!data.ContainsKey(prefix + i + "_Nav"))
                {
                    Assets.reportError(assetContext, "Horde kills condition " + prefix + i + " missing _Nav");
                }
                iNPCCondition = new NPCHordeKillsCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), data.ParseUInt8(prefix + i + "_Nav", 0), desc, flag);
                break;
            case ENPCConditionType.KILLS_ANIMAL:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Animal kills condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Animal kills condition " + prefix + i + " missing _Value");
                }
                if (!data.ContainsKey(prefix + i + "_Animal"))
                {
                    Assets.reportError(assetContext, "Animal kills condition " + prefix + i + " missing _Animal");
                }
                iNPCCondition = new NPCAnimalKillsCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), data.ParseUInt16(prefix + i + "_Animal", 0), desc, flag);
                break;
            case ENPCConditionType.COMPARE_FLAGS:
                if (!data.ContainsKey(prefix + i + "_A_ID"))
                {
                    Assets.reportError(assetContext, "Compare flags condition " + prefix + i + " missing _A_ID");
                }
                if (!data.ContainsKey(prefix + i + "_B_ID"))
                {
                    Assets.reportError(assetContext, "Compare flags condition " + prefix + i + " missing _B_ID");
                }
                iNPCCondition = new NPCCompareFlagsCondition(data.ParseUInt16(prefix + i + "_A_ID", 0), data.ParseUInt16(prefix + i + "_B_ID", 0), data.ContainsKey(prefix + i + "_Allow_A_Unset"), data.ContainsKey(prefix + i + "_Allow_B_Unset"), eNPCLogicType, desc, flag);
                break;
            case ENPCConditionType.TIME_OF_DAY:
                if (!data.ContainsKey(prefix + i + "_Second"))
                {
                    Assets.reportError(assetContext, "Time of day condition " + prefix + i + " missing _Second");
                }
                iNPCCondition = new NPCTimeOfDayCondition(data.ParseInt32(prefix + i + "_Second"), eNPCLogicType, desc, flag);
                break;
            case ENPCConditionType.PLAYER_LIFE_HEALTH:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Player life health condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCPlayerLifeHealthCondition(data.ParseInt32(prefix + i + "_Value"), eNPCLogicType, desc);
                break;
            case ENPCConditionType.PLAYER_LIFE_FOOD:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Player life food condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCPlayerLifeFoodCondition(data.ParseInt32(prefix + i + "_Value"), eNPCLogicType, desc);
                break;
            case ENPCConditionType.PLAYER_LIFE_WATER:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Player life water condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCPlayerLifeWaterCondition(data.ParseInt32(prefix + i + "_Value"), eNPCLogicType, desc);
                break;
            case ENPCConditionType.PLAYER_LIFE_VIRUS:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Player life virus condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCPlayerLifeVirusCondition(data.ParseInt32(prefix + i + "_Value"), eNPCLogicType, desc);
                break;
            case ENPCConditionType.HOLIDAY:
            {
                ENPCHoliday eNPCHoliday = ENPCHoliday.NONE;
                if (data.ContainsKey(prefix + i + "_Value"))
                {
                    eNPCHoliday = data.ParseEnum(prefix + i + "_Value", ENPCHoliday.NONE);
                    if (eNPCHoliday == ENPCHoliday.NONE)
                    {
                        Assets.reportError(assetContext, "Holiday condition " + prefix + i + " _Value is None");
                    }
                }
                else
                {
                    Assets.reportError(assetContext, "Holiday condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCHolidayCondition(eNPCHoliday, eNPCLogicType);
                break;
            }
            case ENPCConditionType.KILLS_PLAYER:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Player kills condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Player kills condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCPlayerKillsCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), desc, flag);
                break;
            case ENPCConditionType.KILLS_OBJECT:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Object kills condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Object kills condition " + prefix + i + " missing _Value");
                }
                Guid newObjectGuid;
                if (data.ContainsKey(prefix + i + "_Object"))
                {
                    newObjectGuid = data.ParseGuid(prefix + i + "_Object");
                }
                else
                {
                    newObjectGuid = default(Guid);
                    Assets.reportError(assetContext, "Object kills condition " + prefix + i + " missing _Object (GUID)");
                }
                byte newNav = data.ParseUInt8(prefix + i + "_Nav", byte.MaxValue);
                iNPCCondition = new NPCObjectKillsCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), newObjectGuid, newNav, desc, flag);
                break;
            }
            case ENPCConditionType.CURRENCY:
            {
                string key7 = prefix + i + "_GUID";
                if (!data.ContainsKey(key7))
                {
                    Assets.reportError(assetContext, "Currency condition " + prefix + i + " missing _GUID");
                }
                string key8 = prefix + i + "_Value";
                if (!data.ContainsKey(key8))
                {
                    Assets.reportError(assetContext, "Currency condition " + prefix + i + " missing _Value");
                }
                AssetReference<ItemCurrencyAsset> newCurrency = data.readAssetReference<ItemCurrencyAsset>(key7);
                uint newValue = data.ParseUInt32(key8);
                iNPCCondition = new NPCCurrencyCondition(newCurrency, newValue, eNPCLogicType, desc, flag);
                break;
            }
            case ENPCConditionType.KILLS_TREE:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Tree kills condition " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Tree kills condition " + prefix + i + " missing _Value");
                }
                Guid newTreeGuid;
                if (data.ContainsKey(prefix + i + "_Tree"))
                {
                    newTreeGuid = data.ParseGuid(prefix + i + "_Tree");
                }
                else
                {
                    newTreeGuid = default(Guid);
                    Assets.reportError(assetContext, "Tree kills condition " + prefix + i + " missing _Tree (GUID)");
                }
                iNPCCondition = new NPCTreeKillsCondition(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), newTreeGuid, desc, flag);
                break;
            }
            case ENPCConditionType.WEATHER_STATUS:
            {
                string key4 = prefix + i + "_GUID";
                if (!data.ContainsKey(key4))
                {
                    Assets.reportError(assetContext, "Weather condition " + prefix + i + " missing _GUID");
                }
                string key5 = prefix + i + "_Value";
                if (!data.ContainsKey(key5))
                {
                    Assets.reportError(assetContext, "Weather condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCWeatherStatusCondition(data.readAssetReference<WeatherAssetBase>(key4), data.ParseEnum(key5, ENPCWeatherStatus.Active), eNPCLogicType, desc);
                break;
            }
            case ENPCConditionType.WEATHER_BLEND_ALPHA:
            {
                string key2 = prefix + i + "_GUID";
                if (!data.ContainsKey(key2))
                {
                    Assets.reportError(assetContext, "Weather condition " + prefix + i + " missing _GUID");
                }
                string key3 = prefix + i + "_Value";
                if (!data.ContainsKey(key3))
                {
                    Assets.reportError(assetContext, "Weather condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCWeatherBlendAlphaCondition(data.readAssetReference<WeatherAssetBase>(key2), data.ParseFloat(key3), eNPCLogicType, desc);
                break;
            }
            case ENPCConditionType.IS_FULL_MOON:
            {
                string key = prefix + i + "_Value";
                if (!data.ContainsKey(key))
                {
                    Assets.reportError(assetContext, "Full moon condition " + prefix + i + " missing _Value");
                }
                iNPCCondition = new NPCIsFullMoonCondition(data.ParseBool(key, defaultValue: true), desc);
                break;
            }
            }
            if (iNPCCondition != null)
            {
                List<int> list = null;
                if (data.TryGetString(prefix + i + "_UI_Requirements", out var value))
                {
                    string[] array = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (array == null || array.Length < 1)
                    {
                        Assets.reportError(assetContext, prefix + i + " empty _UI_Requirements");
                    }
                    else
                    {
                        list = new List<int>(array.Length);
                        string[] array2 = array;
                        foreach (string text2 in array2)
                        {
                            if (!int.TryParse(text2, out var result))
                            {
                                Assets.reportError(assetContext, prefix + i + " unable to parse _UI_Requirements index from \"" + text2 + "\"");
                            }
                            else if (result < 0 || result >= conditions.Length)
                            {
                                Assets.reportError(assetContext, prefix + i + $" UI requirement index {result} out of bounds");
                            }
                            else if (result == i)
                            {
                                Assets.reportError(assetContext, prefix + i + " UI requirement depends on itself");
                            }
                            else
                            {
                                list.Add(result);
                            }
                        }
                        if (list.Count < 1)
                        {
                            list = null;
                        }
                    }
                }
                iNPCCondition.uiRequirementIndices = list;
            }
            conditions[i] = iNPCCondition;
        }
    }

    public static void readRewards(DatDictionary data, Local localization, string prefix, INPCReward[] rewards, Asset assetContext)
    {
        for (int i = 0; i < rewards.Length; i++)
        {
            string text = prefix + i + "_Type";
            if (!data.ContainsKey(text))
            {
                throw new NotSupportedException("Missing " + text);
            }
            ENPCRewardType eNPCRewardType = data.ParseEnum(text, ENPCRewardType.NONE);
            if (eNPCRewardType == ENPCRewardType.NONE)
            {
                Assets.reportError(assetContext, "{0} unknown type", text);
                continue;
            }
            string desc = localization.read(prefix + i);
            desc = ItemTool.filterRarityRichText(desc);
            INPCReward iNPCReward = null;
            switch (eNPCRewardType)
            {
            case ENPCRewardType.EXPERIENCE:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Experience reward " + prefix + i + " missing _Value");
                }
                iNPCReward = new NPCExperienceReward(data.ParseUInt32(prefix + i + "_Value"), desc);
                break;
            case ENPCRewardType.REPUTATION:
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Reputation reward " + prefix + i + " missing _Value");
                }
                iNPCReward = new NPCReputationReward(data.ParseInt32(prefix + i + "_Value"), desc);
                break;
            case ENPCRewardType.FLAG_BOOL:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Bool flag reward " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Bool flag reward " + prefix + i + " missing _Value");
                }
                iNPCReward = new NPCBoolFlagReward(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseBool(prefix + i + "_Value"), desc);
                break;
            case ENPCRewardType.FLAG_SHORT:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Short flag reward " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Value"))
                {
                    Assets.reportError(assetContext, "Short flag reward " + prefix + i + " missing _Value");
                }
                string key2 = prefix + i + "_Modification";
                if (!data.ContainsKey(key2))
                {
                    Assets.reportError(assetContext, "Short flag reward " + prefix + i + " missing _Modification");
                }
                iNPCReward = new NPCShortFlagReward(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Value", 0), data.ParseEnum(key2, ENPCModificationType.NONE), desc);
                break;
            }
            case ENPCRewardType.FLAG_SHORT_RANDOM:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Random short flag reward " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Min_Value"))
                {
                    Assets.reportError(assetContext, "Random short flag reward " + prefix + i + " missing _Min_Value");
                }
                if (!data.ContainsKey(prefix + i + "_Max_Value"))
                {
                    Assets.reportError(assetContext, "Random short flag reward " + prefix + i + " missing _Max_Value");
                }
                string key6 = prefix + i + "_Modification";
                if (!data.ContainsKey(key6))
                {
                    Assets.reportError(assetContext, "Random short flag reward " + prefix + i + " missing _Modification");
                }
                iNPCReward = new NPCRandomShortFlagReward(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseInt16(prefix + i + "_Min_Value", 0), data.ParseInt16(prefix + i + "_Max_Value", 0), data.ParseEnum(key6, ENPCModificationType.NONE), desc);
                break;
            }
            case ENPCRewardType.QUEST:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Quest reward " + prefix + i + " missing _ID");
                }
                data.ParseGuidOrLegacyId(prefix + i + "_ID", out var guid2, out var legacyId2);
                iNPCReward = new NPCQuestReward(guid2, legacyId2, desc);
                break;
            }
            case ENPCRewardType.ITEM:
            {
                string key5 = prefix + i + "_ID";
                if (!data.ContainsKey(key5))
                {
                    Assets.reportError(assetContext, "Item reward " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Amount"))
                {
                    Assets.reportError(assetContext, "Item reward " + prefix + i + " missing _Amount");
                }
                data.ParseGuidOrLegacyId(key5, out var guid, out var legacyId);
                bool newShouldAutoEquip = data.ParseBool(prefix + i + "_Auto_Equip");
                EItemOrigin origin = data.ParseEnum(prefix + i + "_Origin", EItemOrigin.CRAFT);
                iNPCReward = new NPCItemReward(guid, legacyId, data.ParseUInt8(prefix + i + "_Amount", 0), newShouldAutoEquip, data.ParseInt32(prefix + i + "_Sight", -1), data.ParseInt32(prefix + i + "_Tactical", -1), data.ParseInt32(prefix + i + "_Grip", -1), data.ParseInt32(prefix + i + "_Barrel", -1), data.ParseInt32(prefix + i + "_Magazine", -1), data.ParseInt32(prefix + i + "_Ammo", -1), origin, desc);
                break;
            }
            case ENPCRewardType.ITEM_RANDOM:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Random item reward " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Amount"))
                {
                    Assets.reportError(assetContext, "Random item reward " + prefix + i + " missing _Amount");
                }
                bool newShouldAutoEquip2 = data.ParseBool(prefix + i + "_Auto_Equip");
                EItemOrigin origin2 = data.ParseEnum(prefix + i + "_Origin", EItemOrigin.CRAFT);
                iNPCReward = new NPCRandomItemReward(data.ParseUInt16(prefix + i + "_ID", 0), data.ParseUInt8(prefix + i + "_Amount", 0), newShouldAutoEquip2, origin2, desc);
                break;
            }
            case ENPCRewardType.ACHIEVEMENT:
            {
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Achievement reward " + prefix + i + " missing _ID");
                }
                string @string = data.GetString(prefix + i + "_ID");
                if (!Provider.statusData.Achievements.canBeGrantedByNPC(@string))
                {
                    Assets.reportError(assetContext, "achievement \"{0}\" cannot be granted by NPCs", @string);
                }
                iNPCReward = new NPCAchievementReward(@string, desc);
                break;
            }
            case ENPCRewardType.VEHICLE:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Vehicle reward " + prefix + i + " missing _ID");
                }
                if (!data.ContainsKey(prefix + i + "_Spawnpoint"))
                {
                    Assets.reportError(assetContext, "Vehicle reward " + prefix + i + " missing _Spawnpoint");
                }
                iNPCReward = new NPCVehicleReward(data.ParseUInt16(prefix + i + "_ID", 0), data.GetString(prefix + i + "_Spawnpoint"), desc);
                break;
            case ENPCRewardType.TELEPORT:
                if (!data.ContainsKey(prefix + i + "_Spawnpoint"))
                {
                    Assets.reportError(assetContext, "Teleport reward " + prefix + i + " missing _Spawnpoint");
                }
                iNPCReward = new NPCTeleportReward(data.GetString(prefix + i + "_Spawnpoint"), desc);
                break;
            case ENPCRewardType.EVENT:
                if (!data.ContainsKey(prefix + i + "_ID"))
                {
                    Assets.reportError(assetContext, "Event reward " + prefix + i + " missing _ID");
                }
                iNPCReward = new NPCEventReward(data.GetString(prefix + i + "_ID"), desc);
                break;
            case ENPCRewardType.FLAG_MATH:
            {
                if (!data.ContainsKey(prefix + i + "_A_ID"))
                {
                    Assets.reportError(assetContext, "Math reward " + prefix + i + " missing _A_ID");
                }
                if (!data.ContainsKey(prefix + i + "_B_ID") && !data.ContainsKey(prefix + i + "_B_Value"))
                {
                    Assets.reportError(assetContext, "Math reward " + prefix + i + " missing _B_ID or _B_Value");
                }
                string key7 = prefix + i + "_Operation";
                if (!data.ContainsKey(key7))
                {
                    Assets.reportError(assetContext, "Math reward " + prefix + i + " missing _Operation");
                }
                iNPCReward = new NPCFlagMathReward(data.ParseUInt16(prefix + i + "_A_ID", 0), data.ParseUInt16(prefix + i + "_B_ID", 0), data.ParseInt16(prefix + i + "_B_Value", 0), data.ParseEnum(key7, ENPCOperationType.NONE), desc);
                break;
            }
            case ENPCRewardType.CURRENCY:
            {
                string key3 = prefix + i + "_GUID";
                if (!data.ContainsKey(key3))
                {
                    Assets.reportError(assetContext, "Currency reward " + prefix + i + " missing _GUID");
                }
                string key4 = prefix + i + "_Value";
                if (!data.ContainsKey(key4))
                {
                    Assets.reportError(assetContext, "Currency reward " + prefix + i + " missing _Value");
                }
                AssetReference<ItemCurrencyAsset> newCurrency = data.readAssetReference<ItemCurrencyAsset>(key3);
                uint newValue = data.ParseUInt32(key4);
                iNPCReward = new NPCCurrencyReward(newCurrency, newValue, desc);
                break;
            }
            case ENPCRewardType.HINT:
                if (string.IsNullOrEmpty(desc))
                {
                    desc = data.GetString(prefix + i + "_Text");
                }
                iNPCReward = new NPCHintReward(data.ParseFloat(prefix + i + "_Duration", 2f), desc);
                break;
            case ENPCRewardType.PLAYER_SPAWNPOINT:
                iNPCReward = new NPCPlayerSpawnpointReward(data.GetString(prefix + i + "_ID"), desc);
                break;
            case ENPCRewardType.PLAYER_LIFE_HEALTH:
            {
                string key = prefix + i + "_Value";
                if (!data.ContainsKey(key))
                {
                    Assets.reportError(assetContext, "Player life health reward " + prefix + i + " missing _Value");
                }
                iNPCReward = new NPCPlayerLifeHealthReward(data.ParseInt32(key), desc);
                break;
            }
            }
            if (iNPCReward != null)
            {
                iNPCReward.grantDelaySeconds = data.ParseFloat(prefix + i + "_GrantDelaySeconds", -1f);
            }
            rewards[i] = iNPCReward;
        }
    }
}
