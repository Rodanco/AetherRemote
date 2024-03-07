namespace AetherRemoteCommon.Domain;

[Serializable]
public class FriendPreferences
{
    /// <summary>
    /// If true, ignore commands from associated friend
    /// </summary>
    public bool Muted { get; set; } = false;

    public override string ToString()
    {
        return $"FriendPreferences[Muted={Muted}]";
    }
}
