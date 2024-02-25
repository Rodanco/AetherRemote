namespace AetherRemoteCommon.Domain;

[Serializable]
public class BaseFriend
{
    /// <summary>
    /// Id of the friend (UserId)
    /// </summary>
    public string FriendCode { get; set; }

    /// <summary>
    /// A name set by the client to identify a friend more easily
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Friend preferences
    /// </summary>
    public FriendPreferences? Preferences { get; set; }

    public BaseFriend()
    {
        FriendCode = string.Empty;
        Note = null;
        Preferences = null;
    }

    public BaseFriend(string friendCode, string? note = null, FriendPreferences? preferences = null)
    {
        FriendCode = friendCode;
        Note = note;
        Preferences = preferences;
    }

    public override string ToString()
    {
        return $"BaseFriend[FriendCode={FriendCode}, Note={Note}, Preferences={Preferences}]";
    }
}
