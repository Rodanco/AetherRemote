using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Translators;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AetherRemoteClient.Services.Network;

public class NetworkCommandInvoker
{
    private readonly NetworkService networkService;
    private readonly HubConnection connection;
    private readonly IPluginLog logger;
    private readonly SaveService saveService;

    public NetworkCommandInvoker(HubConnection connection, NetworkService networkService, SaveService saveService, IPluginLog logger)
    {
        this.networkService = networkService;
        this.saveService = saveService;
        this.logger = logger;
        this.connection = connection;
    }

    public async Task<bool> Login()
    {
        if (Plugin.DeveloperMode)
        {
            networkService.FriendCode = "Dev";
            return true;
        }

        var secret = saveService.Secret;
        var friendList = saveService.FriendList;

        try
        {
            var baseFriendList = FriendTranslator.DomainFriendListToCommon(friendList);
            var request = new LoginRequest(secret, baseFriendList);
            var response = await InvokeCommand<LoginRequest, LoginResponse>(AetherRemoteConstants.ApiLogin, request);

            if (response.Success)
            {
                networkService.FriendCode = response.FriendCode;
            }
            else
            {
                logger.Info(response.Message);
            }

            return response.Success;
        }
        catch (Exception e)
        {
            logger.Warning($"[Try Login] Error: {e}");
        }

        return false;
    }

    public async Task<bool> CreateOrUpdateFriend(Friend friend)
    {
        var secret = saveService.Secret;

        try
        {
            var baseFriend = FriendTranslator.DomainToCommon(friend);
            var request = new CreateOrUpdateFriendRequest(secret, baseFriend);
            var response = await InvokeCommand<CreateOrUpdateFriendRequest, CreateOrUpdateFriendResponse>(AetherRemoteConstants.ApiCreateOrUpdateFriend, request);
            return response.Success;
        }
        catch(Exception ex)
        {
            logger.Information($"[CreateOrUpdateFriend] Error: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> DeleteFriend(Friend friend)
    {
        var secret = saveService.Secret;

        try
        {
            var request = new DeleteFriendRequest(secret, friend.FriendCode);
            var response = await InvokeCommand<DeleteFriendRequest, DeleteFriendResponse>(AetherRemoteConstants.ApiDeleteFriend, request);
            return response.Success;
        }
        catch (Exception ex)
        {
            logger.Information($"[DeleteFriend] Error: {ex.Message}");
        }

        return false;
    }

    public async void IssueSpeakCommand(List<Friend> targetFriends, string message, ChatMode channel, string? extra = null)
    {
        var secret = saveService.Secret;
        var friendCodes = targetFriends.Select(friend => friend.FriendCode).ToList();

        // TODO: Sanitize
        var request = new SpeakCommandRequest(secret, friendCodes, message, channel, extra);

        try
        {
            var response = await InvokeCommand<SpeakCommandRequest, SpeakCommandResponse>(AetherRemoteConstants.ApiSpeak, request);
        }
        catch (Exception ex)
        {
            logger.Info(ex.Message);
        }
    }

    public async void IssueEmoteCommand(List<Friend> targetFriends, string emote)
    {
        var secret = saveService.Secret;
        var friendCodes = targetFriends.Select(friend => friend.FriendCode).ToList();

        // TODO: Sanitize
        var request = new EmoteCommandRequest(secret, friendCodes, emote);

        try
        {
            var response = await InvokeCommand<EmoteCommandRequest, EmoteCommandResponse>(AetherRemoteConstants.ApiEmote, request);
        }
        catch (Exception ex)
        {
            logger.Info(ex.Message);
        }
    }

    public async void IssueBecomeCommand(List<Friend> targetFriends, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        var secret = saveService.Secret;
        var friendCodes = targetFriends.Select(friend => friend.FriendCode).ToList();

        // TODO: Sanitize
        var request = new BecomeCommandRequest(secret, friendCodes, glamourerData, glamourerApplyType);

        try
        {
            var response = await InvokeCommand<BecomeCommandRequest, BecomeCommandResponse>(AetherRemoteConstants.ApiBecome, request);
        }
        catch (Exception ex)
        {
            logger.Info(ex.Message);
        }
    }

    public async Task<U> InvokeCommand<T, U>(string commandName, T request)
    {
        logger.Info($"[{commandName}] Request: {request}");
        var response = await connection.InvokeAsync<U>(commandName, request);
        logger.Info($"[{commandName}] Response: {response}");
        return response;
    }
}
