namespace AetherRemoteCommon.Domain;

[Serializable]
public class FriendPreferences
{
    /// <summary>
    /// If true, ignore commands from associated friend
    /// </summary>
    public bool Muted { get; set; } = false;
}
