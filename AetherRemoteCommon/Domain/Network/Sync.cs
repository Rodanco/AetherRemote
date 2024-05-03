namespace AetherRemoteCommon.Domain.Network.Sync;

public struct SyncRequest
{
    public string Secret { get; set; }
    public byte[] FriendListHash { get; set; }

    public SyncRequest(string secret, byte[] friendListHash)
    {
        Secret = secret;
        FriendListHash = friendListHash;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SyncRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendListHash", FriendListHash);
        return sb.ToString();
    }
}

public struct SyncResponse
{
    public bool HashesMatch { get; set; }
    public string Message { get; set; }

    public SyncResponse(bool hashesMatch, string message)
    {
        HashesMatch = hashesMatch;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SyncResponse");
        sb.AddVariable("HashesMatch", HashesMatch);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
