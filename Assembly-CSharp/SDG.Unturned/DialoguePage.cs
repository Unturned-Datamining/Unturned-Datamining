namespace SDG.Unturned;

public class DialoguePage
{
    public string text { get; protected set; }

    public DialoguePage(string newText)
    {
        text = newText;
    }
}
