namespace AetherRemoteCommon.Domain.Network.DeleteFriend;

public struct DeleteFriendRequest
{
    public string Secret { get; set; }
    public string FriendCode { get; set; }

    public DeleteFriendRequest(string secret, string friendCode)
    {
        Secret = secret;
        FriendCode = friendCode;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DeleteFriendRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendCode", FriendCode);
        return sb.ToString();
    }
}

public struct DeleteFriendResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public DeleteFriendResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DeleteFriendResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
