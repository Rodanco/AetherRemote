namespace AetherRemoteCommon.Domain.Network;

public class SpeakCommandRequest
{
    public string Secret { get; set; }
    public List<string> TargetFriendCodes { get; set; }
    public string Message { get; set; }
    public ChatMode Channel { get; set; }
    public string? Extra { get; set; }

    public SpeakCommandRequest()
    {
        Secret = string.Empty;
        TargetFriendCodes = new();
        Message = string.Empty;
        Channel = ChatMode.Say;
        Extra = null;
    }

    public SpeakCommandRequest(string secret, List<string> targetFriendCodes, 
        string message, ChatMode channel, string? extra = null)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
        Message = message;
        Channel = channel;
        Extra = extra;
    }

    public override string ToString()
    {
        var friendCodes = string.Join(", ", TargetFriendCodes);
        return $"SpeakCommandRequest[Secret={Secret}, TargetFriendCodes=[{friendCodes}], Message={Message}, Channel={Channel}, Extra={Extra}]";
    }
}
