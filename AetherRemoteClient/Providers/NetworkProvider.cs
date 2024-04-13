using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Translators;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AetherRemoteClient.Providers;

public class NetworkProvider(IPluginLog logger)
{
    // Inject
    private readonly IPluginLog logger = logger;

    // Endpoint
    private const string ConnectionUrl = "http://75.73.3.71:25565/mainHub";

    // Network
    public readonly HubConnection Connection = new HubConnectionBuilder().WithUrl(ConnectionUrl).Build();

    // Data
    public string? FriendCode { get; private set; }

    public async Task<AsyncResult> Connect()
    {
        if (Plugin.DeveloperMode) return AsyncResult.Successful;
        if (Connection.State != HubConnectionState.Disconnected) return AsyncResult.Failure;

        logger.Verbose($"Attempting to connecting to server");

        try
        {
            await Task.Run(() => Connection.StartAsync());

            if (Connection.State == HubConnectionState.Connected)
            {
                logger.Verbose($"Successfully connected to server");
                return new AsyncResult(true, "Successfully connected to server!");
            }
        }
        catch (HttpRequestException)
        {
            logger.Verbose($"Connection refused! The server is likely down");
        }
        catch (Exception ex)
        {
            logger.Verbose(ex.Message);
        }

        logger.Verbose($"Failed to connect to server");
        return new AsyncResult(false, "Failed to connect to server");
    }

    public async Task<AsyncResult> Disconnect()
    {
        if (Plugin.DeveloperMode) return AsyncResult.Successful;
        if (Connection.State == HubConnectionState.Disconnected) return AsyncResult.Successful;
        await Task.Run(() => Connection.StopAsync());
        return AsyncResult.Successful;
    }

    // TODO: Clarify what `friendList` is, and where it comes from. Even explore the possibility of passing the friend list provider into this instance.
    public async Task<AsyncResult> Login(string secret, List<Friend> friendList)
    {
        if (Plugin.DeveloperMode)
        {
            FriendCode = "Dev";
            return AsyncResult.Successful;
        }

        try
        {
            var baseFriendList = FriendTranslator.DomainFriendListToCommon(friendList);
            var request = new LoginRequest(secret, baseFriendList);
            var response = await InvokeCommand<LoginRequest, LoginResponse>(AetherRemoteConstants.ApiLogin, request);

            if (response.Success)
            {
                if (response.RequesterFriendCode == null)
                {
                    logger.Verbose("Login attempt was successful but no friend code was returned");
                    return AsyncResult.Failure;
                }

                FriendCode = response.RequesterFriendCode;

                // Set online status of friends
                // This is slightly hacky, as this friendList is assumed to always be the same
                // Instance of friend list from FriendListProvider
                foreach (var friend in friendList)
                {
                    // OnlineFriends will always have a value when returning a success
                    friend.Online = response.OnlineFriends?.Contains(friend.FriendCode) ?? false;
                }
            }
            else
            {
                logger.Info(response.Message);
            }

            return new AsyncResult(response.Success);
        }
        catch (Exception e)
        {
            logger.Warning($"[Try Login] Error: {e}");
            return new AsyncResult(false, e.Message);
        }
    }

    public async Task<AsyncResult> CreateOrUpdateFriend(string secret, Friend friend)
    {
        if (Plugin.DeveloperMode)
            return AsyncResult.Successful;

        try
        {
            var baseFriend = FriendTranslator.DomainToCommon(friend);
            var request = new CreateOrUpdateFriendRequest(secret, baseFriend);
            var response = await InvokeCommand<CreateOrUpdateFriendRequest, CreateOrUpdateFriendResponse>(AetherRemoteConstants.ApiCreateOrUpdateFriend, request);
            return new AsyncResult(response.Success, response.Message);
        }
        catch (Exception ex)
        {
            logger.Information($"[CreateOrUpdateFriend] Error: {ex.Message}");
            return new AsyncResult(false, ex.Message);
        }
    }

    public async Task<AsyncResult> DeleteFriend(string secret, Friend friend)
    {
        if (Plugin.DeveloperMode)
            return AsyncResult.Successful;

        try
        {
            var request = new DeleteFriendRequest(secret, friend.FriendCode);
            var response = await InvokeCommand<DeleteFriendRequest, DeleteFriendResponse>(AetherRemoteConstants.ApiDeleteFriend, request);
            return new AsyncResult(response.Success);
        }
        catch (Exception ex)
        {
            logger.Information($"[DeleteFriend] Error: {ex.Message}");
            return new AsyncResult(false, ex.Message);
        }
    }

    public async Task<AsyncResult> IssueSpeakCommand(string secret, List<Friend> targetFriends, string message, ChatMode channel, string? extra = null)
    {
        if (Plugin.DeveloperMode)
            return AsyncResult.Successful;

        var friendCodes = targetFriends.Select(friend => friend.FriendCode).ToList();
        var request = new SpeakCommandRequest(secret, friendCodes, message, channel, extra);

        try
        {
            var response = await InvokeCommand<SpeakCommandRequest, SpeakCommandResponse>(AetherRemoteConstants.ApiSpeak, request);
            return new AsyncResult(response.Success, response.Message);
        }
        catch (Exception ex)
        {
            logger.Warning($"Something went wrong issuing speak command: {ex.Message}");
            return new AsyncResult(false, ex.Message);
        }
    }

    public async Task<AsyncResult> IssueEmoteCommand(string secret, List<Friend> targetFriends, string emote)
    {
        if (Plugin.DeveloperMode)
            return AsyncResult.Successful;

        var friendCodes = targetFriends.Select(friend => friend.FriendCode).ToList();
        var request = new EmoteCommandRequest(secret, friendCodes, emote);

        try
        {
            var response = await InvokeCommand<EmoteCommandRequest, EmoteCommandResponse>(AetherRemoteConstants.ApiEmote, request);
            return new AsyncResult(response.Success, response.Message);
        }
        catch (Exception ex)
        {
            logger.Warning($"Something went wrong issuing emote command: {ex.Message}");
            return new AsyncResult(false, ex.Message);
        }
    }

    public async Task<AsyncResult> IssueBecomeCommand(string secret, List<Friend> targetFriends, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        if (Plugin.DeveloperMode)
            return AsyncResult.Successful;

        var friendCodes = targetFriends.Select(friend => friend.FriendCode).ToList();
        var request = new BecomeCommandRequest(secret, friendCodes, glamourerData, glamourerApplyType);

        try
        {
            var response = await InvokeCommand<BecomeCommandRequest, BecomeCommandResponse>(AetherRemoteConstants.ApiBecome, request);
            return new AsyncResult(response.Success, response.Message);
        }
        catch (Exception ex)
        {
            logger.Warning($"Something went wrong issuing become command: {ex.Message}");
            return new AsyncResult(false, ex.Message);
        }
    }

    public async Task<U> InvokeCommand<T, U>(string commandName, T request)
    {
        logger.Info($"[{commandName}] Request: {request}");
        var response = await Connection.InvokeAsync<U>(commandName, request);
        logger.Info($"[{commandName}] Response: {response}");
        return response;
    }
}
