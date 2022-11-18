using Steamworks;

namespace SDG.Provider.Services.Workshop;

public interface IWorkshopService : IService
{
    bool canOpenWorkshop { get; }

    void open(PublishedFileId_t id);
}
