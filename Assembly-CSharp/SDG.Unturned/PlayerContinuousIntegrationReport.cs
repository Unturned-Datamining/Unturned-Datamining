namespace SDG.Unturned;

public class PlayerContinuousIntegrationReport
{
    /// <summary>
    /// Error code that the server exited with.
    /// 0 is succesful, anything else is an error.
    /// </summary>
    public int ExitCode;

    /// <summary>
    /// Empty if successful,
    /// otherwise an explanation of the first error encountered.
    /// </summary>
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
