namespace AetherRemoteCommon.Domain.Network.Emote;

public class EmoteCommandRequest
{
    public string Secret;
    public List<string> TargetFriendCodes;
    public string Emote;

    public EmoteCommandRequest()
    {
        Secret = string.Empty;
        TargetFriendCodes = new();
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
        var sb = new AetherRemoteStringBuilder("EmoteCommandRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("Emote", Emote);
        return sb.ToString();
    }
}
