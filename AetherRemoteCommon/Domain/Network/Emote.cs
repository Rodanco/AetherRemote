namespace AetherRemoteCommon.Domain.Network.Emote;

public struct EmoteRequest
{
    public string Secret { get; set; }
    public List<string> TargetFriendCodes { get; set; }
    public string Emote { get; set; }

    public EmoteRequest(string secret, List<string> targetFriendCodes, string emote)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
        Emote = emote;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteCommandRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("Emote", Emote);
        return sb.ToString();
    }
}

public struct EmoteResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public EmoteResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct EmoteExecute
{
    public string SenderFriendCode { get; set; }
    public string Emote { get; set; }

    public EmoteExecute(string senderFriendCode, string emote)
    {
        SenderFriendCode = senderFriendCode;
        Emote = emote;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteCommandExecute");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("Emote", Emote);
        return sb.ToString();
    }
}
