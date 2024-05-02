using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Translators;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.CreateOrUpdateFriend;
using AetherRemoteCommon.Domain.Network.DeleteFriend;
using AetherRemoteCommon.Domain.Network.DownloadFriendList;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Login;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Domain.Network.Sync;
using AetherRemoteCommon.Domain.Network.UploadFriendList;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AetherRemoteClient.Providers;

public class NetworkProvider : IDisposable
{
    // Inject
    private readonly IPluginLog logger;

    // Endpoint
    private const string ConnectionUrl = "http://75.73.3.71:25565/mainHub";

    // Network
    public readonly HubConnection Connection = new HubConnectionBuilder().WithUrl(ConnectionUrl).Build();

    // State
    private ServerConnectionState connectionState = ServerConnectionState.Disconnected;
    public ServerConnectionState ConnectionState
    {
        get
        {
            return connectionState;
        }
        set
        {
            if (Plugin.DeveloperMode == false)
                return;

            connectionState = value; 
        }
    }

    // Data
    public string? FriendCode { get; private set; } = null;

    public NetworkProvider(IPluginLog logger)
    {
        this.logger = logger;
        Connection.Closed += Closed;
        Connection.Reconnecting += Reconnecting;
        Connection.Reconnected += Reconnected;
    }

    #region === Connect ===
    public async Task<AsyncResult> Connect(string secret)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        if (Connection.State != HubConnectionState.Disconnected)
            return new AsyncResult(false, "Pending connection in progress");

        connectionState = ServerConnectionState.Connecting;

        var connectionResult = await ConnectToServer();
        if (connectionResult.Success == false)
        {
            connectionState = ServerConnectionState.Disconnected;
            return connectionResult;
        }

        var loginResult = await LoginToServer(secret);
        if (loginResult.Success == false)
        {
            connectionState = ServerConnectionState.Disconnected;
            await Task.Run(() => Connection.StopAsync());
            return loginResult;
        }

        connectionState = ServerConnectionState.Connected;
        return new AsyncResult(true);
    }

    private async Task<AsyncResult> ConnectToServer()
    {
        try
        {
            await Task.Run(() => Connection.StartAsync());

            if (Connection.State == HubConnectionState.Connected)
                return new AsyncResult(true);
        }
        catch (HttpRequestException) { /* Server likely down */ }
        catch (Exception) { /* Something else */ }

        return new AsyncResult(false, "Failed to connect to server");
    }

    private async Task<AsyncResult> LoginToServer(string secret)
    {
        if (Plugin.DeveloperMode)
        {
            FriendCode = "DevMode";
            return new AsyncResult(true, "DeveloperMode Enabled");
        }

        try
        {
            var request = new LoginRequest(secret);
            var response = await InvokeCommand<LoginRequest, LoginResponse>(Constants.ApiLogin, request);
            if (response.Success)
                FriendCode = response.FriendCode;

            return new AsyncResult(response.Success, response.Message);
        }
        catch (Exception ex)
        {
            return new AsyncResult(false, ex.Message);
        }
    }

    public async void Disconnect()
    {
        if (Plugin.DeveloperMode == false)
            await Connection.StopAsync();

        connectionState = ServerConnectionState.Disconnected;
        FriendCode = null;
    }

    #endregion

    #region === Sync ===
    public async Task<AsyncResult> Sync(string secret, string friendListHash)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var request = new SyncRequest(secret, friendListHash);
        var response = await InvokeCommand<SyncRequest, SyncResponse>(Constants.ApiSync, request);
        return new AsyncResult(response.HashesMatch, response.Message);
    }

    public async Task<AsyncResult> UploadFriendList(string secret, List<Friend> friendList)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var convertedFriendList = FriendTranslator.DomainFriendListToCommon(friendList);
        var request = new UploadFriendListRequest(secret, convertedFriendList);
        var response = await InvokeCommand<UploadFriendListRequest, UploadFriendListResponse>(Constants.ApiUploadFriendList, request);
        if (response.Success == false) { /* Some kind of retry? */ }

        return new AsyncResult(response.Success, response.Message);
    }

    public async Task<DownloadFriendListResult> DownloadFriendList(string secret)
    {
        if (Plugin.DeveloperMode)
            return new DownloadFriendListResult(true, "DeveloperMode Enabled", []);

        var request = new DownloadFriendListRequest(secret);
        var response = await InvokeCommand<DownloadFriendListRequest, DownloadFriendListResponse>(Constants.ApiDownloadFriendList, request);
        var friendList = response.Success ? FriendTranslator.CommonFriendListToDomain(response.FriendList) : [];
        return new DownloadFriendListResult(response.Success, response.Message, friendList);
    }
    #endregion

    #region === Friend List ===
    // TODO: Add new domain object for AsyncResult to include Online Status as well
    public async Task<AsyncResult> CreateOrUpdateFriend(string secret, Friend friendToCreateOrUpdate)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");
        
        var friend = FriendTranslator.DomainToCommon(friendToCreateOrUpdate);
        var request = new CreateOrUpdateFriendRequest(secret, friend);
        var response = await InvokeCommand<CreateOrUpdateFriendRequest, CreateOrUpdateFriendResponse>(Constants.ApiCreateOrUpdateFriend, request);
        return new AsyncResult(response.Success, response.Message);
    }

    public async Task<AsyncResult> DeleteFriend(string secret, string friendCode)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var request = new DeleteFriendRequest(secret, friendCode);
        var response = await InvokeCommand<DeleteFriendRequest, DeleteFriendResponse>(Constants.ApiDeleteFriend, request);
        return new AsyncResult(response.Success, response.Message);
    }
    #endregion

    #region === Commands ===
    public async Task<AsyncResult> Become(string secret, List<Friend> targets, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new BecomeRequest(secret, targetFriendCodes, glamourerData, glamourerApplyType);
        var response = await InvokeCommand<BecomeRequest, BecomeResponse>(Constants.ApiBecome, request);
        return new AsyncResult(response.Success, response.Message);
    }

    public async Task<AsyncResult> Emote(string secret, List<Friend> targets, string emote)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new EmoteRequest(secret, targetFriendCodes, emote);
        var response = await InvokeCommand<EmoteRequest, EmoteResponse>(Constants.ApiEmote, request);
        return new AsyncResult(response.Success, response.Message);
    }   

    public async Task<AsyncResult> Speak(string secret, List<Friend> targets, string message, ChatMode chatMode, string? extra)
    {
        if (Plugin.DeveloperMode)
            return new AsyncResult(true, "DeveloperMode Enabled");

        var targetFriendCodes = targets.Select(friend => friend.FriendCode).ToList();
        var request = new SpeakRequest(secret, targetFriendCodes, message, chatMode, extra);
        var response = await InvokeCommand<SpeakRequest, SpeakResponse>(Constants.ApiSpeak, request);
        return new AsyncResult(response.Success, response.Message);
    }
    #endregion

    private async Task<U> InvokeCommand<T, U>(string commandName, T request)
    {
        logger.Info($"[{commandName}] Request: {request}");
        var response = await Connection.InvokeAsync<U>(commandName, request);
        logger.Info($"[{commandName}] Response: {response}");
        return response;
    }

    public async void Dispose()
    {
        GC.SuppressFinalize(this);
        Connection.Reconnecting -= Reconnecting;
        Connection.Reconnected -= Reconnected;
        await Connection.DisposeAsync();
    }

    private async Task Closed(Exception? exception)
    {
        await Task.Run(() => { connectionState = ServerConnectionState.Disconnected; });
    }

    private async Task Reconnecting(Exception? exception)
    {
        await Task.Run(() => { connectionState = ServerConnectionState.Reconnecting; });
    }

    private async Task Reconnected(string? arg)
    {
        await Task.Run(() => { connectionState = ServerConnectionState.Connected; });
    }
}
