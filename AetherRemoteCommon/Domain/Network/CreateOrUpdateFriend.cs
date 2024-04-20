using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network.CreateOrUpdateFriend;

public class CreateOrUpdateFriendRequest
{
    public string Secret { get; set; }
    public Friend Friend { get; set; }

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

public class CreateOrUpdateFriendResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public bool Online { get; set; }

    public CreateOrUpdateFriendResponse(bool success, string message, bool online = false)
    {
        Success = success;
        Message = message;
        Online = online;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("Online", Online);
        return sb.ToString();
    }
}
