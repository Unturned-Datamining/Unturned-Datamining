namespace SDG.Unturned;

public interface ICommandInputOutput
{
    event CommandInputHandler inputCommitted;

    void initialize(CommandWindow commandWindow);

    void shutdown(CommandWindow commandWindow);

    void update();

    void outputInformation(string information);

    void outputWarning(string warning);

    void outputError(string error);
}
