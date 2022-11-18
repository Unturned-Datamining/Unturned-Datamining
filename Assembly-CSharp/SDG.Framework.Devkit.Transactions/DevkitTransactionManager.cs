using System.Collections.Generic;
using SDG.Unturned;

namespace SDG.Framework.Devkit.Transactions;

public class DevkitTransactionManager
{
    private static uint _historyLength = 25u;

    protected static LinkedList<DevkitTransactionGroup> undoable = new LinkedList<DevkitTransactionGroup>();

    protected static Stack<DevkitTransactionGroup> redoable = new Stack<DevkitTransactionGroup>();

    protected static DevkitTransactionGroup pendingGroup;

    protected static int transactionDepth;

    public static uint historyLength
    {
        get
        {
            return _historyLength;
        }
        set
        {
            _historyLength = value;
            UnturnedLog.info("Set history_length to: " + historyLength);
        }
    }

    public static bool canUndo => undoable.Count > 0;

    public static bool canRedo => redoable.Count > 0;

    public static event DevkitTransactionPerformedHandler transactionPerformed;

    public static event DevkitTransactionsChangedHandler transactionsChanged;

    protected static void triggerTransactionPerformed(DevkitTransactionGroup group)
    {
        if (DevkitTransactionManager.transactionPerformed != null)
        {
            DevkitTransactionManager.transactionPerformed(group);
        }
    }

    protected static void triggerTransactionsChanged()
    {
        if (DevkitTransactionManager.transactionsChanged != null)
        {
            DevkitTransactionManager.transactionsChanged();
        }
    }

    public static IEnumerable<DevkitTransactionGroup> getUndoable()
    {
        return undoable;
    }

    public static IEnumerable<DevkitTransactionGroup> getRedoable()
    {
        return redoable;
    }

    public static DevkitTransactionGroup undo()
    {
        if (!canUndo)
        {
            return null;
        }
        DevkitTransactionGroup devkitTransactionGroup = popUndo();
        devkitTransactionGroup.undo();
        pushRedo(devkitTransactionGroup);
        triggerTransactionPerformed(devkitTransactionGroup);
        return devkitTransactionGroup;
    }

    public static DevkitTransactionGroup redo()
    {
        if (!canRedo)
        {
            return null;
        }
        DevkitTransactionGroup devkitTransactionGroup = popRedo();
        devkitTransactionGroup.redo();
        pushUndo(devkitTransactionGroup);
        triggerTransactionPerformed(devkitTransactionGroup);
        return devkitTransactionGroup;
    }

    public static void beginTransaction(string name)
    {
        if (transactionDepth == 0)
        {
            clearRedo();
            pendingGroup = new DevkitTransactionGroup(name);
        }
        transactionDepth++;
    }

    public static void recordTransaction(IDevkitTransaction transaction)
    {
        if (pendingGroup != null)
        {
            pendingGroup.record(transaction);
        }
    }

    public static void endTransaction()
    {
        if (transactionDepth == 0)
        {
            return;
        }
        transactionDepth--;
        if (transactionDepth == 0)
        {
            pendingGroup.end();
            if (pendingGroup.delta)
            {
                pushUndo(pendingGroup);
            }
            else
            {
                pendingGroup.forget();
            }
            pendingGroup = null;
            triggerTransactionsChanged();
        }
    }

    public static void resetTransactions()
    {
        clearUndo();
        clearRedo();
        pendingGroup = null;
        transactionDepth = 0;
    }

    protected static void pushUndo(DevkitTransactionGroup group)
    {
        if (undoable.Count >= historyLength)
        {
            undoable.First.Value.forget();
            undoable.RemoveFirst();
        }
        undoable.AddLast(group);
    }

    protected static DevkitTransactionGroup popUndo()
    {
        DevkitTransactionGroup value = undoable.Last.Value;
        undoable.RemoveLast();
        return value;
    }

    protected static void clearUndo()
    {
        while (undoable.Count > 0)
        {
            DevkitTransactionGroup value = undoable.Last.Value;
            undoable.RemoveLast();
            value.forget();
        }
        undoable.Clear();
    }

    protected static void pushRedo(DevkitTransactionGroup group)
    {
        redoable.Push(group);
    }

    protected static DevkitTransactionGroup popRedo()
    {
        return redoable.Pop();
    }

    protected static void clearRedo()
    {
        while (redoable.Count > 0)
        {
            redoable.Pop().forget();
        }
        redoable.Clear();
    }
}
