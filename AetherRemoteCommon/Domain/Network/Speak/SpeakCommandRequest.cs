using AetherRemoteCommon.Domain.CommonChatMode;

namespace AetherRemoteCommon.Domain.Network.Speak;

public class SpeakCommandRequest
{
    public string Secret;
    public List<string> TargetFriendCodes;
    public string Message;
    public ChatMode Channel;
    public string? Extra;

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
        var sb = new AetherRemoteStringBuilder("SpeakCommandRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("Message", Message);
        sb.AddVariable("Channel", Channel);
        sb.AddVariable("Extra", Extra);
        return sb.ToString();
    }
}
