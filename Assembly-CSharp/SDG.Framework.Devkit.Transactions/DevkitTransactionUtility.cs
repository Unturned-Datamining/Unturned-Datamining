using UnityEngine;

namespace SDG.Framework.Devkit.Transactions;

public class DevkitTransactionUtility
{
    public static void beginGenericTransaction()
    {
        DevkitTransactionManager.beginTransaction("Generic");
    }

    public static void endGenericTransaction()
    {
        DevkitTransactionManager.endTransaction();
    }

    public static void recordInstantiation(GameObject go)
    {
        DevkitTransactionManager.recordTransaction(new DevkitGameObjectInstantiationTransaction(go));
    }

    public static void recordObjectDelta(object instance)
    {
        DevkitTransactionManager.recordTransaction(new DevkitObjectDeltaTransaction(instance));
    }

    public static void recordDestruction(GameObject go)
    {
        DevkitTransactionManager.recordTransaction(new DevkitGameObjectDestructionTransaction(go));
    }

    public static void recordTransformChangeParent(Transform transform, Transform parent)
    {
        DevkitTransactionManager.recordTransaction(new DevkitTransformChangeParentTransaction(transform, parent));
    }
}
