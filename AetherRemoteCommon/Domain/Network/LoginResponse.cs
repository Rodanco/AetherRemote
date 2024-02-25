namespace AetherRemoteCommon.Domain.Network;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? FriendCode { get; set; }

    public LoginResponse()
    {
        Success = true;
        Message = string.Empty;
        FriendCode = null;
    }

    public LoginResponse(bool success, string message, string? friendCode = null)
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
    }

    public override string ToString()
    {
        return $"LoginResponse[Success={Success}, Message={Message}, FriendCode={FriendCode}]";
    }
}
