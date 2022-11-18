namespace SDG.Framework.Devkit.Transactions;

public interface IDevkitTransaction
{
    bool delta { get; }

    void undo();

    void redo();

    void begin();

    void end();

    void forget();
}
