namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendRequest
{
    public string Secret { get; set; }
    public BaseFriend Friend { get; set; }

    public CreateOrUpdateFriendRequest()
    {
        Secret = string.Empty;
        Friend = new BaseFriend();
    }

    public CreateOrUpdateFriendRequest(string secret, BaseFriend friend)
    {
        Secret = secret;
        Friend = friend;
    }

    public override string ToString()
    {
        return $"CreateOrUpdateFriendRequest[Secret={Secret}, Friend={Friend}]";
    }
}
