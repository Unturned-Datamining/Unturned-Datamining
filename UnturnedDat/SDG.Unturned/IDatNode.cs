using System.Text;

namespace SDG.Unturned;

public interface IDatNode
{
    EDatNodeType NodeType { get; }

    bool IsMetadataAvailable { get; }

    void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0);

    bool TryGetParsedComment(out DatComment comment);

    bool TryGetParentNode(out IDatNode parentNode);

    bool TryGetParsedLineNumber(out int lineNumber);

    bool TryGetParsedLineNumberRange(out int startingLineNumber, out int endingLineNumber);
}
