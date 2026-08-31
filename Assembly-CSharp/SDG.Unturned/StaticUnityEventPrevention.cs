using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SDG.Unturned;

public static class StaticUnityEventPrevention
{
    private class TypeInfo
    {
        public FieldInfo[] unityEventFields;
    }

    private static List<MonoBehaviour> components;

    private static Dictionary<Type, TypeInfo> cachedTypeInfo;

    private static List<FieldInfo> tempFields;

    private static FieldInfo m_PersistentCalls;

    private static MethodInfo GetListener;

    private static MethodInfo FindMethod;

    private static object[] oneArgument;

    private static FieldInfo m_TargetAssemblyTypeName;

    /// <summary>
    /// Check gameObject (and children) components for any unity events calling static methods (exploitable).
    /// </summary>
    /// <returns>True if nothing was found.</returns>
    public static bool Validate(GameObject gameObject)
    {
        bool flag = true;
        components.Clear();
        gameObject.GetComponentsInChildren(includeInactive: true, components);
        foreach (MonoBehaviour component in components)
        {
            if (component == null)
            {
                continue;
            }
            Type type = component.GetType();
            if (typeof(EventTrigger).IsAssignableFrom(type))
            {
                EventTrigger eventTrigger = component as EventTrigger;
                if (eventTrigger != null)
                {
                    bool flag2 = ValidateEventTrigger(eventTrigger);
                    flag = flag && flag2;
                    continue;
                }
            }
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
                object value = m_PersistentCalls.GetValue(unityEventBase);
                for (int j = 0; j < unityEventBase.GetPersistentEventCount(); j++)
                {
                    if (!ValidateUnityEvent(unityEventBase, value, j, out var reason))
                    {
                        if (!string.IsNullOrEmpty(reason) && (bool)Assets.shouldValidateAssets && (Assets.currentAsset != null || Assets.currentMasterBundle != null))
                        {
                            UnturnedLog.warn($"Deactivating UnityEvent {component.GetSceneHierarchyPath()} {type} {fieldInfo.Name} Reason: {reason} (Asset: {Assets.currentAsset?.FriendlyNameWithFriendlyType} Bundle: {Assets.currentMasterBundle?.assetBundleName})");
                        }
                        unityEventBase.SetPersistentListenerState(j, UnityEventCallState.Off);
                        flag = false;
                    }
                }
            }
        }
        return flag;
    }

    private static bool ValidateEventTrigger(EventTrigger eventTrigger)
    {
        bool result = true;
        foreach (EventTrigger.Entry trigger in eventTrigger.triggers)
        {
            UnityEventBase callback = trigger.callback;
            if (callback == null)
            {
                continue;
            }
            object value = m_PersistentCalls.GetValue(callback);
            for (int i = 0; i < callback.GetPersistentEventCount(); i++)
            {
                if (!ValidateUnityEvent(callback, value, i, out var reason))
                {
                    if (!string.IsNullOrEmpty(reason) && (bool)Assets.shouldValidateAssets && (Assets.currentAsset != null || Assets.currentMasterBundle != null))
                    {
                        UnturnedLog.warn($"Deactivating UnityEvent {eventTrigger.GetSceneHierarchyPath()} EventTrigger {trigger.eventID} Reason: {reason} (Asset: {Assets.currentAsset?.FriendlyNameWithFriendlyType} Bundle: {Assets.currentMasterBundle?.assetBundleName})");
                    }
                    callback.SetPersistentListenerState(i, UnityEventCallState.Off);
                    result = false;
                }
            }
        }
        return result;
    }

    private static bool IsTypeAllowed(Type type)
    {
        if (!typeof(Component).IsAssignableFrom(type) && !(type == typeof(Transform)) && !(type == typeof(GameObject)))
        {
            return type == typeof(Material);
        }
        return true;
    }

    private static bool ValidateUnityEvent(UnityEventBase unityEvent, object persistentCallGroup, int index, out string reason)
    {
        try
        {
            UnityEngine.Object persistentTarget = unityEvent.GetPersistentTarget(index);
            if (persistentTarget == null)
            {
                reason = "null target object";
                return false;
            }
            string persistentMethodName = unityEvent.GetPersistentMethodName(index);
            if (string.IsNullOrEmpty(persistentMethodName))
            {
                reason = "empty method name";
                return false;
            }
            Type type = persistentTarget.GetType();
            if (!IsTypeAllowed(type))
            {
                reason = $"target type {type} is not allowed (if valid, please open an issue)";
                return false;
            }
            oneArgument[0] = index;
            object obj = GetListener.Invoke(persistentCallGroup, oneArgument);
            if (obj == null)
            {
                reason = "null persistent call (shouldn't happen?)";
                return false;
            }
            string text = m_TargetAssemblyTypeName.GetValue(obj) as string;
            if (!string.IsNullOrEmpty(text))
            {
                Type type2 = Type.GetType(text, throwOnError: false);
                if (type2 == null)
                {
                    reason = "unable to resolve target type \"" + text + "\"";
                    return false;
                }
                if (!IsTypeAllowed(type2))
                {
                    reason = $"serialized target type {type2} is not allowed (if valid, please open an issue)";
                    return false;
                }
            }
            oneArgument[0] = obj;
            MethodInfo methodInfo = FindMethod.Invoke(unityEvent, oneArgument) as MethodInfo;
            if (methodInfo == null)
            {
                reason = "unable to find target method \"" + persistentMethodName + "\"";
                return false;
            }
            if (methodInfo.IsStatic)
            {
                reason = $"target method is static ({methodInfo})";
                return false;
            }
            reason = null;
            return true;
        }
        catch
        {
            reason = "threw an exception";
            return false;
        }
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

    static StaticUnityEventPrevention()
    {
        components = new List<MonoBehaviour>();
        cachedTypeInfo = new Dictionary<Type, TypeInfo>();
        tempFields = new List<FieldInfo>();
        m_PersistentCalls = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic);
        GetListener = Type.GetType("UnityEngine.Events.PersistentCallGroup, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", throwOnError: true).GetMethod("GetListener", new Type[1] { typeof(int) });
        oneArgument = new object[1];
        Type type = Type.GetType("UnityEngine.Events.PersistentCall, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        m_TargetAssemblyTypeName = type.GetField("m_TargetAssemblyTypeName", BindingFlags.Instance | BindingFlags.NonPublic);
        FindMethod = typeof(UnityEventBase).GetMethod("FindMethod", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[1] { type }, null);
    }
}
