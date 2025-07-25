namespace SDG.Unturned;

public interface IEditableDatNode : IDatNode
{
    public enum ESortingPreference
    {
        TowardBack,
        TowardFront
    }

    string Comment { get; set; }

    int PreferredLineNumber { get; set; }

    int TopMargin { get; set; }

    int BottomMargin { get; set; }

    ESortingPreference SortingPreference { get; set; }
}
