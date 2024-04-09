using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendRequest
{
    public string Secret { get; set; }
    public Friend Friend { get; set; }

    public CreateOrUpdateFriendRequest()
    {
        Secret = string.Empty;
        Friend = new Friend();
    }

    public CreateOrUpdateFriendRequest(string secret, Friend friend)
    {
        Secret = secret;
        Friend = friend;
    }

    public override string ToString()
    {
        return $"CreateOrUpdateFriendRequest[Secret={Secret}, Friend={Friend}]";
    }
}
