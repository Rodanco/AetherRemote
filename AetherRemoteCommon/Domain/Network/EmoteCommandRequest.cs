namespace AetherRemoteCommon.Domain.Network;

public class EmoteCommandRequest
{
    public string Secret { get; set; }
    public List<string> TargetFriendCodes { get; set; }
    public string Emote { get; set; }

    public EmoteCommandRequest()
    {
        Secret = string.Empty;
        TargetFriendCodes = new List<string>();
        Emote = string.Empty;
    }

    public EmoteCommandRequest(string secret, List<string> targetFriendCodes, string emote)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
        Emote = emote;
    }

    public override string ToString()
    {
        var friendCodes = string.Join(", ", TargetFriendCodes);
        return $"EmoteCommandRequest[Secret={Secret}, TargetFriendCodes=[{friendCodes}], Emote={Emote}]";
    }
}
