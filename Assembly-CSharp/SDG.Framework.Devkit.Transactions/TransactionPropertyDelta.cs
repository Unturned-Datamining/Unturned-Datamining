using System.Reflection;

namespace SDG.Framework.Devkit.Transactions;

public struct TransactionPropertyDelta : ITransactionDelta
{
    public PropertyInfo property;

    public object before;

    public object after;

    public void undo(object instance)
    {
        property.SetValue(instance, before, null);
    }

    public void redo(object instance)
    {
        property.SetValue(instance, after, null);
    }

    public TransactionPropertyDelta(PropertyInfo newProperty)
        : this(newProperty, null, null)
    {
    }

    public TransactionPropertyDelta(PropertyInfo newProperty, object newBefore, object newAfter)
    {
        property = newProperty;
        before = newBefore;
        after = newAfter;
    }
}
