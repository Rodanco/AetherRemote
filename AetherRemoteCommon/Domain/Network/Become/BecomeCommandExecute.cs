using AetherRemoteCommon.Domain.CommonGlamourerApplyType;

namespace AetherRemoteCommon.Domain.Network.Become;

public class BecomeCommandExecute
{
    public string SenderFriendCode;
    public string GlamourerData;
    public GlamourerApplyType GlamourerApplyType;

    public BecomeCommandExecute()
    {
        SenderFriendCode = string.Empty;
        GlamourerData = string.Empty;
        GlamourerApplyType = GlamourerApplyType.CustomizationAndEquipment;
    }

    public BecomeCommandExecute(string senderFriendCode, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        SenderFriendCode = senderFriendCode;
        GlamourerData = glamourerData;
        GlamourerApplyType = glamourerApplyType;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BecomeCommandExecute");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("GlamourerData", GlamourerData);
        sb.AddVariable("GlamourerApplyType", GlamourerApplyType);
        return sb.ToString();
    }
}
