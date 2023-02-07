using System;
using System.Collections.Generic;
using System.IO;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.IO.FormattedFiles.KeyValueTables;
using SDG.Framework.Modules;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class LevelHierarchy : IModuleNexus, IDirtyable
{
    private static uint availableInstanceID;

    protected bool _isDirty;

    internal bool loadedAnyDevkitObjects;

    public static LevelHierarchy instance { get; protected set; }

    public List<IDevkitHierarchyItem> items { get; protected set; }

    public bool isDirty
    {
        get
        {
            return _isDirty;
        }
        set
        {
            if (isDirty != value)
            {
                _isDirty = value;
                if (isDirty)
                {
                    DirtyManager.markDirty(this);
                }
                else
                {
                    DirtyManager.markClean(this);
                }
            }
        }
    }

    public static event LevelHiearchyItemAdded itemAdded;

    public static event LevelHierarchyItemRemoved itemRemoved;

    public static event LevelHierarchyLoaded loaded;

    public static event LevelHierarchyReady ready;

    public static void MarkDirty()
    {
        if (instance != null)
        {
            instance.isDirty = true;
        }
    }

    public static uint generateUniqueInstanceID()
    {
        return availableInstanceID++;
    }

    public static void initItem(IDevkitHierarchyItem item)
    {
        if (item.instanceID == 0)
        {
            item.instanceID = generateUniqueInstanceID();
        }
        if (instance != null)
        {
            instance.isDirty = true;
        }
    }

    public static void addItem(IDevkitHierarchyItem item)
    {
        instance.items.Add(item);
        triggerItemAdded(item);
    }

    public static void removeItem(IDevkitHierarchyItem item)
    {
        instance.items.Remove(item);
        triggerItemRemoved(item);
    }

    protected static void triggerItemAdded(IDevkitHierarchyItem item)
    {
        LevelHierarchy.itemAdded?.Invoke(item);
    }

    protected static void triggerItemRemoved(IDevkitHierarchyItem item)
    {
        if (!Level.isExiting)
        {
            LevelHierarchy.itemRemoved?.Invoke(item);
        }
    }

    protected static void triggerLoaded()
    {
        LevelHierarchy.loaded?.Invoke();
    }

    protected static void triggerReady()
    {
        LevelHierarchy.ready?.Invoke();
    }

    public void load()
    {
        loadedAnyDevkitObjects = false;
        string path = Level.info.path + "/Level.hierarchy";
        if (File.Exists(path))
        {
            using FileStream underlyingStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using SHA1Stream sHA1Stream = new SHA1Stream(underlyingStream);
            using StreamReader input = new StreamReader(sHA1Stream);
            IFormattedFileReader reader = new KeyValueTableReader(input);
            read(reader);
            byte[] hash = sHA1Stream.Hash;
            Level.includeHash("Level.hierarchy", hash);
        }
        if (loadedAnyDevkitObjects)
        {
            UnturnedLog.info("Marking level dirty because devkit objects were converted");
            MarkDirty();
        }
        triggerLoaded();
        TimeUtility.updated += handleLoadUpdated;
    }

    public void save()
    {
        string path = Level.info.path + "/Level.hierarchy";
        string directoryName = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        using StreamWriter writer = new StreamWriter(path);
        IFormattedFileWriter writer2 = new KeyValueTableWriter(writer);
        write(writer2);
    }

    public virtual void read(IFormattedFileReader reader)
    {
        if (reader.containsKey("Available_Instance_ID"))
        {
            availableInstanceID = reader.readValue<uint>("Available_Instance_ID");
        }
        else
        {
            availableInstanceID = 1u;
        }
        int num = reader.readArrayLength("Items");
        for (int i = 0; i < num; i++)
        {
            IFormattedFileReader formattedFileReader = reader.readObject(i);
            Type type = formattedFileReader.readValue<Type>("Type");
            if (type == null)
            {
                UnturnedLog.error("Level hierarchy item index " + i + " missing type: " + formattedFileReader.readValue("Type"));
                continue;
            }
            IDevkitHierarchyItem devkitHierarchyItem = ((!typeof(MonoBehaviour).IsAssignableFrom(type)) ? (Activator.CreateInstance(type) as IDevkitHierarchyItem) : (new GameObject
            {
                name = type.Name
            }.AddComponent(type) as IDevkitHierarchyItem));
            if (devkitHierarchyItem != null)
            {
                if (formattedFileReader.containsKey("Instance_ID"))
                {
                    devkitHierarchyItem.instanceID = formattedFileReader.readValue<uint>("Instance_ID");
                }
                if (devkitHierarchyItem.instanceID == 0)
                {
                    devkitHierarchyItem.instanceID = generateUniqueInstanceID();
                }
                formattedFileReader.readKey("Item");
                devkitHierarchyItem.read(formattedFileReader);
            }
        }
    }

    public virtual void write(IFormattedFileWriter writer)
    {
        writer.writeValue("Available_Instance_ID", availableInstanceID);
        writer.beginArray("Items");
        for (int i = 0; i < items.Count; i++)
        {
            IDevkitHierarchyItem devkitHierarchyItem = items[i];
            if (devkitHierarchyItem.ShouldSave)
            {
                writer.beginObject();
                writer.writeValue("Type", devkitHierarchyItem.GetType());
                writer.writeValue("Instance_ID", devkitHierarchyItem.instanceID);
                writer.writeValue("Item", devkitHierarchyItem);
                writer.endObject();
            }
        }
        writer.endArray();
    }

    public void initialize()
    {
        instance = this;
        items = new List<IDevkitHierarchyItem>();
        Level.loadingSteps += handleLoadingStep;
        DevkitTransactionManager.transactionsChanged += handleTransactionsChanged;
    }

    public void shutdown()
    {
        Level.loadingSteps -= handleLoadingStep;
        DevkitTransactionManager.transactionsChanged -= handleTransactionsChanged;
    }

    protected void handleLoadingStep()
    {
        items.Clear();
        load();
    }

    protected void handleLoadUpdated()
    {
        TimeUtility.updated -= handleLoadUpdated;
        triggerReady();
    }

    protected void handleTransactionsChanged()
    {
        if (Level.isEditor)
        {
            isDirty = true;
        }
    }
}
