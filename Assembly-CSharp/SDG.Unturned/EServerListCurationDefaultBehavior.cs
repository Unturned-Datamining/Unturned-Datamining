namespace SDG.Unturned;

/// <summary>
/// Determines how a server that doesn't match any rules is handled.
/// </summary>
internal enum EServerListCurationDefaultBehavior
{
    /// <summary>
    /// Include in the list. Default.
    /// </summary>
    Show,
    /// <summary>
    /// Exclude from list. (same as EServerListCurationDenyMode.Hide)
    /// </summary>
    Hide,
    /// <summary>
    /// Move to the bottom of the list. Similar to EServerListCurationDenyMode.MoveToBottom, but the server is
    /// still clickable. I.e., low priority.
    /// </summary>
    MoveToBottom
}
