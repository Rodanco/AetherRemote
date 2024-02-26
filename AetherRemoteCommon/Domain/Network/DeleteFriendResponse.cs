using AetherRemoteCommon.Domain.Network.Base;

namespace AetherRemoteCommon.Domain.Network;

public class DeleteFriendResponse : CommandResponse
{
    public DeleteFriendResponse() { }
    public DeleteFriendResponse(bool success, string message) : base(success, message) { }

    public override string ToString()
    {
        return $"DeleteFriendResponse[Success={Success}, Message={Message}]";
    }
}
