using System.Reflection;

namespace SDG.Framework.Devkit.Transactions;

public struct TransactionFieldDelta : ITransactionDelta
{
    public FieldInfo field;

    public object before;

    public object after;

    public void undo(object instance)
    {
        field.SetValue(instance, before);
    }

    public void redo(object instance)
    {
        field.SetValue(instance, after);
    }

    public TransactionFieldDelta(FieldInfo newField)
        : this(newField, null, null)
    {
    }

    public TransactionFieldDelta(FieldInfo newField, object newBefore, object newAfter)
    {
        field = newField;
        before = newBefore;
        after = newAfter;
    }
}
