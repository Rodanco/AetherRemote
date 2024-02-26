using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Become;

public class BecomeCommandResponse : CommandResponse
{
    public BecomeCommandResponse() { }
    public BecomeCommandResponse(bool success, string message) : base(success, message) { }

    public override string ToString()
    {
        return $"BecomeCommandResponse[Success={Success}, Message={Message}]";
    }
}
