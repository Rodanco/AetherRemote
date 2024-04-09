using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendRequest
{
    public string Secret;
    public Friend Friend;

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
        var sb = new AetherRemoteStringBuilder("CreateOrUpdateFriendRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("Friend", Friend);
        return sb.ToString();
    }
}
