using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SDG.Unturned;

internal class DatTokenizer
{
    private enum EContext
    {
        Dictionary,
        List
    }

    public List<string> errorMessages;

    private TextReader inputReader;

    private int currentLineNumber;

    private char currentChar;

    private bool hasChar;

    private List<DatToken> tokens;

    private List<EContext> contextStack;

    private int tokenIndex;

    private StringBuilder stringBuilder;

    public bool HasError => errorMessages.Count > 0;

    public bool EnableComments { get; set; }

    public DatTokenizer()
    {
        errorMessages = new List<string>();
        tokens = new List<DatToken>();
        contextStack = new List<EContext>();
        stringBuilder = new StringBuilder();
    }

    public void Tokenize(TextReader inputReader)
    {
        this.inputReader = inputReader;
        hasChar = false;
        currentLineNumber = 1;
        tokens.Clear();
        errorMessages.Clear();
        contextStack.Clear();
        tokenIndex = 0;
        ReadChar();
        SkipUtf8Bom();
        while (hasChar)
        {
            if (currentChar == '/')
            {
                if (EnableComments)
                {
                    ReadComment();
                }
                else
                {
                    SkipToEndOfLine();
                }
            }
            else if (currentChar == '\r')
            {
                ReadChar();
                if (currentChar == '\n')
                {
                    ReadChar();
                }
                PushToken(EDatTokenType.LineBreak);
            }
            else if (currentChar == '\n')
            {
                ReadChar();
                PushToken(EDatTokenType.LineBreak);
            }
            else if (currentChar == '{')
            {
                PushToken(EDatTokenType.OpenDictionary);
                PushContext(EContext.Dictionary);
                ReadChar();
                if (hasChar && currentChar == ',')
                {
                    ReadChar();
                }
            }
            else if (currentChar == '}')
            {
                PopContext(EContext.Dictionary);
                PushToken(EDatTokenType.CloseDictionary);
                ReadChar();
                if (hasChar && currentChar == ',')
                {
                    ReadChar();
                }
            }
            else if (currentChar == '[')
            {
                PushToken(EDatTokenType.OpenList);
                PushContext(EContext.List);
                ReadChar();
                if (hasChar && currentChar == ',')
                {
                    ReadChar();
                }
            }
            else if (currentChar == ']')
            {
                PopContext(EContext.List);
                PushToken(EDatTokenType.CloseList);
                ReadChar();
                if (hasChar && currentChar == ',')
                {
                    ReadChar();
                }
            }
            else if (char.IsWhiteSpace(currentChar))
            {
                ReadChar();
            }
            else if (GetContext() == EContext.Dictionary)
            {
                ReadDictionaryKey();
                SkipSpacesAndTabs();
                if (hasChar && !char.IsWhiteSpace(currentChar))
                {
                    ReadStringValue();
                }
            }
            else
            {
                ReadStringValue();
            }
        }
    }

    public void Tokenize(string input)
    {
        using StringReader stringReader = new StringReader(input);
        Tokenize(stringReader);
    }

    public bool ReadToken(out DatToken token)
    {
        if (tokenIndex < tokens.Count)
        {
            token = tokens[tokenIndex];
            tokenIndex++;
            return true;
        }
        token = new DatToken(EDatTokenType.Null);
        return false;
    }

    private void ReadChar()
    {
        bool flag = hasChar && currentChar == '\r';
        int num = inputReader.Read();
        hasChar = num >= 0;
        currentChar = (hasChar ? ((char)num) : '\0');
        currentLineNumber += ((hasChar && (currentChar == '\r' || (currentChar == '\n' && !flag))) ? 1 : 0);
    }

    private void SkipUtf8Bom()
    {
        if (!hasChar || currentChar != 'ï')
        {
            return;
        }
        ReadChar();
        if (hasChar && currentChar == '»')
        {
            ReadChar();
            if (hasChar && currentChar == '¿')
            {
                ReadChar();
            }
        }
    }

    private void SkipSpacesAndTabs()
    {
        while (hasChar && (currentChar == ' ' || currentChar == '\t'))
        {
            ReadChar();
        }
    }

    private void SkipToEndOfLine()
    {
        while (hasChar && currentChar != '\r' && currentChar != '\n')
        {
            ReadChar();
        }
    }

    private void ReadComment()
    {
        do
        {
            ReadChar();
        }
        while (hasChar && currentChar == '/');
        if (hasChar && currentChar == ' ')
        {
            ReadChar();
        }
        stringBuilder.Clear();
        while (hasChar && currentChar != '\r' && currentChar != '\n')
        {
            stringBuilder.Append(currentChar);
            ReadChar();
        }
        if (stringBuilder.Length > 0)
        {
            PushToken(EDatTokenType.Comment, stringBuilder.ToString());
        }
        else
        {
            PushToken(EDatTokenType.Comment);
        }
    }

    private void ReadQuotedString(EDatTokenType type)
    {
        int num = currentLineNumber;
        ReadChar();
        bool flag = false;
        bool flag2 = false;
        stringBuilder.Clear();
        while (hasChar)
        {
            if (flag)
            {
                if (currentChar == 'n')
                {
                    currentChar = '\n';
                }
                else if (currentChar == 't')
                {
                    currentChar = '\t';
                }
                else if (currentChar != '\\' && currentChar != '"')
                {
                    stringBuilder.Append('\\');
                    PushErrorMessage($"unrecognized escape sequence (\\{currentChar}) on line {currentLineNumber} — if this is a file path please use forward slash (/)");
                }
            }
            else
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
        if (!flag2)
        {
            PushErrorMessage($"missing closing quotation mark (\") for string opened on line {num}");
        }
        PushToken(type, stringBuilder.ToString());
        if (hasChar && currentChar == ',')
        {
            ReadChar();
        }
    }

    private void ReadDictionaryKey()
    {
        if (currentChar == '"')
        {
            ReadQuotedString(EDatTokenType.Key);
            return;
        }
        stringBuilder.Clear();
        do
        {
            stringBuilder.Append(currentChar);
            ReadChar();
        }
        while (hasChar && !char.IsWhiteSpace(currentChar));
        PushToken(EDatTokenType.Key, stringBuilder.ToString());
    }

    private void ReadStringValue()
    {
        if (currentChar == '"')
        {
            ReadQuotedString(EDatTokenType.Value);
            return;
        }
        bool flag = false;
        stringBuilder.Clear();
        do
        {
            if (flag)
            {
                if (currentChar == 'n')
                {
                    currentChar = '\n';
                }
                else if (currentChar == 't')
                {
                    currentChar = '\t';
                }
                else if (currentChar != '\\')
                {
                    stringBuilder.Append('\\');
                    PushErrorMessage($"unrecognized escape sequence (\\{currentChar}) on line {currentLineNumber} — if this is a file path please use forward slash (/)");
                }
            }
            else
            {
                if (currentChar == '\r' || currentChar == '\n')
                {
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
        while (hasChar);
        PushToken(EDatTokenType.Value, stringBuilder.ToString());
    }

    private EContext GetContext()
    {
        int count = contextStack.Count;
        if (count <= 0)
        {
            return EContext.Dictionary;
        }
        return contextStack[count - 1];
    }

    private void PushToken(EDatTokenType type)
    {
        tokens.Add(new DatToken(type));
    }

    private void PushToken(EDatTokenType type, string value)
    {
        tokens.Add(new DatToken(type, value));
    }

    private void PushErrorMessage(string message)
    {
        errorMessages.Add(message);
    }

    private void PushContext(EContext context)
    {
        contextStack.Add(context);
    }

    private void PopContext(EContext expectedContext)
    {
        int count = contextStack.Count;
        if (count > 0)
        {
            EContext eContext = contextStack[count - 1];
            if (expectedContext == eContext)
            {
                contextStack.RemoveAt(count - 1);
                return;
            }
        }
        switch (expectedContext)
        {
        case EContext.Dictionary:
            PushErrorMessage($"unexpected end of dictionary/object '}}' on line {currentLineNumber}");
            break;
        case EContext.List:
            PushErrorMessage($"unexpected end of list ']' on line {currentLineNumber}");
            break;
        }
    }

    public void DebugDumpTokensToStringBuilder(StringBuilder output)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            output.Append(i);
            output.Append(' ');
            output.Append(tokens[i].type);
            if (string.IsNullOrEmpty(tokens[i].value))
            {
                output.AppendLine();
                continue;
            }
            output.Append(' ');
            output.AppendLine(tokens[i].value);
        }
    }

    public string DebugDumpTokensToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpTokensToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }
}
