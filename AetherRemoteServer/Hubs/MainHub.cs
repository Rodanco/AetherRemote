using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Hubs;

/**
 * Basic rundown of the server architecture:
 * A cilent will connect to the server. A connected client cannot access any API on the
 * server until they register. A client must register by providing a secret, which will
 * be validated, and return a corresponding friend code if successful. This process is repeated
 * for each request to prevent spoofing.
 * 
 * MainHub <-> NetworkService <-> StorageService
 * 
 * Typical Request:
 * MainHub calls NetworkService with a secret to validate
 * NetworkService returns a friend code to MainHub
 * MainHub calls a method in NetworkService with the previously returned friend code
 * NetworkService executes the method and returns the result to MainHub
 */

public class MainHub : Hub
{
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    [HubMethodName(AetherRemoteConstants.ApiLogin)]
    public LoginResponse Login(LoginRequest request, INetworkService networkService)
    {
        var friendCode = networkService.TryGetRequesterFriendCode(request.Secret);
        if (friendCode == null)
            return new LoginResponse(false, "Invalid secret");

        var loginResponse = new LoginResponse();
        var registerResult = networkService.Register(request.Secret, Context.ConnectionId, request.FriendList);
        if (registerResult.Success)
        {
            var onlineFriends = new HashSet<string>();
            foreach (var friend in request.FriendList)
            {
                var onlineFriend = networkService.TryGetOnlineUser(friend.FriendCode);
                if (onlineFriend == null)
                    continue;

                onlineFriends.Add(friend.FriendCode);
            }

            loginResponse.OnlineFriends = onlineFriends;

            // TODO: Think of a way to provide context of both messages.
            // If networkService.Register succeeds but networkService.GetOnlineStatusOfFriends
            // does not, then the error message from networkService.GetOnlineStatusOfFriends will
            // become overwritten. In its current state these two will only fail for the same
            // reason, but in the future that may not be the case.
        }

        loginResponse.Success = registerResult.Success;
        loginResponse.Message = registerResult.Message;
        loginResponse.RequesterFriendCode = friendCode;

        return loginResponse;
    }

    [HubMethodName(AetherRemoteConstants.ApiCreateOrUpdateFriend)]
    public CreateOrUpdateFriendResponse CreateOrUpdateFriend(CreateOrUpdateFriendRequest request, INetworkService networkService)
    {
        // Check if secret is valid
        var requesterFriendCode = networkService.TryGetRequesterFriendCode(request.Secret);
        if (requesterFriendCode == null)
            return new CreateOrUpdateFriendResponse(false, "Invalid secret");

        // Check if requester is registered
        var requesterUser = networkService.TryGetOnlineUser(requesterFriendCode);
        if (requesterUser == null)
            return new CreateOrUpdateFriendResponse(false, "Not registered");

        // Attempt to add or update a friend
        var result = networkService.CreateOrUpdateFriend(requesterUser, request.Friend);

        // Check to see if the friend is online
        var friendOnline = networkService.TryGetOnlineUser(request.Friend.FriendCode) != null;

        return new CreateOrUpdateFriendResponse(result.Success, result.Message, friendOnline);
    }

    [HubMethodName(AetherRemoteConstants.ApiDeleteFriend)]
    public DeleteFriendResponse DeleteFriend(DeleteFriendRequest request, INetworkService networkService)
    {
        // Check if secret is valid
        var requesterFriendCode = networkService.TryGetRequesterFriendCode(request.Secret);
        if (requesterFriendCode == null)
            return new DeleteFriendResponse(false, "Invalid secret");

        // Check if requester is registered
        var requesterUser = networkService.TryGetOnlineUser(requesterFriendCode);
        if (requesterUser == null)
            return new DeleteFriendResponse(false, "Not registered");

        var result = networkService.DeleteFriend(requesterUser, request.FriendCode);
        return new DeleteFriendResponse(result.Success, result.Message);
    }

    [HubMethodName(AetherRemoteConstants.ApiSpeak)]
    public SpeakCommandResponse SpeakCommand(SpeakCommandRequest request, INetworkService networkService)
    {
        // Check if secret is valid
        var issuerFriendCode = networkService.TryGetRequesterFriendCode(request.Secret);
        if (issuerFriendCode == null)
            return new SpeakCommandResponse(false, "Invalid secret");

        // Check if requester is registered
        var requesterUser = networkService.TryGetOnlineUser(issuerFriendCode);
        if (requesterUser == null)
            return new SpeakCommandResponse(false, "Not registered");

        var result = networkService.IssueSpeakCommand(issuerFriendCode, request.Channel, request.Message, 
            request.Extra, request.TargetFriendCodes, Clients);

        return new SpeakCommandResponse(result.Success, result.Message);
    }

    [HubMethodName(AetherRemoteConstants.ApiEmote)]
    public EmoteCommandResponse EmoteCommand(EmoteCommandRequest request, INetworkService networkService)
    {
        // Check if secret is valid
        var issuerFriendCode = networkService.TryGetRequesterFriendCode(request.Secret);
        if (issuerFriendCode == null)
            return new EmoteCommandResponse(false, "Invalid secret");

        // Check if requester is registered
        var requesterUser = networkService.TryGetOnlineUser(issuerFriendCode);
        if (requesterUser == null)
            return new EmoteCommandResponse(false, "Not registered");

        var result = networkService.IssueEmoteCommand(issuerFriendCode, request.Emote, 
            request.TargetFriendCodes, Clients);

        return new EmoteCommandResponse(result.Success, result.Message);
    }

    [HubMethodName(AetherRemoteConstants.ApiBecome)]
    public BecomeCommandResponse BecomeCommand(BecomeCommandRequest request, INetworkService networkService)
    {
        // Check if secret is valid
        var issuerFriendCode = networkService.TryGetRequesterFriendCode(request.Secret);
        if (issuerFriendCode == null)
            return new BecomeCommandResponse(false, "Invalid secret");

        // Check if requester is registered
        var requesterUser = networkService.TryGetOnlineUser(issuerFriendCode);
        if (requesterUser == null)
            return new BecomeCommandResponse(false, "Not registered");

        var result = networkService.IssueBecomeCommand(issuerFriendCode, request.GlamourerData, request.GlamourerApplyType,
            request.TargetFriendCodes, Clients);

        return new BecomeCommandResponse(result.Success, result.Message);
    }
}
