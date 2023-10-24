using System;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class SpawnTableTool
{
    public static Asset Resolve(SpawnAsset spawnAsset, EAssetType legacyTargetAssetType, Func<string> errorContextCallback)
    {
        if (spawnAsset == null)
        {
            if ((bool)Assets.shouldLoadAnyAssets)
            {
                UnturnedLog.error((errorContextCallback?.Invoke() ?? "Unknown") + " attempted to resolve null spawn table");
            }
            return null;
        }
        for (int i = 0; i < 32; i++)
        {
            SpawnTable spawnTable = spawnAsset.PickRandomEntry(errorContextCallback);
            if (spawnTable == null)
            {
                UnturnedLog.warn("Spawn table \"" + spawnAsset.name + "\" from " + spawnAsset.GetOriginName() + " resolved by " + (errorContextCallback?.Invoke() ?? "Unknown") + " returned null entry");
                return null;
            }
            if (spawnTable.legacySpawnId != 0)
            {
                if (!(Assets.find(EAssetType.SPAWN, spawnTable.legacySpawnId) is SpawnAsset spawnAsset2))
                {
                    UnturnedLog.warn(string.Format("Spawn table \"{0}\" from {1} resolved by {2} unable to find table matching legacy spawn ID {3}", spawnAsset.name, spawnAsset.GetOriginName(), errorContextCallback?.Invoke() ?? "Unknown", spawnTable.legacySpawnId));
                    return null;
                }
                spawnAsset = spawnAsset2;
                continue;
            }
            if (spawnTable.legacyAssetId != 0)
            {
                Asset asset = Assets.find(legacyTargetAssetType, spawnTable.legacyAssetId);
                if (asset == null)
                {
                    UnturnedLog.warn(string.Format("Spawn table \"{0}\" from {1} resolved by {2} unable to find asset matching legacy ID {3}", spawnAsset.name, spawnAsset.GetOriginName(), errorContextCallback?.Invoke() ?? "Unknown", spawnTable.legacyAssetId));
                    return null;
                }
                return asset;
            }
            if (!spawnTable.targetGuid.IsEmpty())
            {
                Asset asset2 = Assets.find(spawnTable.targetGuid);
                if (asset2 == null)
                {
                    UnturnedLog.warn(string.Format("Spawn table \"{0}\" from {1} resolved by {2} unable to find asset matching GUID {3}", spawnAsset.name, spawnAsset.GetOriginName(), errorContextCallback?.Invoke() ?? "Unknown", spawnTable.targetGuid));
                    return null;
                }
                if (asset2 is SpawnAsset spawnAsset3)
                {
                    spawnAsset = spawnAsset3;
                    continue;
                }
                return asset2;
            }
            return null;
        }
        UnturnedLog.warn("Spawn table \"" + spawnAsset.name + "\" from " + spawnAsset.GetOriginName() + " resolved by " + (errorContextCallback?.Invoke() ?? "Unknown") + " may have encountered a recursive loop and has given up");
        return null;
    }

    public static Asset Resolve(Guid spawnAssetGuid, EAssetType legacyTargetAssetType, Func<string> errorContextCallback)
    {
        if (spawnAssetGuid.IsEmpty())
        {
            return null;
        }
        if (!(Assets.find(spawnAssetGuid) is SpawnAsset spawnAsset))
        {
            if ((bool)Assets.shouldLoadAnyAssets)
            {
                UnturnedLog.error(string.Format("Unable to find spawn table with guid {0} resolved by {1}", spawnAssetGuid, errorContextCallback?.Invoke() ?? "Unknown"));
            }
            return null;
        }
        return Resolve(spawnAsset, legacyTargetAssetType, errorContextCallback);
    }

    public static Asset Resolve(ushort spawnAssetLegacyId, EAssetType legacyTargetAssetType, Func<string> errorContextCallback)
    {
        if (spawnAssetLegacyId == 0)
        {
            return null;
        }
        if (!(Assets.find(EAssetType.SPAWN, spawnAssetLegacyId) is SpawnAsset spawnAsset))
        {
            if ((bool)Assets.shouldLoadAnyAssets)
            {
                UnturnedLog.error(string.Format("Unable to find spawn table with legacy ID {0} resolved by {1}", spawnAssetLegacyId, errorContextCallback?.Invoke() ?? "Unknown"));
            }
            return null;
        }
        return Resolve(spawnAsset, legacyTargetAssetType, errorContextCallback);
    }

    public static ushort ResolveLegacyId(SpawnAsset spawnAsset, EAssetType legacyTargetAssetType, Func<string> errorContextCallback)
    {
        return Resolve(spawnAsset, legacyTargetAssetType, errorContextCallback)?.id ?? 0;
    }

    public static ushort ResolveLegacyId(Guid spawnAssetGuid, EAssetType legacyTargetAssetType, Func<string> errorContextCallback)
    {
        return Resolve(spawnAssetGuid, legacyTargetAssetType, errorContextCallback)?.id ?? 0;
    }

    public static ushort ResolveLegacyId(ushort spawnAssetLegacyId, EAssetType legacyTargetAssetType, Func<string> errorContextCallback)
    {
        return Resolve(spawnAssetLegacyId, legacyTargetAssetType, errorContextCallback)?.id ?? 0;
    }

    [Obsolete("Please update to the newer Resolve methods with legacyTargetAssetType parameter which support GUIDs")]
    public static ushort resolve(ushort id)
    {
        if (!(Assets.find(EAssetType.SPAWN, id) is SpawnAsset spawnAsset))
        {
            if ((bool)Assets.shouldLoadAnyAssets)
            {
                UnturnedLog.error("Unable to find spawn table for resolve with id " + id);
            }
            return 0;
        }
        spawnAsset.resolve(out id, out var isSpawn);
        if (isSpawn)
        {
            id = resolve(id);
        }
        return id;
    }

    private static bool isVariantItemTier(ItemTier tier)
    {
        if (tier.table.Count < 6)
        {
            return false;
        }
        if (!(Assets.find(EAssetType.ITEM, tier.table[0].item) is ItemAsset itemAsset))
        {
            return false;
        }
        int num = itemAsset.itemName.IndexOf(" ");
        if (num <= 0)
        {
            return false;
        }
        string text = itemAsset.itemName.Substring(num + 1);
        if (text.Length <= 1)
        {
            UnturnedLog.error(itemAsset.itemName + " name has a trailing space!");
            return false;
        }
        for (int i = 1; i < tier.table.Count; i++)
        {
            if (!(Assets.find(EAssetType.ITEM, tier.table[i].item) as ItemAsset).itemName.Contains(text))
            {
                return false;
            }
        }
        tier.name = text;
        return true;
    }

    private static bool isVariantVehicleTier(VehicleTier tier)
    {
        if (tier.table.Count < 6)
        {
            return false;
        }
        if (!(Assets.find(EAssetType.VEHICLE, tier.table[0].vehicle) is VehicleAsset vehicleAsset))
        {
            return false;
        }
        int num = vehicleAsset.vehicleName.IndexOf(" ");
        if (num <= 0)
        {
            return false;
        }
        string text = vehicleAsset.vehicleName.Substring(num + 1);
        if (text.Length <= 1)
        {
            UnturnedLog.error(vehicleAsset.vehicleName + " name has a trailing space!");
            return false;
        }
        for (int i = 1; i < tier.table.Count; i++)
        {
            if (!(Assets.find(EAssetType.VEHICLE, tier.table[i].vehicle) as VehicleAsset).vehicleName.Contains(text))
            {
                return false;
            }
        }
        tier.name = text;
        return true;
    }

    private static void exportItems(string path, Data spawnsData, ref ushort id, bool isLegacy)
    {
        for (int i = 0; i < LevelItems.tables.Count; i++)
        {
            ItemTable itemTable = LevelItems.tables[i];
            if (itemTable.tableID != 0)
            {
                continue;
            }
            itemTable.tableID = id;
            spawnsData.writeString(id.ToString(), Level.info.name + "_" + itemTable.name);
            Data data = new Data();
            data.writeString("Type", "Spawn");
            data.writeUInt16("ID", id++);
            if (ReadWrite.fileExists("/Bundles/Spawns/Items/" + itemTable.name + "/" + itemTable.name + ".dat", useCloud: false, usePath: true))
            {
                Data data2 = ReadWrite.readData("/Bundles/Spawns/Items/" + itemTable.name + "/" + itemTable.name + ".dat", useCloud: false, usePath: true);
                data.writeInt32("Tables", 1);
                data.writeUInt16("Table_0_Spawn_ID", data2.readUInt16("ID", 0));
                data.writeInt32("Table_0_Weight", 100);
            }
            else
            {
                data.writeInt32("Tables", 1);
                data.writeUInt16("Table_0_Spawn_ID", id);
                data.writeInt32("Table_0_Weight", 100);
                spawnsData.writeString(id.ToString(), itemTable.name);
                Data data3 = new Data();
                data3.writeString("Type", "Spawn");
                data3.writeUInt16("ID", id++);
                if (isLegacy)
                {
                    if (itemTable.tiers.Count > 1)
                    {
                        float num = float.MaxValue;
                        for (int j = 0; j < itemTable.tiers.Count; j++)
                        {
                            ItemTier itemTier = itemTable.tiers[j];
                            if (itemTier.chance < num)
                            {
                                num = itemTier.chance;
                            }
                        }
                        int num2 = Mathf.CeilToInt(10f / num);
                        data3.writeInt32("Tables", itemTable.tiers.Count);
                        for (int k = 0; k < itemTable.tiers.Count; k++)
                        {
                            ItemTier itemTier2 = itemTable.tiers[k];
                            bool flag = isVariantItemTier(itemTier2);
                            if (flag && ReadWrite.fileExists("/Bundles/Spawns/Items/" + itemTier2.name + "/" + itemTier2.name + ".dat", useCloud: false, usePath: true))
                            {
                                Data data4 = ReadWrite.readData("/Bundles/Spawns/Items/" + itemTier2.name + "/" + itemTier2.name + ".dat", useCloud: false, usePath: true);
                                data3.writeUInt16("Table_" + k + "_Spawn_ID", data4.readUInt16("ID", 0));
                                data3.writeInt32("Table_" + k + "_Weight", (int)(itemTier2.chance * (float)num2));
                                continue;
                            }
                            if (flag && ReadWrite.fileExists(path + "/Items/" + itemTier2.name + "/" + itemTier2.name + ".dat", useCloud: false, usePath: false))
                            {
                                Data data5 = ReadWrite.readData(path + "/Items/" + itemTier2.name + "/" + itemTier2.name + ".dat", useCloud: false, usePath: false);
                                data3.writeUInt16("Table_" + k + "_Spawn_ID", data5.readUInt16("ID", 0));
                                data3.writeInt32("Table_" + k + "_Weight", (int)(itemTier2.chance * (float)num2));
                                continue;
                            }
                            data3.writeUInt16("Table_" + k + "_Spawn_ID", id);
                            data3.writeInt32("Table_" + k + "_Weight", (int)(itemTier2.chance * (float)num2));
                            if (flag)
                            {
                                spawnsData.writeString(id.ToString(), itemTier2.name);
                            }
                            else
                            {
                                spawnsData.writeString(id.ToString(), itemTable.name + "_" + itemTier2.name);
                            }
                            Data data6 = new Data();
                            data6.writeString("Type", "Spawn");
                            data6.writeUInt16("ID", id++);
                            data6.writeInt32("Tables", itemTier2.table.Count);
                            for (int l = 0; l < itemTier2.table.Count; l++)
                            {
                                ItemSpawn itemSpawn = itemTier2.table[l];
                                data6.writeUInt16("Table_" + l + "_Asset_ID", itemSpawn.item);
                                data6.writeInt32("Table_" + l + "_Weight", 10);
                            }
                            if (flag)
                            {
                                ReadWrite.writeData(path + "/Items/" + itemTier2.name + "/" + itemTier2.name + ".dat", useCloud: false, usePath: false, data6);
                            }
                            else
                            {
                                ReadWrite.writeData(path + "/Items/" + itemTable.name + "_" + itemTier2.name + "/" + itemTable.name + "_" + itemTier2.name + ".dat", useCloud: false, usePath: false, data6);
                            }
                        }
                    }
                    else
                    {
                        ItemTier itemTier3 = itemTable.tiers[0];
                        data3.writeInt32("Tables", itemTier3.table.Count);
                        for (int m = 0; m < itemTier3.table.Count; m++)
                        {
                            ItemSpawn itemSpawn2 = itemTier3.table[m];
                            data3.writeUInt16("Table_" + m + "_Asset_ID", itemSpawn2.item);
                            data3.writeInt32("Table_" + m + "_Weight", 10);
                        }
                    }
                }
                ReadWrite.writeData(path + "/Items/" + itemTable.name + "/" + itemTable.name + ".dat", useCloud: false, usePath: false, data3);
            }
            ReadWrite.writeData(path + "/Items/" + Level.info.name + "_" + itemTable.name + "/" + Level.info.name + "_" + itemTable.name + ".dat", useCloud: false, usePath: false, data);
        }
    }

    private static void exportVehicles(string path, Data spawnsData, ref ushort id, bool isLegacy)
    {
        for (int i = 0; i < LevelVehicles.tables.Count; i++)
        {
            VehicleTable vehicleTable = LevelVehicles.tables[i];
            if (vehicleTable.tableID != 0)
            {
                continue;
            }
            vehicleTable.tableID = id;
            spawnsData.writeString(id.ToString(), Level.info.name + "_" + vehicleTable.name);
            Data data = new Data();
            data.writeString("Type", "Spawn");
            data.writeUInt16("ID", id++);
            if (ReadWrite.fileExists("/Bundles/Spawns/Vehicles/" + vehicleTable.name + "/" + vehicleTable.name + ".dat", useCloud: false, usePath: true))
            {
                Data data2 = ReadWrite.readData("/Bundles/Spawns/Vehicles/" + vehicleTable.name + "/" + vehicleTable.name + ".dat", useCloud: false, usePath: true);
                data.writeInt32("Tables", 1);
                data.writeUInt16("Table_0_Spawn_ID", data2.readUInt16("ID", 0));
                data.writeInt32("Table_0_Weight", 100);
            }
            else
            {
                data.writeInt32("Tables", 1);
                data.writeUInt16("Table_0_Spawn_ID", id);
                data.writeInt32("Table_0_Weight", 100);
                spawnsData.writeString(id.ToString(), vehicleTable.name);
                Data data3 = new Data();
                data3.writeString("Type", "Spawn");
                data3.writeUInt16("ID", id++);
                if (isLegacy)
                {
                    if (vehicleTable.tiers.Count > 1)
                    {
                        float num = float.MaxValue;
                        for (int j = 0; j < vehicleTable.tiers.Count; j++)
                        {
                            VehicleTier vehicleTier = vehicleTable.tiers[j];
                            if (vehicleTier.chance < num)
                            {
                                num = vehicleTier.chance;
                            }
                        }
                        int num2 = Mathf.CeilToInt(10f / num);
                        data3.writeInt32("Tables", vehicleTable.tiers.Count);
                        for (int k = 0; k < vehicleTable.tiers.Count; k++)
                        {
                            VehicleTier vehicleTier2 = vehicleTable.tiers[k];
                            bool flag = isVariantVehicleTier(vehicleTier2);
                            if (flag && ReadWrite.fileExists("/Bundles/Spawns/Vehicles/" + vehicleTier2.name + "/" + vehicleTier2.name + ".dat", useCloud: false, usePath: true))
                            {
                                Data data4 = ReadWrite.readData("/Bundles/Spawns/Vehicles/" + vehicleTier2.name + "/" + vehicleTier2.name + ".dat", useCloud: false, usePath: true);
                                data3.writeUInt16("Table_" + k + "_Spawn_ID", data4.readUInt16("ID", 0));
                                data3.writeInt32("Table_" + k + "_Weight", (int)(vehicleTier2.chance * (float)num2));
                                continue;
                            }
                            if (flag && ReadWrite.fileExists(path + "/Vehicles/" + vehicleTier2.name + "/" + vehicleTier2.name + ".dat", useCloud: false, usePath: false))
                            {
                                Data data5 = ReadWrite.readData(path + "/Vehicles/" + vehicleTier2.name + "/" + vehicleTier2.name + ".dat", useCloud: false, usePath: false);
                                data3.writeUInt16("Table_" + k + "_Spawn_ID", data5.readUInt16("ID", 0));
                                data3.writeInt32("Table_" + k + "_Weight", (int)(vehicleTier2.chance * (float)num2));
                                continue;
                            }
                            data3.writeUInt16("Table_" + k + "_Spawn_ID", id);
                            data3.writeInt32("Table_" + k + "_Weight", (int)(vehicleTier2.chance * (float)num2));
                            if (flag)
                            {
                                spawnsData.writeString(id.ToString(), vehicleTier2.name);
                            }
                            else
                            {
                                spawnsData.writeString(id.ToString(), vehicleTable.name + "_" + vehicleTier2.name);
                            }
                            Data data6 = new Data();
                            data6.writeString("Type", "Spawn");
                            data6.writeUInt16("ID", id++);
                            data6.writeInt32("Tables", vehicleTier2.table.Count);
                            for (int l = 0; l < vehicleTier2.table.Count; l++)
                            {
                                VehicleSpawn vehicleSpawn = vehicleTier2.table[l];
                                data6.writeUInt16("Table_" + l + "_Asset_ID", vehicleSpawn.vehicle);
                                data6.writeInt32("Table_" + l + "_Weight", 10);
                            }
                            if (flag)
                            {
                                ReadWrite.writeData(path + "/Vehicles/" + vehicleTier2.name + "/" + vehicleTier2.name + ".dat", useCloud: false, usePath: false, data6);
                            }
                            else
                            {
                                ReadWrite.writeData(path + "/Vehicles/" + vehicleTable.name + "_" + vehicleTier2.name + "/" + vehicleTable.name + "_" + vehicleTier2.name + ".dat", useCloud: false, usePath: false, data6);
                            }
                        }
                    }
                    else
                    {
                        VehicleTier vehicleTier3 = vehicleTable.tiers[0];
                        data3.writeInt32("Tables", vehicleTier3.table.Count);
                        for (int m = 0; m < vehicleTier3.table.Count; m++)
                        {
                            VehicleSpawn vehicleSpawn2 = vehicleTier3.table[m];
                            data3.writeUInt16("Table_" + m + "_Asset_ID", vehicleSpawn2.vehicle);
                            data3.writeInt32("Table_" + m + "_Weight", 10);
                        }
                    }
                }
                ReadWrite.writeData(path + "/Vehicles/" + vehicleTable.name + "/" + vehicleTable.name + ".dat", useCloud: false, usePath: false, data3);
            }
            ReadWrite.writeData(path + "/Vehicles/" + Level.info.name + "_" + vehicleTable.name + "/" + Level.info.name + "_" + vehicleTable.name + ".dat", useCloud: false, usePath: false, data);
        }
    }

    private static void exportZombies(string path, Data spawnsData, ref ushort id, bool isLegacy)
    {
        for (int i = 0; i < LevelZombies.tables.Count; i++)
        {
            ZombieTable zombieTable = LevelZombies.tables[i];
            if (zombieTable.lootID == 0 && zombieTable.lootIndex < LevelItems.tables.Count)
            {
                zombieTable.lootID = LevelItems.tables[zombieTable.lootIndex].tableID;
            }
        }
    }

    private static void exportAnimals(string path, Data spawnsData, ref ushort id, bool isLegacy)
    {
        for (int i = 0; i < LevelAnimals.tables.Count; i++)
        {
            AnimalTable animalTable = LevelAnimals.tables[i];
            if (animalTable.tableID != 0)
            {
                continue;
            }
            animalTable.tableID = id;
            spawnsData.writeString(id.ToString(), Level.info.name + "_" + animalTable.name);
            Data data = new Data();
            data.writeString("Type", "Spawn");
            data.writeUInt16("ID", id++);
            if (ReadWrite.fileExists("/Bundles/Spawns/Animals/" + animalTable.name + "/" + animalTable.name + ".dat", useCloud: false, usePath: true))
            {
                Data data2 = ReadWrite.readData("/Bundles/Spawns/Animals/" + animalTable.name + "/" + animalTable.name + ".dat", useCloud: false, usePath: true);
                data.writeInt32("Tables", 1);
                data.writeUInt16("Table_0_Spawn_ID", data2.readUInt16("ID", 0));
                data.writeInt32("Table_0_Weight", 100);
            }
            else
            {
                data.writeInt32("Tables", 1);
                data.writeUInt16("Table_0_Spawn_ID", id);
                data.writeInt32("Table_0_Weight", 100);
                spawnsData.writeString(id.ToString(), animalTable.name);
                Data data3 = new Data();
                data3.writeString("Type", "Spawn");
                data3.writeUInt16("ID", id++);
                if (isLegacy)
                {
                    if (animalTable.tiers.Count > 1)
                    {
                        float num = float.MaxValue;
                        for (int j = 0; j < animalTable.tiers.Count; j++)
                        {
                            AnimalTier animalTier = animalTable.tiers[j];
                            if (animalTier.chance < num)
                            {
                                num = animalTier.chance;
                            }
                        }
                        int num2 = Mathf.CeilToInt(10f / num);
                        data3.writeInt32("Tables", animalTable.tiers.Count);
                        for (int k = 0; k < animalTable.tiers.Count; k++)
                        {
                            AnimalTier animalTier2 = animalTable.tiers[k];
                            data3.writeUInt16("Table_" + k + "_Spawn_ID", id);
                            data3.writeInt32("Table_" + k + "_Weight", (int)(animalTier2.chance * (float)num2));
                            spawnsData.writeString(id.ToString(), animalTable.name + "_" + animalTier2.name);
                            Data data4 = new Data();
                            data4.writeString("Type", "Spawn");
                            data4.writeUInt16("ID", id++);
                            data4.writeInt32("Tables", animalTier2.table.Count);
                            for (int l = 0; l < animalTier2.table.Count; l++)
                            {
                                AnimalSpawn animalSpawn = animalTier2.table[l];
                                data4.writeUInt16("Table_" + l + "_Asset_ID", animalSpawn.animal);
                                data4.writeInt32("Table_" + l + "_Weight", 10);
                            }
                            ReadWrite.writeData(path + "/Animals/" + animalTable.name + "_" + animalTier2.name + "/" + animalTable.name + "_" + animalTier2.name + ".dat", useCloud: false, usePath: false, data4);
                        }
                    }
                    else
                    {
                        AnimalTier animalTier3 = animalTable.tiers[0];
                        data3.writeInt32("Tables", animalTier3.table.Count);
                        for (int m = 0; m < animalTier3.table.Count; m++)
                        {
                            AnimalSpawn animalSpawn2 = animalTier3.table[m];
                            data3.writeUInt16("Table_" + m + "_Asset_ID", animalSpawn2.animal);
                            data3.writeInt32("Table_" + m + "_Weight", 10);
                        }
                    }
                }
                ReadWrite.writeData(path + "/Animals/" + animalTable.name + "/" + animalTable.name + ".dat", useCloud: false, usePath: false, data3);
            }
            ReadWrite.writeData(path + "/Animals/" + Level.info.name + "_" + animalTable.name + "/" + Level.info.name + "_" + animalTable.name + ".dat", useCloud: false, usePath: false, data);
        }
    }

    public static void export(ushort id, bool isLegacy)
    {
        string path = Level.info.path;
        path = ((!isLegacy) ? (path + "/Exported_Proxy_Spawn_Tables") : (path + "/Exported_Legacy_Spawn_Tables"));
        if (ReadWrite.folderExists(path, usePath: false))
        {
            ReadWrite.deleteFolder(path, usePath: false);
        }
        Data data = new Data();
        data.writeString("ID", "Spawn");
        exportItems(path, data, ref id, isLegacy);
        exportVehicles(path, data, ref id, isLegacy);
        exportZombies(path, data, ref id, isLegacy);
        exportAnimals(path, data, ref id, isLegacy);
        data.isCSV = true;
        ReadWrite.writeData(path + "/IDs.csv", useCloud: false, usePath: false, data);
    }

    public static void LogAllSpawnTables()
    {
        List<SpawnAsset> list = new List<SpawnAsset>(1000);
        Assets.find(list);
        UnturnedLog.info($"Dumping {list.Count} spawn tables:");
        for (int i = 0; i < list.Count; i++)
        {
            SpawnAsset spawnAsset = list[i];
            if (spawnAsset == null)
            {
                UnturnedLog.error("null entry in spawnAssets list???");
                continue;
            }
            if (spawnAsset.tables == null || spawnAsset.tables.Count < 1)
            {
                UnturnedLog.info($"[{i + 1} of {list.Count}] {spawnAsset.name} is empty");
                continue;
            }
            UnturnedLog.info($"[{i + 1} of {list.Count}] {spawnAsset.name} has {spawnAsset.tables.Count} children:");
            for (int j = 0; j < spawnAsset.tables.Count; j++)
            {
                SpawnTable spawnTable = spawnAsset.tables[j];
                string text;
                if (spawnTable.legacySpawnId != 0)
                {
                    text = ((Assets.find(EAssetType.SPAWN, spawnTable.legacySpawnId) as SpawnAsset)?.name ?? $"Unknown ID {spawnTable.legacySpawnId}") + " (Spawn)";
                }
                else if (spawnTable.legacyAssetId != 0)
                {
                    ItemAsset obj = Assets.find(EAssetType.ITEM, spawnTable.legacyAssetId) as ItemAsset;
                    VehicleAsset vehicleAsset = Assets.find(EAssetType.VEHICLE, spawnTable.legacyAssetId) as VehicleAsset;
                    AnimalAsset animalAsset = Assets.find(EAssetType.ANIMAL, spawnTable.legacyAssetId) as AnimalAsset;
                    string text2 = obj?.FriendlyName ?? $"Unknown ID {spawnTable.legacyAssetId}";
                    string text3 = vehicleAsset?.FriendlyName ?? $"Unknown ID {spawnTable.legacyAssetId}";
                    string text4 = animalAsset?.FriendlyName ?? $"Unknown ID {spawnTable.legacyAssetId}";
                    text = text2 + " (Item) or " + text3 + " (Vehicle) or " + text4 + " (Animal) depending on context";
                }
                else if (!spawnTable.targetGuid.IsEmpty())
                {
                    Asset asset = Assets.find(spawnTable.targetGuid);
                    text = ((asset == null) ? $"Unknown GUID {spawnTable.targetGuid}" : (asset.FriendlyName + " (" + asset.GetTypeNameWithoutSuffix() + ")"));
                }
                else
                {
                    text = "Empty";
                }
                float num = spawnTable.normalizedWeight;
                if (j > 0)
                {
                    num -= spawnAsset.tables[j - 1].normalizedWeight;
                }
                UnturnedLog.info($"[{i + 1} of {list.Count}][{j + 1} of {spawnAsset.tables.Count}] {num:P} {text}");
            }
        }
    }
}
