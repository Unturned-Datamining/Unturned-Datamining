namespace SDG.Unturned;

public interface IDatValue : IDatNode
{
    string Value { get; set; }

    bool TryGetParsedInlineComment(out string inlineComment);

    IEditableDatValue Edit();
}
