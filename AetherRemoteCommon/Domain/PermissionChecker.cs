using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriendPermissions;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;

namespace AetherRemoteCommon.Domain;

public static class PermissionChecker
{
    public static bool HasGlamourerPermission(GlamourerApplyType applyType, FriendPermissions permissions)
    {
        return applyType switch
        {
            GlamourerApplyType.Customization => permissions.AllowChangeAppearance,
            GlamourerApplyType.Equipment => permissions.AllowChangeEquipment,
            GlamourerApplyType.CustomizationAndEquipment => permissions.AllowChangeAppearance || permissions.AllowChangeAppearance,
            _ => false,
        };
    }

    public static bool HasSpeakPermission(ChatMode chatMode, FriendPermissions permissions)
    {
        return chatMode switch
        {
            ChatMode.Alliance => permissions.AllowAlliance,
            ChatMode.CrossworldLinkshell => permissions.AllowCrossworldLinkshell,
            ChatMode.FreeCompany => permissions.AllowFreeCompany,
            ChatMode.Linkshell => permissions.AllowLinkshell,
            ChatMode.Party => permissions.AllowParty,
            ChatMode.PvPTeam => permissions.AllowPvPTeam,
            ChatMode.Say => permissions.AllowSay,
            ChatMode.Shout => permissions.AllowShout,
            ChatMode.Tell => permissions.AllowTell,
            ChatMode.Yell => permissions.AllowYell,
            _ => false,
        };
    }

    public static bool HasEmotePermission(FriendPermissions permissions)
    {
        return permissions.AllowEmote;
    }
}
