using AetherRemoteClient.Components;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text;

namespace AetherRemoteClient.Services;

public class NetworkService
{
    // Injected
    private readonly IPluginLog logger;
    private readonly ISanitizer sanitizer;

    // Providers
    private readonly ActionQueueProvider actionQueueProvider;
    private readonly EmoteProvider emoteProvider;

    public NetworkService(IPluginLog logger, DalamudPluginInterface pluginInterface, NetworkProvider networkProvider, 
        ActionQueueProvider actionQueueProvider, EmoteProvider emoteProvider)
    {
        this.logger = logger;
        this.sanitizer = pluginInterface.Sanitizer;

        this.actionQueueProvider = actionQueueProvider;
        this.emoteProvider = emoteProvider;

        networkProvider.Connection.On(AetherRemoteConstants.ApiSpeak,
            (SpeakCommandExecute execute) => { HandleSpeakCommand(execute); });

        networkProvider.Connection.On(AetherRemoteConstants.ApiEmote,
            (EmoteCommandExecute execute) => { HandleEmoteCommand(execute); });

        networkProvider.Connection.On(AetherRemoteConstants.ApiBecome,
            (BecomeCommandExecute execute) => { HandleBecomeCommand(execute); });
    }

    public void HandleSpeakCommand(SpeakCommandExecute speakCommand)
    {
        // TODO: Client-Side validation???

        logger.Info($"HandleSpeakCommand: {speakCommand}");

        var chatCommand = new StringBuilder();

        chatCommand.Append('/');
        chatCommand.Append(speakCommand.Channel.ToChatCommand());

        if (speakCommand.Channel == ChatMode.Linkshell || speakCommand.Channel == ChatMode.CrossworldLinkshell)
            chatCommand.Append(speakCommand.Extra);

        chatCommand.Append(' ');

        if (speakCommand.Channel == ChatMode.Tell)
            chatCommand.Append(speakCommand.Extra);

        chatCommand.Append(speakCommand.Message);

        var finalSanitizedString = sanitizer.Sanitize(chatCommand.ToString());
        actionQueueProvider.EnqueueChatAction(speakCommand.SenderFriendCode, finalSanitizedString);
    }

    public void HandleEmoteCommand(EmoteCommandExecute emoteCommand)
    {
        // TODO: Client-Side validation???

        logger.Info($"HandleEmoteCommand recieved: {emoteCommand}");

        var validEmote = emoteProvider.Emotes.Contains(emoteCommand.Emote);
        if (validEmote == false)
        {
            logger.Info($"Got invalid emote from server: [{emoteCommand.Emote}]");
        }

        var completedChatCommand = $"/{emoteCommand.Emote} motion";
        actionQueueProvider.EnqueueChatAction(emoteCommand.SenderFriendCode, completedChatCommand);
    }

    public void HandleBecomeCommand(BecomeCommandExecute execute)
    {
        // TODO: Client-Side validation???
        logger.Info($"HandleBecomeCommand recieved: {execute}");

        actionQueueProvider.EnqueueGlamourerAction(execute.SenderFriendCode, execute.GlamourerData, execute.GlamourerApplyType);
    }
}
