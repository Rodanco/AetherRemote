namespace AetherRemoteCommon.Domain;

[Serializable]
public class CommonFriend
{
    /// <summary>
    /// Id of the friend (UserId)
    /// </summary>
    public string FriendCode { get; set; }

    /// <summary>
    /// A name set by the client to identify a friend more easily
    /// </summary>
    /// 

    // TODO: Convert this to a normal string. All logic should check against string.Empty or "".
    public string? Note { get; set; }

    /// <summary>
    /// Friend preferences
    /// </summary>
    public FriendPreferences Preferences { get; set; }

    public CommonFriend()
    {
        FriendCode = string.Empty;
        Note = null;
        Preferences = new();
    }

    public CommonFriend(string friendCode, string? note = null, FriendPreferences? preferences = null)
    {
        FriendCode = friendCode;
        Note = note;
        Preferences = preferences ?? new();
    }

    public override string ToString()
    {
        return $"CommonFriend[FriendCode={FriendCode}, Note={Note}, Preferences={Preferences}]";
    }
}
