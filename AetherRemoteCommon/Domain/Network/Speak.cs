using AetherRemoteCommon.Domain.CommonChatMode;

namespace AetherRemoteCommon.Domain.Network.Speak;

public struct SpeakRequest
{
    public string Secret { get; set; }
    public List<string> TargetFriendCodes { get; set; }
    public string Message { get; set; }
    public ChatMode Channel { get; set; }
    public string? Extra { get; set; }

    public SpeakRequest(string secret, List<string> targetFriendCodes,
        string message, ChatMode channel, string? extra = null)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
        Message = message;
        Channel = channel;
        Extra = extra;
    }

    public override readonly string ToString()
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

public struct SpeakResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public SpeakResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SpeakCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct SpeakExecute
{
    public string SenderFriendCode { get; set; }
    public string Message { get; set; }
    public ChatMode Channel { get; set; }
    public string? Extra { get; set; }

    public SpeakExecute(string senderFriendCode, string message, ChatMode channel, string? extra)
    {
        SenderFriendCode = senderFriendCode;
        Message = message;
        Channel = channel;
        Extra = extra;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SpeakCommandExecute");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("Message", Message);
        sb.AddVariable("Channel", Channel);
        sb.AddVariable("Extra", Extra);
        return sb.ToString();
    }
}
