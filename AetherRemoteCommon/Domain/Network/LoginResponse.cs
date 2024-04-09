using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network;

public class LoginResponse : CommandResponse
{
    public string? FriendCode { get; set; }
    public HashSet<string>? OnlineFriends { get; set; }

    public LoginResponse()
    {
        FriendCode = null;
        OnlineFriends = null;
    }

    public LoginResponse(bool success, string message, string? friendCode = null, HashSet<string>? onlineFriends = null) : base(success, message)
    {
        FriendCode = friendCode;
        OnlineFriends = onlineFriends;
    }

    public override string ToString()
    {
        return $"LoginResponse[Success={Success}, Message={Message}, FriendCode={FriendCode}, OnlineFriends={OnlineFriends}]";
    }
}
