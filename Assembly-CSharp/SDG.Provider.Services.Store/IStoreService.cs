namespace SDG.Provider.Services.Store;

public interface IStoreService : IService
{
    /// <summary>
    /// View a package on the store.
    /// </summary>
    /// <param name="packageID">Package to view.</param>
    void open(IStorePackageID packageID);
}
