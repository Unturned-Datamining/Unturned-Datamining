using System;
using System.IO;

namespace SDG.Unturned;

public class ConsoleInputRedirector
{
    private Stream standardInputStream;

    private StreamReader standardInputReader;

    private TextReader defaultInputReader;

    public void enable()
    {
        if (defaultInputReader == null)
        {
            defaultInputReader = Console.In;
            standardInputStream = Console.OpenStandardInput();
            standardInputReader = new StreamReader(standardInputStream, Console.InputEncoding);
            Console.SetIn(standardInputReader);
        }
    }

    public void disable()
    {
        if (standardInputReader != null)
        {
            standardInputReader.Close();
            standardInputReader.Dispose();
            standardInputReader = null;
        }
        if (standardInputStream != null)
        {
            standardInputStream.Close();
            standardInputStream.Dispose();
            standardInputStream = null;
        }
        if (defaultInputReader != null)
        {
            Console.SetIn(defaultInputReader);
            defaultInputReader = null;
        }
    }
}
