using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network.CreateOrUpdateFriend;
using AetherRemoteCommon.Domain.Network.DeleteFriend;
using AetherRemoteCommon.Domain.Network.DownloadFriendList;
using AetherRemoteCommon.Domain.Network.Login;
using AetherRemoteCommon.Domain.Network.Sync;
using AetherRemoteCommon.Domain.Network.UploadFriendList;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Hubs;

public class MainHub : Hub
{
    public static readonly NetworkService NetworkService = new();

    [HubMethodName(Constants.ApiLogin)]
    public LoginResponse Login(LoginRequest request)
    {
        var connectionId = Context.ConnectionId;
        var result = NetworkService.Login(connectionId, request.Secret);
        return new LoginResponse(result.Success, result.Message, result.Extra);
    }

    [HubMethodName(Constants.ApiSync)]
    public SyncResponse Sync(SyncRequest request)
    {
        var result = NetworkService.Sync(request.Secret, request.FriendListHash);
        return new SyncResponse(result.Success, result.Message);
    }

    [HubMethodName(Constants.ApiDownloadFriendList)]
    public DownloadFriendListResponse DownloadFriendList(DownloadFriendListRequest request)
    {
        var result = NetworkService.FetchFriendList(request.Secret);
        return new DownloadFriendListResponse(result.Success, result.Message, result.FriendList);
    }

    [HubMethodName(Constants.ApiUploadFriendList)]
    public UploadFriendListResponse UploadFriendList(UploadFriendListRequest request)
    {
        var result = NetworkService.UpdateFriendList(request.Secret, request.FriendList);
        return new UploadFriendListResponse(result.Success, result.Message);
    }

    [HubMethodName(Constants.ApiCreateOrUpdateFriend)]
    public CreateOrUpdateFriendResponse CreateOrUpdateFriend(CreateOrUpdateFriendRequest request)
    {
        var result = NetworkService.CreateOrUpdateFriend(request.Secret, request.Friend);
        return new CreateOrUpdateFriendResponse(result.Success, result.Message);
    }

    [HubMethodName(Constants.ApiDeleteFriend)]
    public DeleteFriendResponse DeleteFriend(DeleteFriendRequest request)
    {
        var result = NetworkService.DeleteFriend(request.Secret, request.FriendCode);
        return new DeleteFriendResponse(result.Success, result.Message);
    }

    // TODO: Add Emote, Speak, and Become commands + Filtering

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up the disconnected client
        NetworkService.Logout(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
