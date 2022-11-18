using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SDG.Unturned;

public class ThreadedConsoleInputOutput : ConsoleInputOutputBase
{
    private struct PendingOutput
    {
        public string value;

        public ConsoleColor color;
    }

    private Thread inputThread;

    private ConcurrentQueue<string> pendingInputs = new ConcurrentQueue<string>();

    private ConcurrentQueue<PendingOutput> pendingOutputs = new ConcurrentQueue<PendingOutput>();

    public override void initialize(CommandWindow commandWindow)
    {
        base.initialize(commandWindow);
        inputThread = new Thread(consoleMain);
        inputThread.Start();
    }

    public override void shutdown(CommandWindow commandWindow)
    {
        base.shutdown(commandWindow);
        if (inputThread != null)
        {
            inputThread.Abort();
            inputThread = null;
        }
    }

    public override void update()
    {
        base.update();
        string result;
        while (pendingInputs.TryDequeue(out result))
        {
            notifyInputCommitted(result);
        }
    }

    protected override void outputToConsole(string value, ConsoleColor color)
    {
        PendingOutput pendingOutput = default(PendingOutput);
        pendingOutput.value = value;
        pendingOutput.color = color;
        PendingOutput item = pendingOutput;
        pendingOutputs.Enqueue(item);
    }

    private void consoleMain()
    {
        while (true)
        {
            if (!pendingOutputs.IsEmpty)
            {
                ConsoleColor foregroundColor = Console.ForegroundColor;
                PendingOutput result;
                while (pendingOutputs.TryDequeue(out result))
                {
                    Console.ForegroundColor = result.color;
                    Console.WriteLine(result.value);
                }
                Console.ForegroundColor = foregroundColor;
            }
            if (Console.KeyAvailable)
            {
                string text = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    pendingInputs.Enqueue(text);
                }
            }
            Thread.Sleep(10);
        }
    }
}
