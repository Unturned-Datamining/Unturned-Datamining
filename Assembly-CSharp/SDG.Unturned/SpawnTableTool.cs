using System;
using UnityEngine;

namespace SDG.Unturned;

public class SpawnTableTool
{
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

    internal static ushort resolve(Guid guid)
    {
        if (!(Assets.find(guid) is SpawnAsset spawnAsset))
        {
            if ((bool)Assets.shouldLoadAnyAssets)
            {
                Guid guid2 = guid;
                UnturnedLog.error("Unable to find spawn table for resolve with guid " + guid2.ToString());
            }
            return 0;
        }
        spawnAsset.resolve(out var id, out var isSpawn);
        if (isSpawn)
        {
            return resolve(id);
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
}
