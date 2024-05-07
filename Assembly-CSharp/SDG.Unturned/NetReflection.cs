using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SDG.NetPak;

namespace SDG.Unturned;

public static class NetReflection
{
    private struct GeneratedMethod
    {
        public MethodInfo info;

        public NetInvokableGeneratedMethodAttribute attribute;
    }

    internal static List<ClientMethodInfo> clientMethods;

    internal static uint clientMethodsLength;

    internal static int clientMethodsBitCount;

    internal static List<ServerMethodInfo> serverMethods;

    internal static uint serverMethodsLength;

    internal static int serverMethodsBitCount;

    /// <summary>
    /// Number of server methods with rate limits.
    /// </summary>
    internal static int rateLimitedMethodsCount;

    private static List<string> pendingMessages;

    private static Action<string> logCallback;

    /// <summary>
    /// Log all known net methods.
    /// </summary>
    public static void Dump()
    {
        Log($"{clientMethods.Count} client methods ({clientMethodsBitCount} bits):");
        for (int i = 0; i < clientMethods.Count; i++)
        {
            ClientMethodInfo arg = clientMethods[i];
            Log($"{i} {arg}");
        }
        Log($"{serverMethods.Count} server methods ({serverMethodsBitCount} bits):");
        for (int j = 0; j < serverMethods.Count; j++)
        {
            ServerMethodInfo arg2 = serverMethods[j];
            Log($"{j} {arg2}");
        }
    }

    public static void SetLogCallback(Action<string> logCallback)
    {
        NetReflection.logCallback = logCallback;
        if (pendingMessages == null)
        {
            return;
        }
        foreach (string pendingMessage in pendingMessages)
        {
            logCallback(pendingMessage);
        }
        pendingMessages = null;
    }

    internal static ClientMethodInfo GetClientMethodInfo(Type declaringType, string methodName)
    {
        foreach (ClientMethodInfo clientMethod in clientMethods)
        {
            if (clientMethod.declaringType == declaringType && clientMethod.name.Equals(methodName, StringComparison.Ordinal))
            {
                return clientMethod;
            }
        }
        Log("Unable to find client method info for " + declaringType.Name + "." + methodName);
        return null;
    }

    internal static ServerMethodInfo GetServerMethodInfo(Type declaringType, string methodName)
    {
        foreach (ServerMethodInfo serverMethod in serverMethods)
        {
            if (serverMethod.declaringType == declaringType && serverMethod.name.Equals(methodName, StringComparison.Ordinal))
            {
                return serverMethod;
            }
        }
        Log("Unable to find server method info for " + declaringType.Name + "." + methodName);
        return null;
    }

    private static bool FindAndRemoveGeneratedMethod(List<GeneratedMethod> generatedMethods, string methodName, out GeneratedMethod foundMethod)
    {
        for (int num = generatedMethods.Count - 1; num >= 0; num--)
        {
            GeneratedMethod generatedMethod = generatedMethods[num];
            if (generatedMethod.attribute.targetMethodName == methodName)
            {
                generatedMethods.RemoveAtFast(num);
                foundMethod = generatedMethod;
                return true;
            }
        }
        foundMethod = default(GeneratedMethod);
        return false;
    }

    private static ClientMethodReceive FindClientReceiveMethod(Type generatedType, List<GeneratedMethod> generatedMethods, string methodName)
    {
        if (FindAndRemoveGeneratedMethod(generatedMethods, methodName, out var foundMethod))
        {
            try
            {
                return (ClientMethodReceive)foundMethod.info.CreateDelegate(typeof(ClientMethodReceive));
            }
            catch
            {
                Log("Exception creating delegate for client " + generatedType.Name + "." + methodName + " receive implementation");
                return null;
            }
        }
        Log("Unable to find client " + generatedType.Name + "." + methodName + " receive implementation");
        return null;
    }

    internal static T CreateClientWriteDelegate<T>(ClientMethodInfo clientMethod) where T : Delegate
    {
        try
        {
            return clientMethod.writeMethodInfo.CreateDelegate(typeof(T)) as T;
        }
        catch
        {
            Log($"Exception creating delegate for client {clientMethod} write");
            return null;
        }
    }

    private static ServerMethodReceive FindServerReceiveMethod(Type generatedType, List<GeneratedMethod> generatedMethods, string methodName)
    {
        if (FindAndRemoveGeneratedMethod(generatedMethods, methodName, out var foundMethod))
        {
            try
            {
                return (ServerMethodReceive)foundMethod.info.CreateDelegate(typeof(ServerMethodReceive));
            }
            catch
            {
                Log("Exception creating delegate for server " + generatedType.Name + "." + methodName + " receive implementation");
                return null;
            }
        }
        Log("Unable to find server " + generatedType.Name + "." + methodName + " receive implementation");
        return null;
    }

    internal static T CreateServerWriteDelegate<T>(ServerMethodInfo serverMethod) where T : Delegate
    {
        try
        {
            return serverMethod.writeMethodInfo.CreateDelegate(typeof(T)) as T;
        }
        catch
        {
            Log($"Exception creating delegate for server {serverMethod} write");
            return null;
        }
    }

    /// <summary>
    /// This class gets used from type initializers, so Unity's built-in log is not an option unfortunately.
    /// </summary>
    private static void Log(string message)
    {
        if (logCallback != null)
        {
            logCallback(message);
            return;
        }
        pendingMessages = new List<string>();
        pendingMessages.Add(message);
    }

    /// <summary>
    /// Not *really* supported but *might* probably work. Adding for public discussion #4176.
    /// </summary>
    public static void RegisterFromAssembly(Assembly assembly)
    {
        List<GeneratedMethod> list = new List<GeneratedMethod>();
        List<GeneratedMethod> list2 = new List<GeneratedMethod>();
        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (!type.IsClass || !type.IsAbstract)
            {
                continue;
            }
            NetInvokableGeneratedClassAttribute customAttribute = type.GetCustomAttribute<NetInvokableGeneratedClassAttribute>();
            if (customAttribute == null)
            {
                continue;
            }
            list.Clear();
            list2.Clear();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                NetInvokableGeneratedMethodAttribute customAttribute2 = methodInfo.GetCustomAttribute<NetInvokableGeneratedMethodAttribute>();
                if (customAttribute2 != null)
                {
                    GeneratedMethod item = default(GeneratedMethod);
                    item.info = methodInfo;
                    item.attribute = customAttribute2;
                    switch (customAttribute2.purpose)
                    {
                    case ENetInvokableGeneratedMethodPurpose.Read:
                        list.Add(item);
                        break;
                    case ENetInvokableGeneratedMethodPurpose.Write:
                        list2.Add(item);
                        break;
                    default:
                        Log($"Generated method {type.Name}.{methodInfo.Name} unknown purpose {customAttribute2.purpose}");
                        break;
                    }
                }
            }
            methods = customAttribute.targetType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo methodInfo2 in methods)
            {
                SteamCall customAttribute3 = methodInfo2.GetCustomAttribute<SteamCall>();
                if (customAttribute3 == null)
                {
                    continue;
                }
                ParameterInfo[] parameters = methodInfo2.GetParameters();
                if (customAttribute3.validation == ESteamCallValidation.ONLY_FROM_SERVER)
                {
                    ClientMethodInfo clientMethodInfo = new ClientMethodInfo();
                    clientMethodInfo.declaringType = methodInfo2.DeclaringType;
                    clientMethodInfo.debugName = $"{methodInfo2.DeclaringType}.{methodInfo2.Name}";
                    clientMethodInfo.name = methodInfo2.Name;
                    clientMethodInfo.customAttribute = customAttribute3;
                    bool flag = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ClientInvocationContext);
                    if (methodInfo2.IsStatic && flag)
                    {
                        clientMethodInfo.readMethod = Delegate.CreateDelegate(typeof(ClientMethodReceive), methodInfo2, throwOnBindFailure: false) as ClientMethodReceive;
                    }
                    else
                    {
                        clientMethodInfo.readMethod = FindClientReceiveMethod(type, list, methodInfo2.Name);
                        if (!flag)
                        {
                            if (FindAndRemoveGeneratedMethod(list2, methodInfo2.Name, out var foundMethod))
                            {
                                clientMethodInfo.writeMethodInfo = foundMethod.info;
                            }
                            else
                            {
                                Log("Unable to find client " + type.Name + "." + methodInfo2.Name + " write implementation");
                            }
                        }
                    }
                    clientMethodInfo.methodIndex = (uint)clientMethods.Count;
                    clientMethods.Add(clientMethodInfo);
                }
                else
                {
                    if (customAttribute3.validation != ESteamCallValidation.SERVERSIDE && customAttribute3.validation != ESteamCallValidation.ONLY_FROM_OWNER)
                    {
                        continue;
                    }
                    ServerMethodInfo serverMethodInfo = new ServerMethodInfo();
                    serverMethodInfo.declaringType = methodInfo2.DeclaringType;
                    serverMethodInfo.name = methodInfo2.Name;
                    serverMethodInfo.debugName = $"{methodInfo2.DeclaringType}.{methodInfo2.Name}";
                    serverMethodInfo.customAttribute = customAttribute3;
                    bool flag2 = parameters.Length == 1 && parameters[0].ParameterType.GetElementType() == typeof(ServerInvocationContext);
                    if (methodInfo2.IsStatic && flag2)
                    {
                        serverMethodInfo.readMethod = Delegate.CreateDelegate(typeof(ServerMethodReceive), methodInfo2, throwOnBindFailure: false) as ServerMethodReceive;
                    }
                    else
                    {
                        serverMethodInfo.readMethod = FindServerReceiveMethod(type, list, methodInfo2.Name);
                        if (!flag2)
                        {
                            if (FindAndRemoveGeneratedMethod(list2, methodInfo2.Name, out var foundMethod2))
                            {
                                serverMethodInfo.writeMethodInfo = foundMethod2.info;
                            }
                            else
                            {
                                Log("Unable to find server " + type.Name + "." + methodInfo2.Name + " write implementation");
                            }
                        }
                    }
                    if (customAttribute3.ratelimitHz > 0)
                    {
                        serverMethodInfo.rateLimitIndex = rateLimitedMethodsCount;
                        customAttribute3.rateLimitIndex = rateLimitedMethodsCount;
                        customAttribute3.ratelimitSeconds = 1f / (float)customAttribute3.ratelimitHz;
                        rateLimitedMethodsCount++;
                    }
                    else
                    {
                        serverMethodInfo.rateLimitIndex = -1;
                    }
                    serverMethodInfo.methodIndex = (uint)serverMethods.Count;
                    serverMethods.Add(serverMethodInfo);
                }
            }
            foreach (GeneratedMethod item2 in list)
            {
                Log("Generated read method " + type.Name + "." + item2.info.Name + " not used");
            }
            foreach (GeneratedMethod item3 in list2)
            {
                Log("Generated write method " + type.Name + "." + item3.info.Name + " not used");
            }
        }
        clientMethodsLength = (uint)clientMethods.Count;
        clientMethodsBitCount = NetPakConst.CountBits(clientMethodsLength);
        serverMethodsLength = (uint)serverMethods.Count;
        serverMethodsBitCount = NetPakConst.CountBits(serverMethodsLength);
    }

    static NetReflection()
    {
        clientMethods = new List<ClientMethodInfo>();
        serverMethods = new List<ServerMethodInfo>();
        rateLimitedMethodsCount = 0;
        Stopwatch stopwatch = Stopwatch.StartNew();
        RegisterFromAssembly(Assembly.GetExecutingAssembly());
        stopwatch.Stop();
        Log($"Reflect net invokables: {stopwatch.ElapsedMilliseconds}ms");
    }
}
