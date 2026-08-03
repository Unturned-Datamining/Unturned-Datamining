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
            if (component == null)
            {
                continue;
            }
            Type type = component.GetType();
            TypeInfo typeInfo = GetTypeInfo(type);
            if (typeInfo.unityEventFields == null)
            {
                continue;
            }
            FieldInfo[] unityEventFields = typeInfo.unityEventFields;
            foreach (FieldInfo fieldInfo in unityEventFields)
            {
                if (!(fieldInfo.GetValue(component) is UnityEventBase unityEventBase))
                {
                    continue;
                }
                for (int j = 0; j < unityEventBase.GetPersistentEventCount(); j++)
                {
                    UnityEngine.Object persistentTarget = unityEventBase.GetPersistentTarget(j);
                    string persistentMethodName = unityEventBase.GetPersistentMethodName(j);
                    if (persistentTarget == null && !string.IsNullOrEmpty(persistentMethodName))
                    {
                        if ((bool)Assets.shouldValidateAssets && (Assets.currentAsset != null || Assets.currentMasterBundle != null))
                        {
                            UnturnedLog.warn($"Found call to method \"{persistentMethodName}\" without target in {component.GetSceneHierarchyPath()} {type} {fieldInfo.Name} (Asset: {Assets.currentAsset?.FriendlyNameWithFriendlyType} Bundle: {Assets.currentMasterBundle?.assetBundleName}), deactivating (may be attempting to call a static method, or target is unassigned)");
                        }
                        unityEventBase.SetPersistentListenerState(j, UnityEventCallState.Off);
                        result = false;
                    }
                }
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
        cachedTypeInfo.Add(type, value);
        return value;
    }
}
