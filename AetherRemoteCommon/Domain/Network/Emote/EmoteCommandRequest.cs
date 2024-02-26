using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Emote;

public class EmoteCommandRequest : CommandRequest
{
    public string Emote { get; set; }

    public EmoteCommandRequest()
    {
        Emote = string.Empty;
    }

    public EmoteCommandRequest(string secret, List<string> targetFriendCodes, 
        string emote) : base(secret, targetFriendCodes)
    {
        Emote = emote;
    }

    public override string ToString()
    {
        var friendCodes = string.Join(", ", TargetFriendCodes);
        return $"EmoteCommandRequest[Secret={Secret}, TargetFriendCodes=[{friendCodes}], Emote={Emote}]";
    }
}
