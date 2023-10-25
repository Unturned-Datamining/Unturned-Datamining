namespace SDG.Provider.Services.Browser;

public interface IBrowserService : IService
{
    /// <summary>
    /// Whether the user has their overlay enabled.
    /// </summary>
    bool canOpenBrowser { get; }

    void open(string url);
}
