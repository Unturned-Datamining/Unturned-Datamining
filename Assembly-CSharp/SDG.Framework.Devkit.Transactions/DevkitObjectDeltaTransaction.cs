using System;
using System.Collections.Generic;
using System.Reflection;
using SDG.Framework.Utilities;
using SDG.Unturned;

namespace SDG.Framework.Devkit.Transactions;

public class DevkitObjectDeltaTransaction : IDevkitTransaction
{
    protected object instance;

    protected List<object> tempFields;

    protected List<object> tempProperties;

    protected List<ITransactionDelta> deltas;

    public bool delta => deltas.Count > 0;

    public void undo()
    {
        for (int i = 0; i < deltas.Count; i++)
        {
            deltas[i].undo(instance);
        }
    }

    public void redo()
    {
        for (int i = 0; i < deltas.Count; i++)
        {
            deltas[i].redo(instance);
        }
    }

    public void begin()
    {
        tempFields = ListPool<object>.claim();
        tempProperties = ListPool<object>.claim();
        Type type = instance.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        for (int i = 0; i < fields.Length; i++)
        {
            try
            {
                object value = fields[i].GetValue(instance);
                tempFields.Add(value);
            }
            catch
            {
                tempFields.Add(null);
            }
        }
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        for (int j = 0; j < properties.Length; j++)
        {
            try
            {
                PropertyInfo propertyInfo = properties[j];
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    object value2 = propertyInfo.GetValue(instance, null);
                    tempProperties.Add(value2);
                }
                else
                {
                    tempProperties.Add(null);
                }
            }
            catch
            {
                tempProperties.Add(null);
            }
        }
    }

    public void end()
    {
        deltas = ListPool<ITransactionDelta>.claim();
        Type type = instance.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        for (int i = 0; i < fields.Length; i++)
        {
            try
            {
                FieldInfo fieldInfo = fields[i];
                object value = fieldInfo.GetValue(instance);
                if (changed(tempFields[i], value))
                {
                    deltas.Add(new TransactionFieldDelta(fieldInfo, tempFields[i], value));
                }
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e);
            }
        }
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        for (int j = 0; j < properties.Length; j++)
        {
            try
            {
                PropertyInfo propertyInfo = properties[j];
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    object value2 = propertyInfo.GetValue(instance, null);
                    if (changed(tempProperties[j], value2))
                    {
                        deltas.Add(new TransactionPropertyDelta(propertyInfo, tempProperties[j], value2));
                    }
                }
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2);
            }
        }
        ListPool<object>.release(tempFields);
        ListPool<object>.release(tempProperties);
    }

    public void forget()
    {
        if (deltas != null)
        {
            ListPool<ITransactionDelta>.release(deltas);
            deltas = null;
        }
    }

    protected bool changed(object before, object after)
    {
        if (before == null || after == null)
        {
            return before != after;
        }
        return !before.Equals(after);
    }

    public DevkitObjectDeltaTransaction(object newInstance)
    {
        instance = newInstance;
    }
}
