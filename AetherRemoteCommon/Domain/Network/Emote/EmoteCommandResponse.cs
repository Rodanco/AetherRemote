using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network.Emote;

public class EmoteCommandResponse : CommandResponse
{
    public EmoteCommandResponse() { }
    public EmoteCommandResponse(bool success, string message) : base(success, message) { }

    public override string ToString()
    {
        return $"EmoteCommandResponse[Success={Success}, Message={Message}]";
    }
}
