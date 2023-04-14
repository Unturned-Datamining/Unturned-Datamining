using System.IO;
using System.Text;

namespace SDG.Unturned;

public class DatParser
{
    private TextReader inputReader;

    private int currentLineNumber;

    private int currentReadResult;

    private char currentChar;

    private bool hasChar;

    private bool hasError;

    private string errorMessage;

    private StringBuilder stringBuilder;

    public bool HasError => hasError;

    public string ErrorMessage
    {
        get
        {
            return errorMessage;
        }
        private set
        {
            errorMessage = value;
            hasError = !string.IsNullOrEmpty(errorMessage);
        }
    }

    public DatParser()
    {
        stringBuilder = new StringBuilder();
    }

    public DatDictionary Parse(TextReader inputReader)
    {
        this.inputReader = inputReader;
        ErrorMessage = null;
        hasChar = false;
        currentLineNumber = 1;
        ReadChar();
        SkipWhitespaceAndComments();
        DatDictionary datDictionary = new DatDictionary();
        while (hasChar)
        {
            if (currentChar == '/')
            {
                SkipWhitespaceAndComments();
                continue;
            }
            string key = ReadDictionaryKey();
            SkipSpacesAndTabs();
            IDatNode value = ReadDictionaryValue();
            AddValueToDictionary(datDictionary, key, value);
        }
        return datDictionary;
    }

    public DatDictionary Parse(string input)
    {
        using StringReader stringReader = new StringReader(input);
        return Parse(stringReader);
    }

    private void ReadChar()
    {
        bool flag = hasChar && currentChar == '\r';
        currentReadResult = inputReader.Read();
        hasChar = currentReadResult >= 0;
        currentChar = (hasChar ? ((char)currentReadResult) : '\0');
        currentLineNumber += ((hasChar && (currentChar == '\r' || (currentChar == '\n' && !flag))) ? 1 : 0);
    }

    private void SkipSpacesAndTabs()
    {
        while (hasChar && (currentChar == ' ' || currentChar == '\t'))
        {
            ReadChar();
        }
    }

    private void SkipWhitespaceAndComments()
    {
        while (hasChar)
        {
            if (currentChar == '/')
            {
                ReadChar();
                while (hasChar && currentChar != '\n' && currentChar != '\r')
                {
                    ReadChar();
                }
                continue;
            }
            if (char.IsWhiteSpace(currentChar) || currentChar == ',')
            {
                ReadChar();
                continue;
            }
            break;
        }
    }

    private void AddValueToDictionary(DatDictionary dictionary, string key, IDatNode value)
    {
        if (hasError)
        {
            dictionary[key] = value;
            return;
        }
        if (!dictionary.TryGetNode(key, out var node))
        {
            dictionary.Add(key, value);
            return;
        }
        dictionary[key] = value;
        string nodeStringForErrorMessage = GetNodeStringForErrorMessage(node);
        string nodeStringForErrorMessage2 = GetNodeStringForErrorMessage(value);
        ErrorMessage = $"duplicate key \"{key}\" on line {currentLineNumber} replacing existing value {nodeStringForErrorMessage} with {nodeStringForErrorMessage2}";
    }

    private string GetNodeStringForErrorMessage(IDatNode node)
    {
        if (node == null)
        {
            return "null";
        }
        if (node is DatList datList)
        {
            return $"list with {datList.Count} item(s)";
        }
        if (node is DatDictionary datDictionary)
        {
            return $"dictionary with {datDictionary.Count} item(s)";
        }
        if (node is DatValue datValue)
        {
            if (datValue.value == null)
            {
                return "value(null)";
            }
            return "\"" + datValue.value + "\"";
        }
        return node.GetType().Name;
    }

    private IDatNode ReadDictionaryValue()
    {
        string value = ReadString();
        SkipWhitespaceAndComments();
        if (hasChar)
        {
            if (currentChar == '{')
            {
                return ReadDictionary();
            }
            if (currentChar == '[')
            {
                return ReadList();
            }
        }
        return new DatValue(value);
    }

    private DatDictionary ReadDictionary()
    {
        int num = currentLineNumber;
        ReadChar();
        SkipWhitespaceAndComments();
        DatDictionary datDictionary = new DatDictionary();
        bool flag = false;
        while (hasChar)
        {
            if (currentChar == '/')
            {
                SkipWhitespaceAndComments();
                continue;
            }
            if (currentChar == '}')
            {
                ReadChar();
                flag = true;
                break;
            }
            string key = ReadDictionaryKey();
            SkipSpacesAndTabs();
            IDatNode value = ReadDictionaryValue();
            AddValueToDictionary(datDictionary, key, value);
        }
        if (!flag && !hasError)
        {
            ErrorMessage = $"missing closing curly bracket '}}' for dictionary opened on line {num}";
        }
        SkipWhitespaceAndComments();
        return datDictionary;
    }

    private DatList ReadList()
    {
        int num = currentLineNumber;
        ReadChar();
        SkipWhitespaceAndComments();
        DatList datList = new DatList();
        bool flag = false;
        while (hasChar)
        {
            if (currentChar == '/')
            {
                SkipWhitespaceAndComments();
                continue;
            }
            if (currentChar == ']')
            {
                ReadChar();
                flag = true;
                break;
            }
            if (currentChar == '{')
            {
                datList.Add(ReadDictionary());
                continue;
            }
            if (currentChar == '[')
            {
                datList.Add(ReadList());
                continue;
            }
            string value = ReadString();
            SkipWhitespaceAndComments();
            datList.Add(new DatValue(value));
        }
        if (!flag && !hasError)
        {
            ErrorMessage = $"missing closing bracket ']' for list opened on line {num}";
        }
        SkipWhitespaceAndComments();
        return datList;
    }

    private string ReadDictionaryKey()
    {
        if (currentChar == '"')
        {
            return ReadQuotedString();
        }
        stringBuilder.Clear();
        while (hasChar && !char.IsWhiteSpace(currentChar))
        {
            stringBuilder.Append(currentChar);
            ReadChar();
        }
        return stringBuilder.ToString();
    }

    private string ReadString()
    {
        if (currentChar == '"')
        {
            return ReadQuotedString();
        }
        stringBuilder.Clear();
        while (hasChar && currentChar != '\r' && currentChar != '\n')
        {
            stringBuilder.Append(currentChar);
            ReadChar();
        }
        return stringBuilder.ToString();
    }

    private string ReadQuotedString()
    {
        int num = currentLineNumber;
        ReadChar();
        bool flag = false;
        bool flag2 = false;
        stringBuilder.Clear();
        while (hasChar)
        {
            if (!flag)
            {
                if (currentChar == '"')
                {
                    ReadChar();
                    flag2 = true;
                    break;
                }
                if (currentChar == '\\')
                {
                    flag = true;
                    ReadChar();
                    continue;
                }
            }
            flag = false;
            stringBuilder.Append(currentChar);
            ReadChar();
        }
        if (!flag2 && !hasError)
        {
            ErrorMessage = $"missing closing quotation mark (\") for string opened on line {num}";
        }
        return stringBuilder.ToString();
    }
}
