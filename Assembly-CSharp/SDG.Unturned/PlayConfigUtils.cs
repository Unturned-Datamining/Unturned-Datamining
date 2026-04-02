using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal static class PlayConfigUtils
{
    private static ConfigData configDefaults = ConfigData.CreateDefault(singleplayer: false);

    /// <summary>
    /// Each generated comment line is prefixed with this string.
    /// </summary>
    public const string COMMENT_PREFIX = "> ";

    private static List<string> generatedLines = new List<string>();

    private static List<string> tempParsedLines = new List<string>();

    private static StringBuilder commentStringBuilder = new StringBuilder();

    private static string GetModeFileName(EGameMode mode)
    {
        return mode switch
        {
            EGameMode.EASY => "EasyDifficulty", 
            EGameMode.NORMAL => "NormalDifficulty", 
            EGameMode.HARD => "HardDifficulty", 
            _ => throw new NotImplementedException(mode.ToString()), 
        };
    }

    /// <summary>
    /// Format absolute path to newer txt (UnturnedDat) config file.
    /// </summary>
    public static string GetSingleplayerConfigPathV2(int characterSlot, EGameMode singleplayerMode)
    {
        string modeFileName = GetModeFileName(singleplayerMode);
        return PathEx.Join(UnturnedPaths.RootDirectory, "Worlds", $"Singleplayer_{characterSlot}", "Config_" + modeFileName + ".txt");
    }

    /// <summary>
    /// Format absolute path to older json serialized config file.
    /// </summary>
    public static string GetSingleplayerConfigPathV1(int characterSlot)
    {
        return PathEx.Join(UnturnedPaths.RootDirectory, "Worlds", $"Singleplayer_{characterSlot}", "Config.json");
    }

    /// <summary>
    /// Config path used for new servers.
    /// </summary>
    public static string GetServerConfigPathV2(string serverId)
    {
        return PathEx.Join(UnturnedPaths.RootDirectory, "Servers", serverId, "Config.txt");
    }

    /// <summary>
    /// Config path used for conversion from Config.json.
    /// </summary>
    public static string GetServerConfigPathV2(string serverId, EGameMode serverMode)
    {
        string modeFileName = GetModeFileName(serverMode);
        return PathEx.Join(UnturnedPaths.RootDirectory, "Servers", serverId, "Config_" + modeFileName + ".txt");
    }

    public static string GetFieldPath(FieldInfo field)
    {
        string text = field.DeclaringType.Name;
        if (text.EndsWith("ConfigData", StringComparison.Ordinal))
        {
            text = text.Substring(0, text.Length - "ConfigData".Length);
        }
        return text + "." + field.Name;
    }

    /// <summary>
    /// Fill server-related sections of config from dat file.
    /// </summary>
    public static void ParseServerConfig(IDatDictionary rootDictionary, ConfigData config)
    {
        if (rootDictionary.TryGetDictionary("Browser", out var node))
        {
            ParseCategory(node, config.Browser, null);
        }
        if (rootDictionary.TryGetDictionary("Server", out var node2))
        {
            ParseCategory(node2, config.Server, null);
        }
        if (rootDictionary.TryGetDictionary("UnityEvents", out var node3))
        {
            ParseCategory(node3, config.UnityEvents, null);
        }
    }

    /// <summary>
    /// Fill mode-related sections of config from dat file and gather overrides.
    /// (for servers and singleplayer)
    /// </summary>
    public static void ParseModeConfig(IDatDictionary rootDictionary, ModeConfigData config, Dictionary<FieldInfo, object> overrides)
    {
        FieldInfo[] fields = typeof(ModeConfigData).GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            if (rootDictionary.TryGetDictionary(fieldInfo.Name, out var node))
            {
                object value = fieldInfo.GetValue(config);
                ParseCategory(node, value, overrides);
            }
        }
    }

    /// <summary>
    /// Parses dictionary keys according to reflected fields in targetObject.
    /// If overrides is valid, gathers which values were set. (used for mode config)
    /// </summary>
    private static void ParseCategory(IDatDictionary dictionary, object targetObject, Dictionary<FieldInfo, object> overrides)
    {
        FieldInfo[] fields = targetObject.GetType().GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            if (!dictionary.TryGetNode(fieldInfo.Name, out var node))
            {
                continue;
            }
            if (fieldInfo.FieldType.IsArray)
            {
                if (node is IDatList listNode)
                {
                    if (TryParseArrayField(fieldInfo, listNode, out var overrideValues))
                    {
                        fieldInfo.SetValue(targetObject, overrideValues);
                        overrides?.Add(fieldInfo, overrideValues);
                    }
                }
                else if (node is IDatValue valueNode)
                {
                    if (!valueNode.IsValueNullOrEmpty())
                    {
                        node.TryGetParsedLineNumber(out var lineNumber);
                        CommandWindow.LogWarning($"Server config: expected {fieldInfo.Name} on line {lineNumber} to be a List, but found a Value");
                    }
                }
                else if (node is IDatDictionary)
                {
                    node.TryGetParsedLineNumber(out var lineNumber2);
                    CommandWindow.LogWarning($"Server config: expected {fieldInfo.Name} on line {lineNumber2} to be a List, but found a Dictionary");
                }
            }
            else if (node is IDatValue datValue)
            {
                if (!string.IsNullOrEmpty(datValue.Value) && TryParseValueField(fieldInfo, datValue, out var overrideValue))
                {
                    fieldInfo.SetValue(targetObject, overrideValue);
                    overrides?.Add(fieldInfo, overrideValue);
                }
            }
            else
            {
                node.TryGetParsedLineNumber(out var lineNumber3);
                CommandWindow.LogWarning($"Server config: expected {fieldInfo.Name} on line {lineNumber3} to be a Value, but found {node.NodeType}");
            }
        }
    }

    /// <summary>
    /// Attempt to parse user-supplied value from dat file according to field's reflected type.
    /// </summary>
    private static bool TryParseValueField(FieldInfo fieldInfo, IDatValue valueNode, out object overrideValue)
    {
        Type fieldType = fieldInfo.FieldType;
        if (fieldType == typeof(bool))
        {
            if (valueNode.TryParseBool(out var value))
            {
                overrideValue = value;
                return true;
            }
            CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as bool (true/false) from \"{valueNode.Value}\"");
        }
        else if (fieldType == typeof(float))
        {
            if (valueNode.TryParseFloat(out var value2))
            {
                overrideValue = value2;
                return true;
            }
            CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as decimal number from \"{valueNode.Value}\"");
        }
        else if (fieldType == typeof(int))
        {
            if (valueNode.TryParseInt32(out var value3))
            {
                overrideValue = value3;
                return true;
            }
            CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as integer number from \"{valueNode.Value}\"");
        }
        else if (fieldType == typeof(uint))
        {
            if (valueNode.TryParseUInt32(out var value4))
            {
                overrideValue = value4;
                return true;
            }
            CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as non-negative integer number from \"{valueNode.Value}\"");
        }
        else
        {
            if (fieldType == typeof(string))
            {
                overrideValue = valueNode.Value;
                return true;
            }
            if (!fieldType.IsEnum)
            {
                throw new NotImplementedException(fieldType.ToString());
            }
            if (valueNode.TryParseEnum(fieldType, out overrideValue))
            {
                return true;
            }
            CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {valueNode.GetParsedLineNumber()} as {fieldType.Name} from \"{valueNode.Value}\"");
        }
        overrideValue = null;
        return false;
    }

    /// <summary>
    /// Attempt to parse user-supplied list from dat file according to field's reflected type.
    /// </summary>
    private static bool TryParseArrayField(FieldInfo fieldInfo, IDatList listNode, out Array overrideValues)
    {
        Type elementType = fieldInfo.FieldType.GetElementType();
        Array array = Array.CreateInstance(elementType, listNode.Count);
        if (elementType == typeof(string))
        {
            int num = -1;
            for (int i = 0; i < listNode.Count; i++)
            {
                IDatNode datNode = listNode[i];
                if (!(datNode is IDatValue datValue))
                {
                    CommandWindow.LogWarning($"Server config: expected {fieldInfo.Name} on line {datNode.GetParsedLineNumber()} to be a Value, but found a {datNode.NodeType}");
                    continue;
                }
                num++;
                array.SetValue(datValue.Value, num);
            }
            int num2 = num + 1;
            overrideValues = Array.CreateInstance(elementType, num2);
            if (num2 > 0)
            {
                Array.Copy(array, overrideValues, num2);
            }
            return true;
        }
        if (typeof(IDatParseable).IsAssignableFrom(elementType))
        {
            int num3 = -1;
            for (int j = 0; j < listNode.Count; j++)
            {
                IDatNode node = listNode[j];
                IDatParseable datParseable = (IDatParseable)Activator.CreateInstance(elementType);
                if (!datParseable.TryParse(node))
                {
                    CommandWindow.LogWarning($"Server config: unable to read {fieldInfo.Name} on line {node.GetParsedLineNumber()} as {elementType.Name}");
                    continue;
                }
                num3++;
                array.SetValue(datParseable, num3);
            }
            int num4 = num3 + 1;
            overrideValues = Array.CreateInstance(elementType, num4);
            if (num4 > 0)
            {
                Array.Copy(array, overrideValues, num4);
            }
            return true;
        }
        throw new NotImplementedException(elementType.ToString());
    }

    /// <summary>
    /// WARNING: This is called on a worker thread.
    ///
    /// Add empty dat values (if not yet added), and include code documentation
    /// in their comments prefixed with COMMENT_PREFIX. User-supplied comments are preserved.
    /// </summary>
    public static void PopulateConfigFilePropertiesAndComments(IEditableDatDictionary rootDictionary)
    {
        UnturnedCodeDocsHelper codeDocsHelper = new UnturnedCodeDocsHelper();
        IEditableDatDictionary orAddDictionary = rootDictionary.GetOrAddDictionary("Browser");
        if (!orAddDictionary.IsMetadataAvailable)
        {
            orAddDictionary.TopMargin = 1;
        }
        PopulateConfigFilePropertiesAndComments(codeDocsHelper, orAddDictionary, typeof(BrowserConfigData), null, configDefaults.Browser, null);
        IEditableDatDictionary orAddDictionary2 = rootDictionary.GetOrAddDictionary("Server");
        if (!orAddDictionary2.IsMetadataAvailable)
        {
            orAddDictionary2.TopMargin = 1;
        }
        PopulateConfigFilePropertiesAndComments(codeDocsHelper, orAddDictionary2, typeof(ServerConfigData), null, configDefaults.Server, null);
        IEditableDatDictionary orAddDictionary3 = rootDictionary.GetOrAddDictionary("UnityEvents");
        if (!orAddDictionary3.IsMetadataAvailable)
        {
            orAddDictionary3.TopMargin = 1;
        }
        PopulateConfigFilePropertiesAndComments(codeDocsHelper, orAddDictionary3, typeof(UnityEventConfigData), null, configDefaults.UnityEvents, null);
        FieldInfo[] fields = typeof(ModeConfigData).GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            object value = fieldInfo.GetValue(configDefaults.Easy);
            object value2 = fieldInfo.GetValue(configDefaults.Normal);
            object value3 = fieldInfo.GetValue(configDefaults.Hard);
            IEditableDatDictionary orAddDictionary4 = rootDictionary.GetOrAddDictionary(fieldInfo.Name);
            if (!orAddDictionary4.IsMetadataAvailable)
            {
                orAddDictionary4.TopMargin = 1;
            }
            PopulateConfigFilePropertiesAndComments(codeDocsHelper, orAddDictionary4, fieldInfo.FieldType, value, value2, value3);
        }
    }

    /// <summary>
    /// Add empty dat values for every field in category (if not yet added), and include code documentation
    /// in their comments prefixed with COMMENT_PREFIX. User-supplied comments are preserved.
    ///
    /// In categories without easy/normal/hard split (server config), only normalObject is set.
    /// </summary>
    private static void PopulateConfigFilePropertiesAndComments(UnturnedCodeDocsHelper codeDocsHelper, IEditableDatDictionary dictionary, Type categoryType, object easyObject, object normalObject, object hardObject)
    {
        FieldInfo[] fields = categoryType.GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            IEditableDatNode editableDatNode = null;
            if (dictionary.TryGetNode(fieldInfo.Name, out var node))
            {
                if (node is IDatValue datValue)
                {
                    editableDatNode = datValue.Edit();
                }
                else if (node is IDatList datList)
                {
                    editableDatNode = datList.Edit();
                }
            }
            if (editableDatNode == null)
            {
                editableDatNode = dictionary.AddValue(fieldInfo.Name);
            }
            if (!editableDatNode.IsMetadataAvailable)
            {
                editableDatNode.TopMargin = 1;
            }
            object easy = ((easyObject != null) ? fieldInfo.GetValue(easyObject) : null);
            object normal = ((normalObject != null) ? fieldInfo.GetValue(normalObject) : null);
            object hard = ((hardObject != null) ? fieldInfo.GetValue(hardObject) : null);
            string summary = codeDocsHelper.GetSummary(categoryType.Name, fieldInfo.Name);
            UpdateFieldComment(fieldInfo, editableDatNode, summary, easy, normal, hard);
        }
    }

    /// <summary>
    /// For conversion from json file. Server-only.
    /// </summary>
    public static void ApplyServerConfigOverrides(IEditableDatDictionary rootDictionary, Dictionary<FieldInfo, object> overrides)
    {
        ApplyOverridesInCategory(rootDictionary.GetOrAddDictionary("Browser"), typeof(BrowserConfigData), overrides);
        ApplyOverridesInCategory(rootDictionary.GetOrAddDictionary("Server"), typeof(ServerConfigData), overrides);
        ApplyOverridesInCategory(rootDictionary.GetOrAddDictionary("UnityEvents"), typeof(UnityEventConfigData), overrides);
    }

    /// <summary>
    /// For conversion from json file.
    /// </summary>
    public static void ApplyModeConfigOverrides(IEditableDatDictionary rootDictionary, Dictionary<FieldInfo, object> overrides)
    {
        FieldInfo[] fields = typeof(ModeConfigData).GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            IEditableDatDictionary orAddDictionary = rootDictionary.GetOrAddDictionary(fieldInfo.Name);
            Type fieldType = fieldInfo.FieldType;
            ApplyOverridesInCategory(orAddDictionary, fieldType, overrides);
        }
    }

    /// <summary>
    /// Set dat values for every field in category that has an override specified.
    /// (Will not add values if not overridden.)
    /// </summary>
    private static void ApplyOverridesInCategory(IEditableDatDictionary dictionary, Type categoryType, Dictionary<FieldInfo, object> overrides)
    {
        FieldInfo[] fields = categoryType.GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            if (overrides.TryGetValue(fieldInfo, out var value) && value != null)
            {
                if (fieldInfo.FieldType.IsArray)
                {
                    IEditableDatList orAddList = dictionary.GetOrAddList(fieldInfo.Name);
                    ApplyArrayFieldOverride(fieldInfo, orAddList, (Array)value);
                }
                else
                {
                    IEditableDatValue orAddValue = dictionary.GetOrAddValue(fieldInfo.Name);
                    ApplyValueFieldOverride(fieldInfo, orAddValue, value);
                }
            }
        }
    }

    private static void ApplyValueFieldOverride(FieldInfo fieldInfo, IEditableDatValue valueNode, object overrideValue)
    {
        Type fieldType = fieldInfo.FieldType;
        if (fieldType == typeof(bool))
        {
            valueNode.SetBool((bool)overrideValue);
            return;
        }
        if (fieldType == typeof(float))
        {
            valueNode.SetFloat((float)overrideValue);
            return;
        }
        if (fieldType == typeof(uint))
        {
            valueNode.SetUInt32((uint)overrideValue);
            return;
        }
        if (fieldType == typeof(int))
        {
            valueNode.SetInt32((int)overrideValue);
            return;
        }
        if (fieldType.IsEnum)
        {
            valueNode.Value = overrideValue.ToString();
            return;
        }
        if (fieldType == typeof(string))
        {
            valueNode.Value = (string)overrideValue;
            return;
        }
        throw new NotImplementedException(fieldType.ToString());
    }

    private static void ApplyArrayFieldOverride(FieldInfo fieldInfo, IEditableDatList listNode, Array overrideValues)
    {
        Type fieldType = fieldInfo.FieldType;
        if (fieldType == typeof(string[]))
        {
            foreach (object overrideValue in overrideValues)
            {
                listNode.AddValue().SetString((string)overrideValue);
            }
            return;
        }
        if (typeof(IDatSerializable).IsAssignableFrom(fieldType.GetElementType()))
        {
            foreach (IDatSerializable overrideValue2 in overrideValues)
            {
                IEditableDatDictionary dictionary = listNode.AddDictionary();
                overrideValue2.SerializeIntoDictionary(dictionary);
            }
            return;
        }
        throw new NotImplementedException(fieldType.ToString());
    }

    /// <summary>
    /// For conversion from json file. Find fields different from default in the server-related categories.
    /// </summary>
    public static void GatherServerModifiedFields(ConfigData baseConfig, ConfigData currentConfig, Dictionary<FieldInfo, object> results)
    {
        GatherModifiedFields(baseConfig.Server, currentConfig.Server, results);
        GatherModifiedFields(baseConfig.Browser, currentConfig.Browser, results);
        GatherModifiedFields(baseConfig.UnityEvents, currentConfig.UnityEvents, results);
    }

    /// <summary>
    /// For conversion from json file. Find fields different from defaults in one of easy/normal/hard mode.
    /// </summary>
    public static void GatherModifiedFields(ModeConfigData baseConfig, ModeConfigData currentConfig, Dictionary<FieldInfo, object> results)
    {
        FieldInfo[] fields = typeof(ModeConfigData).GetFields();
        foreach (FieldInfo obj in fields)
        {
            object value = obj.GetValue(baseConfig);
            object value2 = obj.GetValue(currentConfig);
            GatherModifiedFields(value, value2, results);
        }
    }

    private static void GatherModifiedFields(object baseObject, object currentObject, Dictionary<FieldInfo, object> results)
    {
        FieldInfo[] fields = baseObject.GetType().GetFields();
        foreach (FieldInfo fieldInfo in fields)
        {
            object value = fieldInfo.GetValue(baseObject);
            object value2 = fieldInfo.GetValue(currentObject);
            Type fieldType = fieldInfo.FieldType;
            if (fieldType == typeof(bool))
            {
                bool num = (bool)value2;
                bool flag = (bool)value;
                if (num != flag)
                {
                    results.Add(fieldInfo, value2);
                }
            }
            else if (fieldType == typeof(float))
            {
                float a = (float)value2;
                float b = (float)value;
                if (!MathfEx.IsNearlyEqual(a, b, 0.0001f))
                {
                    results.Add(fieldInfo, value2);
                }
            }
            else if (fieldType == typeof(int) || fieldType.IsEnum)
            {
                int num2 = (int)value2;
                int num3 = (int)value;
                if (num2 != num3)
                {
                    results.Add(fieldInfo, value2);
                }
            }
            else if (fieldType == typeof(uint))
            {
                uint num4 = (uint)value2;
                uint num5 = (uint)value;
                if (num4 != num5)
                {
                    results.Add(fieldInfo, value2);
                }
            }
            else if (fieldType == typeof(string))
            {
                string a2 = (string)value2;
                string b2 = (string)value;
                if (!string.Equals(a2, b2))
                {
                    results.Add(fieldInfo, value2);
                }
            }
            else if (value == null != (value2 == null))
            {
                results.Add(fieldInfo, value2);
            }
            else
            {
                if (value == null || value2 == null || !fieldType.IsArray)
                {
                    continue;
                }
                Array array = (Array)value2;
                Array array2 = (Array)value;
                if (array.Length != array2.Length)
                {
                    results.Add(fieldInfo, value2);
                    continue;
                }
                for (int j = 0; j < array.Length; j++)
                {
                    if (!object.Equals(array.GetValue(j), array2.GetValue(j)))
                    {
                        results.Add(fieldInfo, value2);
                        break;
                    }
                }
            }
        }
    }

    public static void RemoveEmptyValues(IEditableDatDictionary dictionary)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, IDatNode> item in dictionary)
        {
            switch (item.Value.NodeType)
            {
            case EDatNodeType.Value:
                if (((IDatValue)item.Value).IsValueNullOrEmpty())
                {
                    list.Add(item.Key);
                }
                break;
            case EDatNodeType.Dictionary:
            {
                IEditableDatDictionary editableDatDictionary = ((IDatDictionary)item.Value).Edit();
                if (editableDatDictionary != null)
                {
                    RemoveEmptyValues(editableDatDictionary);
                }
                if (((IDatDictionary)item.Value).Count < 1)
                {
                    list.Add(item.Key);
                }
                break;
            }
            case EDatNodeType.List:
            {
                IEditableDatList editableDatList = ((IDatList)item.Value).Edit();
                if (editableDatList != null)
                {
                    RemoveEmptyValues(editableDatList);
                }
                if (((IDatList)item.Value).Count < 1)
                {
                    list.Add(item.Key);
                }
                break;
            }
            }
        }
        foreach (string item2 in list)
        {
            dictionary.Remove(item2);
        }
    }

    private static void RemoveEmptyValues(IEditableDatList list)
    {
        for (int num = list.Count - 1; num >= 0; num--)
        {
            IDatNode datNode = list[num];
            switch (datNode.NodeType)
            {
            case EDatNodeType.Value:
                if (((IDatValue)datNode).IsValueNullOrEmpty())
                {
                    list.RemoveAt(num);
                }
                break;
            case EDatNodeType.Dictionary:
            {
                IEditableDatDictionary editableDatDictionary = ((IDatDictionary)datNode).Edit();
                if (editableDatDictionary != null)
                {
                    RemoveEmptyValues(editableDatDictionary);
                }
                if (((IDatDictionary)datNode).Count < 1)
                {
                    list.RemoveAt(num);
                }
                break;
            }
            case EDatNodeType.List:
            {
                IEditableDatList editableDatList = ((IDatList)datNode).Edit();
                if (editableDatList != null)
                {
                    RemoveEmptyValues(editableDatList);
                }
                if (((IDatList)datNode).Count < 1)
                {
                    list.RemoveAt(num);
                }
                break;
            }
            }
        }
    }

    public static void RemoveGeneratedComments(IEditableDatNode node)
    {
        node.MergeGeneratedComment<IEditableDatNode, string[]>("> ", null, commentStringBuilder, tempParsedLines);
        switch (node.NodeType)
        {
        case EDatNodeType.Dictionary:
        {
            foreach (KeyValuePair<string, IDatNode> item in (IDatDictionary)node)
            {
                RemoveGeneratedCommentsWrapper(item.Value);
            }
            break;
        }
        case EDatNodeType.List:
        {
            foreach (IDatNode item2 in (IDatList)node)
            {
                RemoveGeneratedCommentsWrapper(item2);
            }
            break;
        }
        }
    }

    private static void RemoveGeneratedCommentsWrapper(IDatNode node)
    {
        switch (node.NodeType)
        {
        case EDatNodeType.Dictionary:
            RemoveGeneratedComments(((IDatDictionary)node).Edit());
            break;
        case EDatNodeType.List:
            RemoveGeneratedComments(((IDatList)node).Edit());
            break;
        case EDatNodeType.Value:
            RemoveGeneratedComments(((IDatValue)node).Edit());
            break;
        }
    }

    private static string GetDefaultValueComment(object defaultValue)
    {
        if (defaultValue == null)
        {
            return null;
        }
        if (defaultValue is string text)
        {
            if (text.Length < 1)
            {
                return null;
            }
            return text;
        }
        if (defaultValue.GetType().IsArray)
        {
            return null;
        }
        return defaultValue.ToString();
    }

    private static void UpdateFieldComment(FieldInfo fieldInfo, IEditableDatNode node, string summary, object easy, object normal, object hard)
    {
        commentStringBuilder.Clear();
        generatedLines.Clear();
        if (!string.IsNullOrEmpty(summary))
        {
            string[] array = summary.SplitLinesIncludingEmpty();
            int num = array.Length;
            if (string.IsNullOrWhiteSpace(array[num - 1]))
            {
                num--;
            }
            for (int i = 0; i < num; i++)
            {
                generatedLines.Add(array[i].Trim());
            }
        }
        if (fieldInfo.FieldType.IsEnum)
        {
            string[] enumNames = fieldInfo.FieldType.GetEnumNames();
            commentStringBuilder.Clear();
            commentStringBuilder.Append("Options: ");
            bool flag = true;
            string[] array2 = enumNames;
            foreach (string value in array2)
            {
                if (!flag)
                {
                    commentStringBuilder.Append(", ");
                }
                commentStringBuilder.Append(value);
                flag = false;
            }
            generatedLines.Add(commentStringBuilder.ToString());
        }
        if (easy == null || hard == null || (normal.Equals(easy) && normal.Equals(hard)))
        {
            string defaultValueComment = GetDefaultValueComment(normal);
            if (!string.IsNullOrEmpty(defaultValueComment))
            {
                commentStringBuilder.Clear();
                commentStringBuilder.Append("Default: ");
                commentStringBuilder.Append(defaultValueComment);
                generatedLines.Add(commentStringBuilder.ToString());
            }
        }
        else
        {
            commentStringBuilder.Clear();
            commentStringBuilder.Append("Easy: ");
            commentStringBuilder.Append(GetDefaultValueComment(easy));
            commentStringBuilder.Append("    Normal: ");
            commentStringBuilder.Append(GetDefaultValueComment(normal));
            commentStringBuilder.Append("    Hard: ");
            commentStringBuilder.Append(GetDefaultValueComment(hard));
            generatedLines.Add(commentStringBuilder.ToString());
        }
        node.MergeGeneratedComment("> ", generatedLines, commentStringBuilder, tempParsedLines);
    }
}
