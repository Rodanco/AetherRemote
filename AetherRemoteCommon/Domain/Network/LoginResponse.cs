using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network;

public class LoginResponse : CommandResponse
{
    public string? FriendCode { get; set; }

    public LoginResponse()
    {
        FriendCode = null;
    }

    public LoginResponse(bool success, string message, string? friendCode = null) : base(success, message)
    {
        FriendCode = friendCode;
    }

    public override string ToString()
    {
        return $"LoginResponse[Success={Success}, Message={Message}, FriendCode={FriendCode}]";
    }
}
