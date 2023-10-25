namespace SDG.Provider.Services;

public interface IService
{
    /// <summary>
    /// Initialize this service's external API. Should be called before using.
    /// </summary>
    void initialize();

    /// <summary>
    /// Update this service's external API. Should be called every frame.
    /// </summary>
    void update();

    /// <summary>
    /// Shutdown this service's external API. Should be called before closing the program.
    /// </summary>
    void shutdown();
}
