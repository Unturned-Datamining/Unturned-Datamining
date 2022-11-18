namespace SDG.Unturned;

public interface IContentReference
{
    string name { get; set; }

    string path { get; set; }

    bool isValid { get; }
}
