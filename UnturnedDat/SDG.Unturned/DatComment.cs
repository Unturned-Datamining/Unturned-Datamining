using System;
using System.Text;

namespace SDG.Unturned;

public struct DatComment
{
    private static string[] messageLineBreaks = new string[2] { "\r\n", "\n" };

    public string[] MessageLines { get; set; }

    public int StartingLineNumber { get; set; }

    public int EndingLineNumber { get; set; }

    public bool AreMessageLinesNullOrEmpty
    {
        get
        {
            if (MessageLines == null || MessageLines.Length < 1)
            {
                return true;
            }
            if (MessageLines.Length == 1)
            {
                return string.IsNullOrEmpty(MessageLines[0]);
            }
            return false;
        }
    }

    public string MessageWithLineBreaks
    {
        get
        {
            return JoinLines('\n');
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                MessageLines = null;
            }
            else
            {
                MessageLines = value.Split(messageLineBreaks, StringSplitOptions.None);
            }
        }
    }

    public string JoinLines(char separator)
    {
        if (MessageLines == null || MessageLines.Length < 1)
        {
            return null;
        }
        if (MessageLines.Length == 1)
        {
            return MessageLines[0];
        }
        return string.Join(separator, MessageLines);
    }

    public string JoinLines(string separator)
    {
        if (MessageLines == null || MessageLines.Length < 1)
        {
            return null;
        }
        if (MessageLines.Length == 1)
        {
            return MessageLines[0];
        }
        return string.Join(separator, MessageLines);
    }

    public void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0)
    {
        if (MessageLines == null || MessageLines.Length < 1)
        {
            return;
        }
        string[] messageLines = MessageLines;
        foreach (string value in messageLines)
        {
            for (int j = 0; j < indentationLevel + 1; j++)
            {
                output.Append('\t');
            }
            if (!string.IsNullOrEmpty(value))
            {
                output.Append("// ");
                output.AppendLine(value);
            }
            else
            {
                output.AppendLine("//");
            }
        }
    }

    public override string ToString()
    {
        if (StartingLineNumber == EndingLineNumber)
        {
            return $"(Line: {StartingLineNumber} Message: \"{MessageWithLineBreaks}\")";
        }
        return $"(Lines: {StartingLineNumber}-{EndingLineNumber} Message: \"{MessageWithLineBreaks}\")";
    }

    public DatComment(string message)
    {
        MessageLines = null;
        StartingLineNumber = 0;
        EndingLineNumber = 0;
        MessageWithLineBreaks = message;
    }
}
