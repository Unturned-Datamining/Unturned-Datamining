using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SDG.Unturned;

/// <summary>
/// Breaks down Steam's version of BBcode into tokens like, "[b]", "[i]", "actual text", etc.
/// </summary>
public class BbCodeTokenizer
{
    private TextReader inputReader;

    private int currentLineNumber;

    private int currentReadResult;

    private char currentChar;

    private bool hasChar;

    private bool hasError;

    private string errorMessage;

    private StringBuilder tagStringBuilder;

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

    public BbCodeTokenizer()
    {
        tagStringBuilder = new StringBuilder();
        stringBuilder = new StringBuilder();
    }

    public List<BbCodeToken> Tokenize(TextReader inputReader)
    {
        this.inputReader = inputReader;
        ErrorMessage = null;
        hasChar = false;
        currentLineNumber = 1;
        List<BbCodeToken> list = new List<BbCodeToken>();
        ReadChar();
        int num = 0;
        while (hasChar)
        {
            ReadToken(list);
            num++;
            if (num >= 10000)
            {
                ErrorMessage = "Infinite loop attempting to tokenize";
                break;
            }
        }
        return list;
    }

    public List<BbCodeToken> Tokenize(string input)
    {
        using StringReader stringReader = new StringReader(input);
        return Tokenize(stringReader);
    }

    public string DebugDumpTokensToString(List<BbCodeToken> tokens)
    {
        stringBuilder.Clear();
        for (int i = 0; i < tokens.Count; i++)
        {
            stringBuilder.Append(i);
            stringBuilder.Append(": ");
            BbCodeToken bbCodeToken = tokens[i];
            if (string.IsNullOrEmpty(bbCodeToken.tokenValue))
            {
                stringBuilder.AppendLine(bbCodeToken.tokenType.ToString());
                continue;
            }
            stringBuilder.Append(bbCodeToken.tokenType.ToString());
            stringBuilder.Append(": ");
            stringBuilder.AppendLine(bbCodeToken.tokenValue);
        }
        return stringBuilder.ToString();
    }

    private void ReadChar()
    {
        bool flag = hasChar && currentChar == '\r';
        currentReadResult = inputReader.Read();
        hasChar = currentReadResult >= 0;
        currentChar = (hasChar ? ((char)currentReadResult) : '\0');
        currentLineNumber += ((hasChar && (currentChar == '\r' || (currentChar == '\n' && !flag))) ? 1 : 0);
    }

    private void ReadToken(List<BbCodeToken> outputTokens)
    {
        if (currentChar == '[')
        {
            ReadTag(outputTokens);
        }
        else if (currentChar == '\r')
        {
            ReadChar();
            if (hasChar && currentChar == '\n')
            {
                ReadChar();
            }
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.LineBreak));
        }
        else if (currentChar == '\n')
        {
            ReadChar();
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.LineBreak));
        }
        else
        {
            ReadString(outputTokens);
        }
    }

    private void ReadString(List<BbCodeToken> tokens)
    {
        stringBuilder.Clear();
        do
        {
            stringBuilder.Append(currentChar);
            ReadChar();
        }
        while (currentChar != '[' && currentChar != '\r' && currentChar != '\n' && hasChar);
        if (stringBuilder.Length > 0)
        {
            string tokenValue = stringBuilder.ToString();
            tokens.Add(new BbCodeToken(EBbCodeTokenType.String, tokenValue));
        }
    }

    private void ReadTag(List<BbCodeToken> outputTokens)
    {
        bool flag = false;
        stringBuilder.Clear();
        stringBuilder.Append(currentChar);
        tagStringBuilder.Clear();
        ReadChar();
        while (hasChar)
        {
            stringBuilder.Append(currentChar);
            if (currentChar == ']')
            {
                ReadChar();
                break;
            }
            if (currentChar == '=')
            {
                flag = true;
                ReadChar();
                break;
            }
            tagStringBuilder.Append(currentChar);
            ReadChar();
        }
        string text = tagStringBuilder.ToString();
        string tokenValue = null;
        if (flag)
        {
            tagStringBuilder.Clear();
            while (hasChar)
            {
                stringBuilder.Append(currentChar);
                if (currentChar == ']')
                {
                    ReadChar();
                    break;
                }
                tagStringBuilder.Append(currentChar);
                ReadChar();
            }
            tokenValue = tagStringBuilder.ToString();
        }
        bool flag2 = false;
        if (text.StartsWith('/'))
        {
            if (string.Equals(text, "/b"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BoldClose));
            }
            else if (string.Equals(text, "/i"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ItalicClose));
            }
            else if (string.Equals(text, "/list"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BulletListClose));
            }
            else if (string.Equals(text, "/olist"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.OrderedListClose));
            }
            else if (string.Equals(text, "/h1"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H1Close));
            }
            else if (string.Equals(text, "/h2"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H2Close));
            }
            else if (string.Equals(text, "/h3"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H3Close));
            }
            else if (string.Equals(text, "/url"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.UrlClose));
            }
            else if (string.Equals(text, "/img"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ImgClose));
            }
            else if (string.Equals(text, "/previewyoutube"))
            {
                outputTokens.Add(new BbCodeToken(EBbCodeTokenType.PreviewYouTubeClose));
            }
            else
            {
                flag2 = true;
            }
        }
        else if (string.Equals(text, "b"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BoldOpen));
        }
        else if (string.Equals(text, "i"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ItalicOpen));
        }
        else if (string.Equals(text, "*"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ListItem));
        }
        else if (string.Equals(text, "list"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.BulletListOpen));
        }
        else if (string.Equals(text, "olist"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.OrderedListOpen));
        }
        else if (string.Equals(text, "h1"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H1Open));
        }
        else if (string.Equals(text, "h2"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H2Open));
        }
        else if (string.Equals(text, "h3"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.H3Open));
        }
        else if (string.Equals(text, "url"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.UrlOpen, tokenValue));
        }
        else if (string.Equals(text, "img"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.ImgOpen));
        }
        else if (string.Equals(text, "previewyoutube"))
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.PreviewYouTubeOpen, tokenValue));
        }
        else
        {
            flag2 = true;
        }
        if (flag2)
        {
            outputTokens.Add(new BbCodeToken(EBbCodeTokenType.String, stringBuilder.ToString()));
        }
    }
}
