namespace SDG.Unturned;

internal interface IMetadataPreservingDatWriterCompatible
{
    EDatNodeType NodeType { get; }

    DatComment? WriterGetPrefixComment();

    string WriterGetInlineComment();

    int WriterGetEarliestLineNumber();

    int WriterGetLatestLineNumber();

    void WriterGetSortingParameters(out int lineNumber, out int sortOrder);

    void WriterGetMargins(out int upperMargin, out int lowerMargin);
}
