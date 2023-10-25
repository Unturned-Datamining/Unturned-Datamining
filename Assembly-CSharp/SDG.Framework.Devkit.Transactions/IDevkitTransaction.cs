namespace SDG.Framework.Devkit.Transactions;

public interface IDevkitTransaction
{
    /// <summary>
    /// If false this transaction is ignored. If there were no changes at all in the group it's discarded.
    /// </summary>
    bool delta { get; }

    void undo();

    void redo();

    void begin();

    void end();

    /// <summary>
    /// Called when history buffer is too long so this transaction is discarded.
    /// </summary>
    void forget();
}
