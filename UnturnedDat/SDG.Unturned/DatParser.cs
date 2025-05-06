using System.Collections.Generic;
using System.IO;

namespace SDG.Unturned;

public class DatParser
{
    private DatTokenizer tokenizer;

    private int currentLineNumber;

    private DatToken currentToken;

    private bool hasToken;

    private List<string> errorMessages;

    private bool enableMetadata;

    private List<string> commentLines;

    private int commentStartingLineNumber;

    private int commentEndingLineNumber;

    public bool EnableMetadata
    {
        get
        {
            return enableMetadata;
        }
        set
        {
            enableMetadata = value;
            if (enableMetadata && commentLines == null)
            {
                commentLines = new List<string>();
            }
        }
    }

    public bool HasError => errorMessages.Count > 0;

    public string ErrorMessage
    {
        get
        {
            if (errorMessages.Count <= 0)
            {
                return null;
            }
            return errorMessages[0];
        }
    }

    public IReadOnlyList<string> ErrorMessages => errorMessages;

    public DatParser()
    {
        tokenizer = new DatTokenizer();
        errorMessages = new List<string>();
    }

    public IDatDictionary Parse(TextReader inputReader)
    {
        tokenizer.EnableComments = enableMetadata;
        tokenizer.Tokenize(inputReader);
        errorMessages.Clear();
        hasToken = false;
        currentLineNumber = 1;
        if (tokenizer.HasError)
        {
            errorMessages.AddRange(tokenizer.errorMessages);
        }
        ReadToken();
        DatDictionary datDictionary = new DatDictionary();
        IDatDictionary datDictionary2 = datDictionary;
        if (enableMetadata)
        {
            datDictionary2 = new DatDictionaryWithMetadata(datDictionary)
            {
                openingLineNumber = 1
            };
        }
        while (hasToken)
        {
            switch (currentToken.type)
            {
            case EDatTokenType.Key:
            {
                string value = currentToken.value;
                IDatNode value2 = ReadDictionaryValue();
                AddValueToDictionary(datDictionary, datDictionary2, value, value2);
                break;
            }
            case EDatTokenType.Comment:
                BuildComment();
                break;
            default:
                ReadToken();
                break;
            }
        }
        if (enableMetadata)
        {
            ((DatDictionaryWithMetadata)datDictionary2).closingLineNumber = currentLineNumber;
        }
        return datDictionary2;
    }

    public IDatDictionary Parse(string input)
    {
        using StringReader inputReader = new StringReader(input);
        return Parse(inputReader);
    }

    public IDatDictionary Parse(byte[] input)
    {
        using MemoryStream stream = new MemoryStream(input);
        using StreamReader inputReader = new StreamReader(stream);
        return Parse(inputReader);
    }

    private void ReadToken()
    {
        hasToken = tokenizer.ReadToken(out currentToken);
        if (currentToken.type == EDatTokenType.LineBreak)
        {
            currentLineNumber++;
        }
    }

    private void AddValueToDictionary(DatDictionary underlyingDictionary, IDatDictionary dictionary, string key, IDatNode value)
    {
        if (enableMetadata)
        {
            ((DatNodeWithMetadataBase)value).parentNode = dictionary;
        }
        if (dictionary.TryGetNode(key, out var node))
        {
            string nodeStringForErrorMessage = GetNodeStringForErrorMessage(node);
            string nodeStringForErrorMessage2 = GetNodeStringForErrorMessage(value);
            PushErrorMessage($"duplicate key \"{key}\" on line {currentLineNumber} replacing existing value {nodeStringForErrorMessage} with {nodeStringForErrorMessage2}");
        }
        underlyingDictionary[key] = value;
    }

    private string GetNodeStringForErrorMessage(IDatNode node)
    {
        if (node == null)
        {
            return "null";
        }
        if (node is IDatList datList)
        {
            return $"list with {datList.Count} item(s)";
        }
        if (node is IDatDictionary datDictionary)
        {
            return $"dictionary with {datDictionary.Count} item(s)";
        }
        if (node is IDatValue datValue)
        {
            if (datValue.Value == null)
            {
                return "value(null)";
            }
            return "\"" + datValue.Value + "\"";
        }
        return node.GetType().Name;
    }

    private void BuildComment()
    {
        if (enableMetadata)
        {
            if (commentLines.Count < 1)
            {
                commentStartingLineNumber = currentLineNumber;
            }
            commentEndingLineNumber = currentLineNumber;
            commentLines.Add(currentToken.value);
        }
        ReadToken();
    }

    private DatComment? ConsumePrefixComment()
    {
        DatComment? result = null;
        if (enableMetadata && commentLines.Count > 0)
        {
            result = new DatComment
            {
                MessageLines = commentLines.ToArray(),
                StartingLineNumber = commentStartingLineNumber,
                EndingLineNumber = commentEndingLineNumber
            };
            commentLines.Clear();
        }
        return result;
    }

    private IDatNode ReadDictionaryValue()
    {
        int lineNumber = currentLineNumber;
        ReadToken();
        DatComment? prefixComment = ConsumePrefixComment();
        DatToken datToken = currentToken;
        if (hasToken && currentToken.type == EDatTokenType.Value)
        {
            ReadToken();
        }
        DatToken datToken2 = currentToken;
        if (hasToken && currentToken.type == EDatTokenType.Comment)
        {
            ReadToken();
        }
        if (hasToken && currentToken.type == EDatTokenType.LineBreak)
        {
            ReadToken();
        }
        if (hasToken)
        {
            switch (currentToken.type)
            {
            case EDatTokenType.OpenDictionary:
            {
                IDatDictionary datDictionary = ReadDictionary();
                if (enableMetadata)
                {
                    ((DatDictionaryWithMetadata)datDictionary).prefixComment = prefixComment;
                }
                return datDictionary;
            }
            case EDatTokenType.OpenList:
            {
                IDatList datList = ReadList();
                if (enableMetadata)
                {
                    ((DatListWithMetadata)datList).prefixComment = prefixComment;
                }
                return datList;
            }
            }
        }
        DatValue datValue = new DatValue((datToken.type == EDatTokenType.Value) ? datToken.value : null);
        if (enableMetadata)
        {
            string inlineComment = ((datToken2.type == EDatTokenType.Comment) ? datToken2.value : null);
            return new DatValueWithMetadata(datValue, lineNumber, inlineComment, prefixComment);
        }
        return datValue;
    }

    private IDatDictionary ReadDictionary()
    {
        int num = currentLineNumber;
        ReadToken();
        DatDictionary datDictionary = new DatDictionary();
        IDatDictionary datDictionary2 = datDictionary;
        if (enableMetadata)
        {
            datDictionary2 = new DatDictionaryWithMetadata(datDictionary);
            commentLines.Clear();
        }
        bool flag = false;
        while (hasToken && !flag)
        {
            switch (currentToken.type)
            {
            case EDatTokenType.CloseDictionary:
                if (enableMetadata)
                {
                    DatDictionaryWithMetadata obj = (DatDictionaryWithMetadata)datDictionary2;
                    obj.openingLineNumber = num;
                    obj.closingLineNumber = currentLineNumber;
                }
                ReadToken();
                flag = true;
                break;
            case EDatTokenType.Key:
            {
                string value = currentToken.value;
                IDatNode value2 = ReadDictionaryValue();
                AddValueToDictionary(datDictionary, datDictionary2, value, value2);
                break;
            }
            case EDatTokenType.Comment:
                BuildComment();
                break;
            default:
                ReadToken();
                break;
            }
        }
        if (!flag)
        {
            PushErrorMessage($"missing closing curly bracket '}}' for dictionary opened on line {num}");
        }
        return datDictionary2;
    }

    private IDatList ReadList()
    {
        int num = currentLineNumber;
        ReadToken();
        DatList datList = new DatList();
        IDatList datList3;
        if (!enableMetadata)
        {
            IDatList datList2 = datList;
            datList3 = datList2;
        }
        else
        {
            IDatList datList2 = new DatListWithMetadata(datList);
            datList3 = datList2;
        }
        IDatList datList4 = datList3;
        if (enableMetadata)
        {
            commentLines.Clear();
        }
        bool flag = false;
        while (hasToken && !flag)
        {
            switch (currentToken.type)
            {
            case EDatTokenType.CloseList:
                if (enableMetadata)
                {
                    DatListWithMetadata obj2 = (DatListWithMetadata)datList4;
                    obj2.openingLineNumber = num;
                    obj2.closingLineNumber = currentLineNumber;
                }
                ReadToken();
                flag = true;
                break;
            case EDatTokenType.OpenDictionary:
            {
                DatComment? prefixComment3 = ConsumePrefixComment();
                IDatDictionary datDictionary = ReadDictionary();
                if (enableMetadata)
                {
                    DatDictionaryWithMetadata obj3 = (DatDictionaryWithMetadata)datDictionary;
                    obj3.parentNode = datList4;
                    obj3.prefixComment = prefixComment3;
                }
                datList.Add(datDictionary);
                break;
            }
            case EDatTokenType.OpenList:
            {
                DatComment? prefixComment2 = ConsumePrefixComment();
                IDatList datList5 = ReadList();
                if (enableMetadata)
                {
                    DatListWithMetadata obj = (DatListWithMetadata)datList5;
                    obj.parentNode = datList4;
                    obj.prefixComment = prefixComment2;
                }
                datList.Add(datList5);
                break;
            }
            case EDatTokenType.Value:
            {
                DatComment? prefixComment = ConsumePrefixComment();
                string value = currentToken.value;
                int lineNumber = currentLineNumber;
                ReadToken();
                DatValue datValue = new DatValue(value);
                if (enableMetadata)
                {
                    string inlineComment = null;
                    if (hasToken && currentToken.type == EDatTokenType.Comment)
                    {
                        inlineComment = currentToken.value;
                        ReadToken();
                    }
                    DatValueWithMetadata datValueWithMetadata = new DatValueWithMetadata(datValue, lineNumber, inlineComment, prefixComment);
                    datValueWithMetadata.parentNode = datList4;
                    datList.Add(datValueWithMetadata);
                }
                else
                {
                    datList.Add(datValue);
                }
                break;
            }
            case EDatTokenType.Comment:
                BuildComment();
                break;
            default:
                ReadToken();
                break;
            }
        }
        if (!flag)
        {
            PushErrorMessage($"missing closing bracket ']' for list opened on line {num}");
        }
        return datList4;
    }

    private void PushErrorMessage(string message)
    {
        errorMessages.Add(message);
    }
}
