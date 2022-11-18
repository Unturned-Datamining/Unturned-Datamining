using UnityEngine;

namespace SDG.Framework.Devkit.Transactions;

public class DevkitGameObjectInstantiationTransaction : IDevkitTransaction
{
    protected GameObject go;

    protected bool isActive;

    public bool delta => true;

    public void undo()
    {
        if (go != null)
        {
            go.SetActive(value: false);
        }
        isActive = false;
    }

    public void redo()
    {
        if (go != null)
        {
            go.SetActive(value: true);
        }
        isActive = true;
    }

    public void begin()
    {
        if (go != null)
        {
            go.SetActive(value: true);
        }
    }

    public void end()
    {
    }

    public void forget()
    {
        if (go != null && !isActive)
        {
            Object.Destroy(go);
        }
    }

    public DevkitGameObjectInstantiationTransaction(GameObject newGO)
    {
        go = newGO;
        isActive = true;
    }
}
