namespace SDG.Unturned;

public enum ENPCEventReplicationMode
{
    /// <summary>
    /// Do not replicate to clients. Run the event on the listen server (singleplayer) / dedicated server.
    /// Equivalent to the `shouldReplicate = false` parameter.
    /// Default.
    /// </summary>
    AuthorityOnly,
    /// <summary>
    /// Replicate to clients. Run the event everywhere.
    /// Replaces the `shouldReplicate = true` parameter.
    /// </summary>
    AuthorityAndClients,
    /// <summary>
    /// Only runs the event for the instigating player.
    /// </summary>
    InstigatorOnly
}
