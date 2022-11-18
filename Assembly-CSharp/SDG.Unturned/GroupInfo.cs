using Steamworks;

namespace SDG.Unturned;

public class GroupInfo
{
    public string name;

    public uint members;

    public CSteamID groupID { get; private set; }

    public bool useMaxGroupMembersLimit => Provider.modeConfigData.Gameplay.Max_Group_Members != 0;

    public bool hasSpaceForMoreMembersInGroup
    {
        get
        {
            if (useMaxGroupMembersLimit)
            {
                return members < Provider.modeConfigData.Gameplay.Max_Group_Members;
            }
            return true;
        }
    }

    public GroupInfo(CSteamID newGroupID, string newName, uint newMembers)
    {
        groupID = newGroupID;
        name = newName;
        members = newMembers;
    }
}
