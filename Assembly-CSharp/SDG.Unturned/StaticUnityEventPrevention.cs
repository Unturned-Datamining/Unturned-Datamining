using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

public static class StaticUnityEventPrevention
{
    private class TypeInfo
    {
        public FieldInfo[] unityEventFields;
    }

    private static List<MonoBehaviour> components = new List<MonoBehaviour>();

    private static Dictionary<Type, TypeInfo> cachedTypeInfo = new Dictionary<Type, TypeInfo>();

    private static List<FieldInfo> tempFields = new List<FieldInfo>();

    /// <summary>
    /// Check gameObject (and children) components for any unity events calling static methods (exploitable).
    /// </summary>
    /// <returns>True if nothing was found.</returns>
    public static bool Validate(GameObject gameObject)
    {
        bool result = true;
        components.Clear();
        gameObject.GetComponentsInChildren(includeInactive: true, components);
        foreach (MonoBehaviour component in components)
        {
            Type type = component.GetType();
            TypeInfo typeInfo = GetTypeInfo(type);
            if (typeInfo.unityEventFields == null)
            {
                continue;
            }
            bool flag = false;
            FieldInfo[] unityEventFields = typeInfo.unityEventFields;
            foreach (FieldInfo fieldInfo in unityEventFields)
            {
                if (!(fieldInfo.GetValue(component) is UnityEventBase unityEventBase))
                {
                    continue;
                }
                int num = 0;
                while (num < unityEventBase.GetPersistentEventCount())
                {
                    if (!(unityEventBase.GetPersistentTarget(num) == null))
                    {
                        num++;
                        continue;
                    }
                    goto IL_008c;
                }
                continue;
                IL_008c:
                flag = true;
                UnturnedLog.warn($"Found call to static method in {component.GetSceneHierarchyPath()} {type} {fieldInfo.Name}, deleting component");
                break;
            }
            if (flag)
            {
                UnityEngine.Object.DestroyImmediate(component, allowDestroyingAssets: true);
                result = false;
            }
        }
        return result;
    }

    private static TypeInfo GetTypeInfo(Type type)
    {
        if (cachedTypeInfo.TryGetValue(type, out var value))
        {
            return value;
        }
        value = new TypeInfo();
        tempFields.Clear();
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo fieldInfo in fields)
        {
            if (typeof(UnityEventBase).IsAssignableFrom(fieldInfo.FieldType))
            {
                tempFields.Add(fieldInfo);
            }
        }
        if (tempFields.Count > 0)
        {
            value.unityEventFields = tempFields.ToArray();
        }
        return value;
    }
}
