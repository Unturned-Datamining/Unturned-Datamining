namespace SDG.Unturned;

public interface IUnturnedNavmeshInterface
{
    bool ContainsAnyBakedData { get; }

    void Deserialize(River river);

    void Serialize(River river);
}
