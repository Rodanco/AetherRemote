using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Become;

public class BecomeCommandRequest : CommandRequest
{
    public string GlamourerData { get; set; }
    public GlamourerApplyType GlamourerApplyType { get; set; }

    public BecomeCommandRequest()
    {
        GlamourerData = string.Empty;
        GlamourerApplyType = GlamourerApplyType.CustomizationAndEquipment;
    }

    public BecomeCommandRequest(string secret, List<string> targetFriendCodes, string glamourerData, 
        GlamourerApplyType glamourerApplyType): base(secret, targetFriendCodes)
    {
        GlamourerData = glamourerData;
        GlamourerApplyType = glamourerApplyType;
    }

    public override string ToString()
    {
        var friendCodes = string.Join(", ", TargetFriendCodes);
        return $"BecomeCommandRequest[Secret={Secret}, TargetFriendCodes=[{friendCodes}], GlamourerData={GlamourerData}, GlamourerApplyType={GlamourerApplyType}]";
    }
}
