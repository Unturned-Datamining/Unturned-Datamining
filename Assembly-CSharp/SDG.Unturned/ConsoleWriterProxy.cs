using System;
using System.IO;
using System.Text;

namespace SDG.Unturned;

public class ConsoleWriterProxy : TextWriter
{
    protected TextWriter defaultConsoleWriter;

    private StreamWriter customWriter;

    public override Encoding Encoding => defaultConsoleWriter.Encoding;

    public override IFormatProvider FormatProvider => defaultConsoleWriter.FormatProvider;

    public override string NewLine
    {
        get
        {
            return defaultConsoleWriter.NewLine;
        }
        set
        {
            defaultConsoleWriter.NewLine = value;
        }
    }

    public ConsoleWriterProxy(StreamWriter streamWriter, TextWriter defaultConsoleWriter)
    {
        customWriter = streamWriter;
        this.defaultConsoleWriter = defaultConsoleWriter;
    }

    public override void Close()
    {
        customWriter.Close();
        defaultConsoleWriter.Close();
    }

    public override void Flush()
    {
        customWriter.Flush();
        defaultConsoleWriter.Flush();
    }

    /// <summary>
    /// This is the only /required/ override of text writer.
    /// </summary>
    public override void Write(char value)
    {
        customWriter.Write(value);
        defaultConsoleWriter.Write(value);
    }

    public override void WriteLine()
    {
        customWriter.WriteLine();
        defaultConsoleWriter.WriteLine();
    }

    public override void WriteLine(string value)
    {
        customWriter.WriteLine(value);
        defaultConsoleWriter.WriteLine(value);
    }
}
