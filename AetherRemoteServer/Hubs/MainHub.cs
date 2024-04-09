using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Hubs;

public class MainHub : Hub
{
    [HubMethodName(AetherRemoteConstants.ApiLogin)]
    public LoginResponse Login(LoginRequest request, INetworkService networkService)
    {
        var friendCode = networkService.GetFriendCode(request.Secret);
        if (friendCode == null)
            return new LoginResponse(false, "Invalid secret");

        var loginResponse = new LoginResponse();

        var registerResult = networkService.Register(request.Secret, Context.ConnectionId, request.FriendList);
        if (registerResult.Success)
            loginResponse.OnlineFriends = networkService.GetOnlineUserFriendCodes();
        else
            loginResponse.Message = registerResult.Message;

        loginResponse.Success = registerResult.Success;
        loginResponse.FriendCode = friendCode;

        return loginResponse;
    }

    [HubMethodName(AetherRemoteConstants.ApiCreateOrUpdateFriend)]
    public CreateOrUpdateFriendResponse CreateOrUpdateFriend(CreateOrUpdateFriendRequest request, INetworkService networkService)
    {
        var isValid = networkService.IsValidSecret(request.Secret);
        if (!isValid)
            return new CreateOrUpdateFriendResponse(false, "Invalid secret");

        var result = networkService.CreateOrUpdateFriend(request.Secret, request.Friend);
        return new CreateOrUpdateFriendResponse(result.Success, result.Message);
    }

    [HubMethodName(AetherRemoteConstants.ApiDeleteFriend)]
    public DeleteFriendResponse DeleteFriend(DeleteFriendRequest request, INetworkService networkService)
    {
        var isValid = networkService.IsValidSecret(request.Secret);
        if (!isValid)
            return new DeleteFriendResponse(false, "Invalid secret");

        var result = networkService.DeleteFriend(request.Secret, request.FriendCode);
        return new DeleteFriendResponse(result.Success, result.Message);
    }

    [HubMethodName(AetherRemoteConstants.ApiSpeak)]
    public SpeakCommandResponse SpeakCommand(SpeakCommandRequest request, INetworkService networkService)
    {
        var isValid = networkService.IsValidSecret(request.Secret);
        if (!isValid)
            return new SpeakCommandResponse(false, "Invalid secret");

        var result = networkService.IssueSpeakCommand(request.Secret, request.Channel, request.Message, 
            request.Extra, request.TargetFriendCodes, Clients);

        return new SpeakCommandResponse(result.Success, result.Message);
    }

    [HubMethodName(AetherRemoteConstants.ApiEmote)]
    public EmoteCommandResponse EmoteCommand(EmoteCommandRequest request, INetworkService networkService)
    {
        var isValid = networkService.IsValidSecret(request.Secret);
        if (!isValid)
            return new EmoteCommandResponse(false, "Invalid secret");

        var result = networkService.IssueEmoteCommand(request.Secret, request.Emote, 
            request.TargetFriendCodes, Clients);

        return new EmoteCommandResponse(result.Success, result.Message);
    }

    [HubMethodName(AetherRemoteConstants.ApiBecome)]
    public BecomeCommandResponse BecomeCommand(BecomeCommandRequest request, INetworkService networkService)
    {
        var isValid = networkService.IsValidSecret(request.Secret);
        if (!isValid)
            return new BecomeCommandResponse(false, "Invalid secret");

        var result = networkService.IssueBecomeCommand(request.Secret, request.GlamourerData, request.GlamourerApplyType,
            request.TargetFriendCodes, Clients);

        return new BecomeCommandResponse(result.Success, result.Message);
    }
}
