namespace AetherRemoteCommon.Domain.Network;

public class DeleteFriendRequest
{
    public string Secret { get; set; }
    public string FriendCode { get; set; }

    public DeleteFriendRequest()
    {
        Secret = string.Empty;
        FriendCode = string.Empty;
    }

    public DeleteFriendRequest(string secret, string friendCode)
    {
        Secret = secret;
        FriendCode = friendCode;
    }

    public override string ToString()
    {
        return $"DeleteFriendRequest[Secret={Secret}, FriendCode={FriendCode}]";
    }
}
