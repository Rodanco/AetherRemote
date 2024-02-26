namespace AetherRemoteCommon.Domain.Network.Base;

public class CommandRequest
{
    public string Secret { get; set; }
    public List<string> TargetFriendCodes { get; set; }

    public CommandRequest()
    {
        Secret = string.Empty;
        TargetFriendCodes = new List<string>();
    }

    public CommandRequest(string secret, List<string> targetFriendCodes)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
    }

    public override string ToString()
    {
        var friendCodes = string.Join(", ", TargetFriendCodes);
        return $"CommandRequest[Secret={Secret}, TargetFriendCodes=[{friendCodes}]]";
    }
}
