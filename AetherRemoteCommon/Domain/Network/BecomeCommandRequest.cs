namespace AetherRemoteCommon.Domain.Network;

public class BecomeCommandRequest
{
    public string Secret { get; set; }
    public List<string> TargetFriendCodes { get; set; }
    public string GlamourerData { get; set; }
    public GlamourerApplyType GlamourerApplyType { get; set; }
    
    public BecomeCommandRequest()
    {
        Secret = string.Empty;
        TargetFriendCodes = new();
        GlamourerData = string.Empty;
        GlamourerApplyType = GlamourerApplyType.CustomizationAndEquipment;
    }

    public BecomeCommandRequest(string secret, List<string> targetFriendCodes, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
        GlamourerData = glamourerData;
        GlamourerApplyType = glamourerApplyType;
    }

    public override string ToString()
    {
        var friendCodes = string.Join(", ", TargetFriendCodes);
        return $"BecomeCommandRequest[Secret={Secret}, TargetFriendCodes=[{friendCodes}], GlamourerData={GlamourerData}, GlamourerApplyType={GlamourerApplyType}]";
    }
}
