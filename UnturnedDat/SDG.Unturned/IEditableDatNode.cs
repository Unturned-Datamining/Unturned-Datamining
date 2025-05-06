namespace SDG.Unturned;

public interface IEditableDatNode
{
    string Comment { get; set; }

    int PreferredLineNumber { get; set; }

    int TopMargin { get; set; }

    int BottomMargin { get; set; }
}
