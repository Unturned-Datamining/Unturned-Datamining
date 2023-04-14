using System.Text;

namespace SDG.Unturned;

public interface IDatNode
{
    void DebugDumpToStringBuilder(StringBuilder output, int indentationLevel = 0);

    string DebugDumpToString();
}
