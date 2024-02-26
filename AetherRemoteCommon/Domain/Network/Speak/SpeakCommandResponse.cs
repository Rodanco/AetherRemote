using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Speak;

public class SpeakCommandResponse : CommandResponse
{
    public SpeakCommandResponse() { }
    public SpeakCommandResponse(bool success, string message) : base(success, message) { }

    public override string ToString()
    {
        return $"SpeakCommandResponse[Success={Success}, Message={Message}]";
    }
}
