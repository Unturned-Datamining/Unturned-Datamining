using System.Collections.Generic;

namespace SDG.Framework.Devkit;

public class DirtyManager
{
    protected static List<IDirtyable> _dirty = new List<IDirtyable>();

    public static HashSet<IDirtyable> _notSaveable = new HashSet<IDirtyable>();

    protected static bool isSaving;

    public static List<IDirtyable> dirty => _dirty;

    public static HashSet<IDirtyable> notSaveable => _notSaveable;

    public static event MarkedDirtyHandler markedDirty;

    public static event MarkedCleanHandler markedClean;

    public static event SaveableChangedHandler saveableChanged;

    public static event DirtySaved saved;

    public static void markDirty(IDirtyable item)
    {
        dirty.Add(item);
        triggerMarkedDirty(item);
    }

    public static void markClean(IDirtyable item)
    {
        if (!isSaving)
        {
            dirty.Remove(item);
            triggerMarkedClean(item);
        }
    }

    public static bool checkSaveable(IDirtyable item)
    {
        return !notSaveable.Contains(item);
    }

    public static void toggleSaveable(IDirtyable item)
    {
        if (!notSaveable.Remove(item))
        {
            notSaveable.Add(item);
            triggerSaveableChanged(item, isSaveable: true);
        }
        else
        {
            triggerSaveableChanged(item, isSaveable: false);
        }
    }

    public static void save()
    {
        isSaving = true;
        for (int num = dirty.Count - 1; num >= 0; num--)
        {
            IDirtyable dirtyable = dirty[num];
            if (!notSaveable.Contains(dirtyable))
            {
                dirtyable.save();
                dirtyable.isDirty = false;
                dirty.RemoveAt(num);
            }
        }
        isSaving = false;
        triggerSaved();
    }

    protected static void triggerMarkedDirty(IDirtyable item)
    {
        if (DirtyManager.markedDirty != null)
        {
            DirtyManager.markedDirty(item);
        }
    }

    protected static void triggerMarkedClean(IDirtyable item)
    {
        if (DirtyManager.markedClean != null)
        {
            DirtyManager.markedClean(item);
        }
    }

    protected static void triggerSaveableChanged(IDirtyable item, bool isSaveable)
    {
        if (DirtyManager.saveableChanged != null)
        {
            DirtyManager.saveableChanged(item, isSaveable);
        }
    }

    protected static void triggerSaved()
    {
        if (DirtyManager.saved != null)
        {
            DirtyManager.saved();
        }
    }
}
