namespace SDG.Framework.Devkit.Transactions;

public interface ITransactionDelta
{
    void undo(object instance);

    void redo(object instance);
}
