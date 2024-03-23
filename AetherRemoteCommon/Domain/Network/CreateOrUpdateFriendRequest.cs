namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendRequest
{
    public string Secret { get; set; }
    public CommonFriend Friend { get; set; }

    public CreateOrUpdateFriendRequest()
    {
        Secret = string.Empty;
        Friend = new CommonFriend();
    }

    public CreateOrUpdateFriendRequest(string secret, CommonFriend friend)
    {
        Secret = secret;
        Friend = friend;
    }

    public override string ToString()
    {
        return $"CreateOrUpdateFriendRequest[Secret={Secret}, Friend={Friend}]";
    }
}
