using AetherRemoteCommon.Domain.CommonGlamourerApplyType;

namespace AetherRemoteCommon.Domain.Network.Become;

public struct BecomeRequest
{
    public string Secret { get; set; }
    public List<string> TargetFriendCodes { get; set; }
    public string GlamourerData { get; set; }
    public GlamourerApplyType GlamourerApplyType { get; set; }

    public BecomeRequest(string secret, List<string> targetFriendCodes,
        string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
        GlamourerData = glamourerData;
        GlamourerApplyType = glamourerApplyType;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("GlamourerData", GlamourerData);
        sb.AddVariable("GlamourerApplyType", GlamourerApplyType);
        return sb.ToString();
    }
}

public struct BecomeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public BecomeResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct BecomeExecute
{
    public string SenderFriendCode { get; set; }
    public string GlamourerData { get; set; }
    public GlamourerApplyType GlamourerApplyType { get; set; }

    public BecomeExecute(string senderFriendCode, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        SenderFriendCode = senderFriendCode;
        GlamourerData = glamourerData;
        GlamourerApplyType = glamourerApplyType;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandExecute");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("GlamourerData", GlamourerData);
        sb.AddVariable("GlamourerApplyType", GlamourerApplyType);
        return sb.ToString();
    }
}
