using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

/// <summary>
/// Converts Steam BBcode tokens into widgets displayable using Glazier UI.
/// </summary>
public class BbCodeWidgetConverter
{
    private bool _inferLineBreaks;

    private List<BbCodeToken> inputTokens;

    private int inputIndex;

    private bool hasToken;

    private BbCodeToken currentToken;

    private bool hasError;

    private string errorMessage;

    private StringBuilder richTextStringBuilder;

    /// <summary>
    /// If false, expect LineBreak tokens in input. (default false)
    /// If true, insert line breaks where appropriate.
    /// Steam's new visual editor doesn't emit newlines, instead inferring line breaks from paragraph blocks. To
    /// make life easier we will do the same for the main menu announcement feed.
    /// </summary>
    public bool InferLineBreaks
    {
        get
        {
            return _inferLineBreaks;
        }
        set
        {
            _inferLineBreaks = value;
        }
    }

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

    public BbCodeWidgetConverter()
    {
        richTextStringBuilder = new StringBuilder();
    }

    public List<BbCodeWidget> Convert(List<BbCodeToken> tokens)
    {
        richTextStringBuilder.Clear();
        inputTokens = tokens;
        inputIndex = -1;
        hasToken = false;
        List<BbCodeWidget> list = new List<BbCodeWidget>();
        AdvanceToken();
        int num = 0;
        while (hasToken)
        {
            ConvertToken(list);
            num++;
            if (num >= 10000)
            {
                ErrorMessage = "Infinite loop attempting to convert tokens into widgets";
                break;
            }
        }
        return list;
    }

    private void AdvanceToken()
    {
        inputIndex++;
        hasToken = inputIndex < inputTokens.Count;
        if (hasToken)
        {
            currentToken = inputTokens[inputIndex];
        }
    }

    private EBbCodeTokenType PeekNextTokenType()
    {
        if (inputIndex + 1 < inputTokens.Count)
        {
            return inputTokens[inputIndex + 1].tokenType;
        }
        return EBbCodeTokenType.Invalid;
    }

    private void ConvertToken(List<BbCodeWidget> outputWidgets)
    {
        if (currentToken.tokenType == EBbCodeTokenType.PreviewYouTubeOpen)
        {
            ConvertPreviewYouTube(outputWidgets);
        }
        else if (currentToken.tokenType == EBbCodeTokenType.ImgOpen)
        {
            ConvertImage(outputWidgets);
        }
        else if (currentToken.tokenType == EBbCodeTokenType.UrlOpen)
        {
            ConvertLinkButton(outputWidgets);
        }
        else
        {
            ConvertRichText(outputWidgets);
        }
    }

    private void ConvertPreviewYouTube(List<BbCodeWidget> outputWidgets)
    {
        string unquotedValue = currentToken.GetUnquotedValue();
        if (!string.IsNullOrEmpty(unquotedValue))
        {
            int num = unquotedValue.IndexOf(';');
            string widgetData = ((num <= 0) ? unquotedValue : unquotedValue.Substring(0, num));
            outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.YouTubeButton, widgetData));
        }
        AdvanceToken();
        if (hasToken && currentToken.tokenType == EBbCodeTokenType.PreviewYouTubeClose)
        {
            AdvanceToken();
        }
        if (hasToken && currentToken.tokenType == EBbCodeTokenType.LineBreak)
        {
            AdvanceToken();
        }
    }

    private void ConvertImage(List<BbCodeWidget> outputWidgets)
    {
        string value;
        bool flag = currentToken.TryParseValue("src", out value);
        AdvanceToken();
        if (hasToken && currentToken.tokenType == EBbCodeTokenType.String)
        {
            if (!flag)
            {
                value = currentToken.tokenValue;
            }
            outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.Image, value));
            AdvanceToken();
            if (hasToken && currentToken.tokenType == EBbCodeTokenType.ImgClose)
            {
                AdvanceToken();
            }
        }
        else if (hasToken && currentToken.tokenType == EBbCodeTokenType.ImgClose)
        {
            if (flag)
            {
                outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.Image, value));
            }
            AdvanceToken();
        }
        if (hasToken && currentToken.tokenType == EBbCodeTokenType.LineBreak)
        {
            AdvanceToken();
        }
    }

    private void ConvertLinkButton(List<BbCodeWidget> outputWidgets)
    {
        string text = currentToken.GetUnquotedValue();
        string text2 = null;
        AdvanceToken();
        if (hasToken && currentToken.tokenType == EBbCodeTokenType.String)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = currentToken.tokenValue;
            }
            else
            {
                text2 = currentToken.tokenValue;
            }
            AdvanceToken();
        }
        if (hasToken && currentToken.tokenType == EBbCodeTokenType.UrlClose)
        {
            AdvanceToken();
        }
        if (hasToken && currentToken.tokenType == EBbCodeTokenType.LineBreak)
        {
            AdvanceToken();
        }
        if (string.IsNullOrEmpty(text2))
        {
            outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.LinkButton, text));
        }
        else
        {
            outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.LinkButton, text + "," + text2));
        }
    }

    private void ConvertRichText(List<BbCodeWidget> outputWidgets)
    {
        richTextStringBuilder.Clear();
        bool flag = false;
        int num = 0;
        bool flag2 = true;
        do
        {
            bool flag3 = false;
            switch (currentToken.tokenType)
            {
            case EBbCodeTokenType.String:
                richTextStringBuilder.Append(currentToken.tokenValue);
                break;
            case EBbCodeTokenType.BoldOpen:
                richTextStringBuilder.Append("<b>");
                break;
            case EBbCodeTokenType.BoldClose:
                richTextStringBuilder.Append("</b>");
                break;
            case EBbCodeTokenType.ListItemClose:
            case EBbCodeTokenType.ParagraphClose:
                if (_inferLineBreaks)
                {
                    if (!flag2)
                    {
                        richTextStringBuilder.Append('\n');
                    }
                    switch (PeekNextTokenType())
                    {
                    case EBbCodeTokenType.H1Open:
                    case EBbCodeTokenType.H2Open:
                    case EBbCodeTokenType.H3Open:
                    case EBbCodeTokenType.ParagraphOpen:
                        richTextStringBuilder.Append('\n');
                        break;
                    }
                }
                flag3 = true;
                break;
            case EBbCodeTokenType.ItalicOpen:
                richTextStringBuilder.Append("<i>");
                break;
            case EBbCodeTokenType.ItalicClose:
                richTextStringBuilder.Append("</i>");
                break;
            case EBbCodeTokenType.H1Open:
                if (!flag2 && _inferLineBreaks)
                {
                    richTextStringBuilder.Append("\n\n");
                }
                flag3 = true;
                richTextStringBuilder.Append("<size=20>");
                break;
            case EBbCodeTokenType.H1Close:
                richTextStringBuilder.Append("</size>");
                if (_inferLineBreaks)
                {
                    richTextStringBuilder.Append("\n\n");
                }
                flag3 = true;
                break;
            case EBbCodeTokenType.H2Open:
                if (!flag2 && _inferLineBreaks)
                {
                    richTextStringBuilder.Append("\n\n");
                }
                flag3 = true;
                richTextStringBuilder.Append("<size=17>");
                break;
            case EBbCodeTokenType.H2Close:
                richTextStringBuilder.Append("</size>");
                if (_inferLineBreaks)
                {
                    richTextStringBuilder.Append("\n\n");
                }
                flag3 = true;
                break;
            case EBbCodeTokenType.H3Open:
                if (!flag2 && _inferLineBreaks)
                {
                    richTextStringBuilder.Append("\n\n");
                }
                flag3 = true;
                richTextStringBuilder.Append("<size=14>");
                break;
            case EBbCodeTokenType.H3Close:
                richTextStringBuilder.Append("</size>");
                if (_inferLineBreaks)
                {
                    richTextStringBuilder.Append("\n\n");
                }
                flag3 = true;
                break;
            case EBbCodeTokenType.BulletListOpen:
            case EBbCodeTokenType.BulletListClose:
                PushPendingRichText(outputWidgets);
                flag3 = true;
                break;
            case EBbCodeTokenType.OrderedListOpen:
                flag = true;
                num = 0;
                PushPendingRichText(outputWidgets);
                flag3 = true;
                break;
            case EBbCodeTokenType.OrderedListClose:
                flag = false;
                PushPendingRichText(outputWidgets);
                flag3 = true;
                break;
            case EBbCodeTokenType.ListItemOpen:
                if (!flag2 && _inferLineBreaks)
                {
                    richTextStringBuilder.Append('\n');
                }
                flag3 = true;
                if (flag)
                {
                    richTextStringBuilder.Append(num + 1);
                    richTextStringBuilder.Append(". ");
                    num++;
                }
                else
                {
                    richTextStringBuilder.Append("• ");
                }
                break;
            case EBbCodeTokenType.LineBreak:
                richTextStringBuilder.Append('\n');
                flag3 = true;
                break;
            case EBbCodeTokenType.QuoteOpen:
                if (string.IsNullOrEmpty(currentToken.tokenValue))
                {
                    richTextStringBuilder.Append("<indent=2em>");
                }
                else
                {
                    richTextStringBuilder.Append("<indent=2em><b>" + currentToken.tokenValue + ":</b>\n");
                }
                break;
            case EBbCodeTokenType.QuoteClose:
                richTextStringBuilder.Append("</indent>");
                if (_inferLineBreaks)
                {
                    richTextStringBuilder.Append('\n');
                }
                flag3 = true;
                break;
            }
            EBbCodeTokenType tokenType = currentToken.tokenType;
            AdvanceToken();
            if (currentToken.tokenType == EBbCodeTokenType.PreviewYouTubeOpen || currentToken.tokenType == EBbCodeTokenType.ImgOpen || (currentToken.tokenType == EBbCodeTokenType.UrlOpen && (tokenType == EBbCodeTokenType.LineBreak || tokenType == EBbCodeTokenType.ParagraphOpen)))
            {
                break;
            }
            flag2 = flag3;
        }
        while (hasToken);
        PushPendingRichText(outputWidgets);
    }

    private void PushPendingRichText(List<BbCodeWidget> outputWidgets)
    {
        if (richTextStringBuilder.Length > 0)
        {
            string widgetData = richTextStringBuilder.ToString();
            outputWidgets.Add(new BbCodeWidget(EBbCodeWidgetType.RichTextLabel, widgetData));
            richTextStringBuilder.Clear();
        }
    }
}
