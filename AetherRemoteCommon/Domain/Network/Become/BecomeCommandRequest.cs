using AetherRemoteCommon.Domain.CommonGlamourerApplyType;

namespace AetherRemoteCommon.Domain.Network.Become;

public class BecomeCommandRequest
{
    public string Secret;
    public List<string> TargetFriendCodes;
    public string GlamourerData;
    public GlamourerApplyType GlamourerApplyType;

    public BecomeCommandRequest()
    {
        Secret = string.Empty;
        TargetFriendCodes = new();
        GlamourerData = string.Empty;
        GlamourerApplyType = GlamourerApplyType.CustomizationAndEquipment;
    }

    public BecomeCommandRequest(string secret, List<string> targetFriendCodes, 
        string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        Secret = secret;
        TargetFriendCodes = targetFriendCodes;
        GlamourerData = glamourerData;
        GlamourerApplyType = glamourerApplyType;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("GlamourerData", GlamourerData);
        sb.AddVariable("GlamourerApplyType", GlamourerApplyType);
        return sb.ToString();
    }
}
