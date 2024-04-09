using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Speak;

public class SpeakCommandRequest : CommandRequest
{
    public string Message { get; set; }
    public ChatMode Channel { get; set; }
    public string? Extra { get; set; }

    public SpeakCommandRequest()
    {
        Message = string.Empty;
        Channel = ChatMode.Say;
        Extra = null;
    }

    public SpeakCommandRequest(string secret, List<string> targetFriendCodes,
        string message, ChatMode channel, string? extra = null) : base(secret, targetFriendCodes)
    {
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
