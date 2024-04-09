namespace AetherRemoteCommon.Domain.Network;

public class DeleteFriendRequest
{
    public string Secret;
    public string FriendCode;

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
        var sb = new AetherRemoteStringBuilder("DeleteFriendRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendCode", FriendCode);
        return sb.ToString();
    }
}
