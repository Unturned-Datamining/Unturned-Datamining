namespace SDG.Unturned;

public interface IEditableDatValue : IDatValue, IDatNode, IEditableDatNode
{
    string InlineComment { get; set; }
}
