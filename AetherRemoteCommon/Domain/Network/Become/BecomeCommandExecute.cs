using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Become;

public class BecomeCommandExecute : CommandExecute
{
    public string GlamourerData { get; set; }
    public GlamourerApplyType GlamourerApplyType { get; set; }

    public BecomeCommandExecute()
    {
        GlamourerData = string.Empty;
        GlamourerApplyType = GlamourerApplyType.CustomizationAndEquipment;
    }

    public BecomeCommandExecute(string senderFriendCode, string glamourerData, 
        GlamourerApplyType glamourerApplyType) : base(senderFriendCode)
    {
        GlamourerData = glamourerData;
        GlamourerApplyType = glamourerApplyType;
    }

    public override string ToString()
    {
        return $"BecomeCommandExecute=[SenderFriendCode={SenderFriendCode}, GlamourerData={GlamourerData}, GlamourerApplyType={GlamourerApplyType}]";
    }
}
