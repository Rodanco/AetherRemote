namespace AetherRemoteCommon.Domain.Network.Sync;

public class SyncRequest
{
    public string Secret { get; set; }
    public string FriendListHash { get; set; }

    public SyncRequest(string secret, string friendListHash)
    {
        Secret = secret;
        FriendListHash = friendListHash;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SyncRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendListHash", FriendListHash);
        return sb.ToString();
    }
}//

public class SyncResponse
{
    public bool HashesMatch { get; set; }
    public string Message { get; set; }

    public SyncResponse(bool hashesMatch, string message)
    {
        HashesMatch = hashesMatch;
        Message = message;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SyncResponse");
        sb.AddVariable("HashesMatch", HashesMatch);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
