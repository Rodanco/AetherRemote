namespace AetherRemoteCommon.Domain.Network.Login;

public class LoginRequest
{
    public string Secret { get; set; }

    public LoginRequest(string secret)
    {
        Secret = secret;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginRequest");
        sb.AddVariable("Secret", Secret);
        return sb.ToString();
    }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string FriendCode { get; set; }

    public LoginResponse(bool success, string message, string friendCode)
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("FriendCode", FriendCode);
        return sb.ToString();
    }
}
