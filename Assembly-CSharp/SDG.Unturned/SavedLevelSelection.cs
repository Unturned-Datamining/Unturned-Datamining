namespace SDG.Unturned;

/// <summary>
/// Matches level with same file name AND workshop file ID.
/// </summary>
internal struct SavedLevelSelection
{
    public string name;

    public ulong workshopFileId;

    public void Read(Block block)
    {
        name = block.readString();
        workshopFileId = block.readUInt64();
    }

    public void Write(Block block)
    {
        block.writeString(name);
        block.writeUInt64(workshopFileId);
    }

    public void Clear()
    {
        name = string.Empty;
        workshopFileId = 0uL;
    }

    public SavedLevelSelection(LevelInfo level)
    {
        if (level != null)
        {
            name = level.name;
            workshopFileId = level.publishedFileId;
        }
        else
        {
            name = string.Empty;
            workshopFileId = 0uL;
        }
    }
}
