using System.Text;

namespace SDG.Unturned;

public interface IDatNode
{
    EDatNodeType NodeType { get; }

    void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0);

    string DebugDumpToString();
}
