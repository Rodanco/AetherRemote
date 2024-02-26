using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network;

public class CreateOrUpdateFriendResponse : CommandResponse
{
    public CreateOrUpdateFriendResponse() { }
    public CreateOrUpdateFriendResponse(bool success, string message) : base(success, message) { }

    public override string ToString()
    {
        return $"CreateOrUpdateFriendResponse[Success={Success}, Message={Message}]";
    }
}
