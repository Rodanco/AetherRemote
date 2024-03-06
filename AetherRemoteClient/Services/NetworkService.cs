using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Services;

public class NetworkService
{
    // Injected
    private readonly IPluginLog logger;
    private readonly ISanitizer sanitizer;

    // Providers
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly EmoteProvider emoteProvider;
    private readonly FriendListProvider friendListProvider;

    public NetworkService(IPluginLog logger, DalamudPluginInterface pluginInterface, NetworkProvider networkProvider, 
        ActionQueueProvider actionQueueProvider, EmoteProvider emoteProvider, FriendListProvider friendListProvider)
    {
        this.logger = logger;
        this.sanitizer = pluginInterface.Sanitizer;

        this.actionQueueProvider = actionQueueProvider;
        this.emoteProvider = emoteProvider;
        this.friendListProvider = friendListProvider;

        networkProvider.Connection.On(AetherRemoteConstants.ApiBecome,
            (BecomeCommandExecute execute) => { HandleBecomeCommand(execute); });

        networkProvider.Connection.On(AetherRemoteConstants.ApiEmote,
            (EmoteCommandExecute execute) => { HandleEmoteCommand(execute); });

        networkProvider.Connection.On(AetherRemoteConstants.ApiSpeak,
            (SpeakCommandExecute execute) => { HandleSpeakCommand(execute); });
    }

    public void HandleBecomeCommand(BecomeCommandExecute becomeCommand)
    {
        logger.Info($"HandleBecomeCommand recieved: {becomeCommand}");

        var senderFriend = friendListProvider.FindFriend(becomeCommand.SenderFriendCode);
        if (senderFriend == null)
        {
            logger.Info($"Recieved message from someone not on your friend list: {becomeCommand.SenderFriendCode}");
            return;
        }

        actionQueueProvider.EnqueueBecomeAction(becomeCommand.SenderFriendCode, becomeCommand.GlamourerData, becomeCommand.GlamourerApplyType);
    }

    public void HandleEmoteCommand(EmoteCommandExecute emoteCommand)
    {
        logger.Info($"HandleEmoteCommand recieved: {emoteCommand}");

        var senderFriend = friendListProvider.FindFriend(emoteCommand.SenderFriendCode);
        if (senderFriend == null)
        {
            logger.Info($"Recieved message from someone not on your friend list: {emoteCommand.SenderFriendCode}");
            return;
        }

        var validEmote = emoteProvider.Emotes.Contains(emoteCommand.Emote);
        if (validEmote == false)
        {
            logger.Info($"Recieved invalid emote from server: [{emoteCommand.Emote}]");
            return;
        }

        actionQueueProvider.EnqueueEmoteAction(senderFriend.NoteOrId, emoteCommand.Emote);
    }

    public void HandleSpeakCommand(SpeakCommandExecute speakCommand)
    {
        logger.Info($"HandleSpeakCommand: {speakCommand}");

        var senderFriend = friendListProvider.FindFriend(speakCommand.SenderFriendCode);
        if (senderFriend == null)
        {
            logger.Info($"Recieved message from someone not on your friend list: {speakCommand.SenderFriendCode}");
            return;
        }

        var sanitizedMessage = sanitizer.Sanitize(speakCommand.Message);
        var sanitizedExtra = speakCommand.Extra == null ? null : sanitizer.Sanitize(speakCommand.Extra);

        actionQueueProvider.EnqueueSpeakAction(senderFriend.NoteOrId, sanitizedMessage, speakCommand.Channel, sanitizedExtra);
    }
}
