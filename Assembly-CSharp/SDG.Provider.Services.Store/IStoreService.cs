namespace SDG.Provider.Services.Store;

public interface IStoreService : IService
{
    void open(IStorePackageID packageID);
}
