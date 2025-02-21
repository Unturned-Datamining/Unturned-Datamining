namespace SDG.Unturned;

/// <summary>
/// Nelson 2025-02-20: Hacking this in to address duplicate buttons when onPublishedAdded is called for a second
/// page of published files. (public issue #4882)
/// </summary>
internal struct PublishedFileUpdateButton
{
    public SteamPublished publishedFile;

    public ISleekButton button;
}
