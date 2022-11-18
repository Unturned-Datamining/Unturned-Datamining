using System.IO;
using System.Text;

namespace SDG.Unturned;

public class LogFile
{
    private FileStream stream;

    private StreamWriter writer;

    public string path { get; private set; }

    public LogFile(string path)
    {
        this.path = path;
        stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        Encoding encoding = Encoding.GetEncoding(65001, new EncoderReplacementFallback(), new DecoderReplacementFallback());
        writer = new StreamWriter(stream, encoding);
        writer.AutoFlush = true;
    }

    public void writeLine(string line)
    {
        writer.WriteLine(line);
    }

    public void close()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
            writer.Dispose();
            writer = null;
        }
        if (stream != null)
        {
            stream.Close();
            stream.Dispose();
            stream = null;
        }
    }
}
