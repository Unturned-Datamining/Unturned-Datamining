using System;
using System.Reflection;
using UnityEngine;

namespace SDG.Unturned;

public class SteamChannelMethod
{
    public enum EContextType
    {
        None,
        Client,
        Server
    }

    public int typesReadOffset;

    public EContextType contextType;

    /// <summary>
    /// Index of the context parameter, if not None.
    /// </summary>
    public int contextParameterIndex;

    public Component component { get; protected set; }

    public MethodInfo method { get; protected set; }

    public string legacyMethodName { get; protected set; }

    public Type[] types { get; protected set; }

    /// <summary>
    /// Reflected attribute that was used to find this method.
    /// Contains extra information about how to call it.
    /// </summary>
    public SteamCall attribute { get; protected set; }

    public SteamChannelMethod(Component newComponent, MethodInfo newMethod, string legacyMethodName, Type[] newTypes, int typesReadOffset, EContextType contextType, int contextParameterIndex, SteamCall attribute)
    {
        component = newComponent;
        method = newMethod;
        this.legacyMethodName = legacyMethodName;
        types = newTypes;
        this.typesReadOffset = typesReadOffset;
        this.contextType = contextType;
        this.contextParameterIndex = contextParameterIndex;
        this.attribute = attribute;
    }
}
