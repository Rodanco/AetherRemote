using AetherRemoteCommon.Domain.CommonFriendPermissions;

namespace AetherRemoteCommon.Domain.CommonFriend;

[Serializable]
public class Friend
{
    /// <summary>
    /// Id of the friend (UserId)
    /// </summary>
    public string FriendCode { get; set; }

    /// <summary>
    /// A name set by the client to identify a friend more easily
    /// </summary>
    /// 
    public string? Note { get; set; }

    /// <summary>
    /// Friend preferences
    /// </summary>
    public FriendPermissions Permissions { get; set; }

    public Friend()
    {
        FriendCode = string.Empty;
        Note = null;
        Permissions = new();
    }

    public Friend(string friendCode, string? note = null, FriendPermissions? preferences = null)
    {
        FriendCode = friendCode;
        Note = note;
        Permissions = preferences ?? new();
    }

    public override string ToString()
    {
        return $"CommonFriend[FriendCode={FriendCode}, Note={Note}, Preferences={Permissions}]";
    }
}
