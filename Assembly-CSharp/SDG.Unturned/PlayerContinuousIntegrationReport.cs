namespace SDG.Unturned;

public class PlayerContinuousIntegrationReport
{
    public int ExitCode;

    public string ErrorMessage;

    public PlayerContinuousIntegrationReport()
    {
        ExitCode = 0;
        ErrorMessage = null;
    }

    public PlayerContinuousIntegrationReport(string ErrorMessage)
    {
        ExitCode = 1;
        this.ErrorMessage = ErrorMessage;
    }
}
