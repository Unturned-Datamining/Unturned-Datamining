using System;
using System.IO;

namespace SDG.Unturned;

public class ConsoleOutputRedirector
{
    private Stream standardOutputStream;

    private StreamWriter standardOutputWriter;

    private ConsoleWriterProxy proxyWriter;

    private TextWriter defaultOutputWriter;

    public void enable(bool shouldProxy)
    {
        if (defaultOutputWriter == null)
        {
            defaultOutputWriter = Console.Out;
            standardOutputStream = Console.OpenStandardOutput();
            standardOutputWriter = new StreamWriter(standardOutputStream, Console.OutputEncoding);
            standardOutputWriter.AutoFlush = true;
            if (shouldProxy)
            {
                proxyWriter = new ConsoleWriterProxy(standardOutputWriter, defaultOutputWriter);
                Console.SetOut(proxyWriter);
            }
            else
            {
                Console.SetOut(standardOutputWriter);
            }
        }
    }

    public void disable()
    {
        if (proxyWriter != null)
        {
            proxyWriter.Close();
            proxyWriter.Dispose();
            proxyWriter = null;
        }
        if (standardOutputWriter != null)
        {
            standardOutputWriter.Close();
            standardOutputWriter.Dispose();
            standardOutputWriter = null;
        }
        if (standardOutputStream != null)
        {
            standardOutputStream.Close();
            standardOutputStream.Dispose();
            standardOutputStream = null;
        }
        if (defaultOutputWriter != null)
        {
            Console.SetOut(defaultOutputWriter);
            defaultOutputWriter = null;
        }
    }
}
