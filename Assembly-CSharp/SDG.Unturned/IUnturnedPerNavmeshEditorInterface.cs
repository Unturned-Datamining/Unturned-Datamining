namespace SDG.Unturned;

public interface IUnturnedPerNavmeshEditorInterface
{
    int GraphIndexForUI { get; }

    void OnDestroy();

    void Bake();
}
