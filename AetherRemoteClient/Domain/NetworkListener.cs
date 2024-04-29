using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Listens for commands from the server. Will also perform validation and enqueue actions to <see cref="ActionQueueProvider"/>
/// </summary>
public class NetworkListener
{
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly EmoteProvider emoteProvider;
    private readonly FriendListProvider friendListProvider;
    private readonly IPluginLog logger;

    /// <summary>
    /// Listens for commands from the server. Will also perform validation and enqueue actions to <see cref="ActionQueueProvider"/>
    /// </summary>
    public NetworkListener(
        ActionQueueProvider actionQueueProvider,
        EmoteProvider emoteProvider,
        FriendListProvider friendListProvider,
        NetworkProvider networkProvider, 
        IPluginLog logger)
    {
        this.actionQueueProvider = actionQueueProvider;
        this.friendListProvider = friendListProvider;
        this.emoteProvider = emoteProvider;
        this.logger = logger;

        networkProvider.Connection.On(Constants.ApiBecome, (BecomeExecute execute) => { HandleBecome(execute); });
        networkProvider.Connection.On(Constants.ApiEmote, (EmoteExecute execute) => { HandleEmote(execute); });
        networkProvider.Connection.On(Constants.ApiSpeak, (SpeakExecute execute) => { HandleSpeak(execute); });
    }

    public void HandleBecome(BecomeExecute execute)
    {
        var validFriend = friendListProvider.FriendList.FirstOrDefault(friend => friend.FriendCode == execute.SenderFriendCode);
        if (validFriend == null)
        {
            var message = $"Filtered out \'Become\' command from {execute.SenderFriendCode} who is not on your friend list";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var hasPermission = PermissionChecker.HasGlamourerPermission(execute.GlamourerApplyType, validFriend.Permissions);
        if (hasPermission == false)
        {
            var message = $"Filtered out \'Become\' command from {execute.SenderFriendCode} who does not have {execute.GlamourerApplyType} permissions";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }
        
        actionQueueProvider.EnqueueBecomeAction(execute.SenderFriendCode, execute.GlamourerData, execute.GlamourerApplyType);
    }

    public void HandleEmote(EmoteExecute execute)
    {
        var validFriend = friendListProvider.FriendList.FirstOrDefault(friend => friend.FriendCode == execute.SenderFriendCode);
        if (validFriend == null)
        {
            var message = $"Filtered out \'Emote\' command from {execute.SenderFriendCode} who is not on your friend list";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var hasPermission = PermissionChecker.HasEmotePermission(validFriend.Permissions);
        if (hasPermission == false)
        {
            var message = $"Filtered out \'Emote\' command from {execute.SenderFriendCode} who does not have Emote permissions";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var validEmote = emoteProvider.Emotes.Any(emote => emote == execute.Emote);
        if (validEmote == false)
        {
            var message = $"Filtered out \'Emote\' command from {execute.SenderFriendCode} that contained an invalid emote";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        actionQueueProvider.EnqueueEmoteAction(execute.SenderFriendCode, execute.Emote);
    }

    public void HandleSpeak(SpeakExecute execute)
    {
        var validFriend = friendListProvider.FriendList.FirstOrDefault(friend => friend.FriendCode == execute.SenderFriendCode);
        if (validFriend == null)
        {
            var message = $"Filtered out \'Speak\' command from {execute.SenderFriendCode} who is not on your friend list";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        var hasPermission = PermissionChecker.HasSpeakPermission(execute.ChatMode, validFriend.Permissions);
        if (hasPermission == false)
        {
            var message = $"Filtered out \'Speak\' command from {execute.SenderFriendCode} who does not have {execute.ChatMode} permissions";
            AetherRemoteLogging.Log(execute.SenderFriendCode, message, DateTime.Now, LogType.Error);
            return;
        }

        // TODO: The idea that someone could go 'rogue' and make you say offensive language
        // is something that has been on my mind. Ideally, a toggleable 'safe mode' should
        // be implemented where undesired language can be filtered out.

        actionQueueProvider.EnqueueSpeakAction(execute.SenderFriendCode, execute.Message, execute.ChatMode, execute.Extra);
    } 
}
