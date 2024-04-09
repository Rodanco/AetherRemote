namespace AetherRemoteCommon.Domain.Network;

public class LoginResponse
{
    public bool Success;
    public string Message;
    public string? FriendCode;
    public HashSet<string>? OnlineFriends;

    public LoginResponse()
    {
        Success = false;
        Message = string.Empty;
        FriendCode = null;
        OnlineFriends = null;
    }

    public LoginResponse(bool success, string message, string? friendCode = null, HashSet<string>? onlineFriends = null)
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
        OnlineFriends = onlineFriends;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("OnlineFriends", OnlineFriends);
        return sb.ToString();
    }
}
