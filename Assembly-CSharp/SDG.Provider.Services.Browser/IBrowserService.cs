namespace SDG.Provider.Services.Browser;

public interface IBrowserService : IService
{
    bool canOpenBrowser { get; }

    void open(string url);
}
