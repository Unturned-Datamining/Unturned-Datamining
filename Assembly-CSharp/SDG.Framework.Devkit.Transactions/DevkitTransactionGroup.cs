using System.Collections.Generic;

namespace SDG.Framework.Devkit.Transactions;

public class DevkitTransactionGroup
{
    public string name { get; protected set; }

    public List<IDevkitTransaction> transactions { get; protected set; }

    public bool delta
    {
        get
        {
            for (int num = transactions.Count - 1; num >= 0; num--)
            {
                if (!transactions[num].delta)
                {
                    transactions.RemoveAt(num);
                }
            }
            return transactions.Count > 0;
        }
    }

    public void record(IDevkitTransaction transaction)
    {
        transaction.begin();
        transactions.Add(transaction);
    }

    public void undo()
    {
        for (int i = 0; i < transactions.Count; i++)
        {
            transactions[i].undo();
        }
    }

    public void redo()
    {
        for (int i = 0; i < transactions.Count; i++)
        {
            transactions[i].redo();
        }
    }

    public void end()
    {
        for (int i = 0; i < transactions.Count; i++)
        {
            transactions[i].end();
        }
    }

    public void forget()
    {
        for (int i = 0; i < transactions.Count; i++)
        {
            transactions[i].forget();
        }
    }

    public DevkitTransactionGroup(string newName)
    {
        name = newName;
        transactions = new List<IDevkitTransaction>();
    }
}
