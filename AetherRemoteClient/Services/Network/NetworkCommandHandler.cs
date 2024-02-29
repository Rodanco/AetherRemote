using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using Dalamud.Game.Text.Sanitizer;
using Dalamud.Plugin.Services;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text;

namespace AetherRemoteClient.Services.Network;

public class NetworkCommandHandler : IDisposable
{
    private readonly EmoteService emoteService;
    private readonly ActionQueueService actionQueueService;
    private readonly IPluginLog logger;
    private readonly ISanitizer sanitizer;

    public NetworkCommandHandler(
        HubConnection connection,
        ActionQueueService actionQueueService,
        EmoteService emoteService,
        GlamourerAccessor glamourerAccessor,
        IPluginLog logger, 
        ISanitizer sanitizer, 
        IClientState clientState)
    {
        this.logger = logger;
        this.sanitizer = sanitizer;
        this.emoteService = emoteService;
        this.actionQueueService = actionQueueService;

        connection.On(AetherRemoteConstants.ApiSpeak, 
            (SpeakCommandExecute execute) => { HandleSpeakCommand(execute); });

        connection.On(AetherRemoteConstants.ApiEmote, 
            (EmoteCommandExecute execute) => { HandleEmoteCommand(execute); });

        connection.On(AetherRemoteConstants.ApiBecome,
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
        actionQueueService.EnqueueChatAction(speakCommand.SenderFriendCode, finalSanitizedString);
    }

    public void HandleEmoteCommand(EmoteCommandExecute emoteCommand)
    {
        // TODO: Client-Side validation???

        logger.Info($"HandleEmoteCommand recieved: {emoteCommand}");

        var validEmote = emoteService.GetEmotes().Contains(emoteCommand.Emote);
        if (validEmote == false)
        {
            logger.Info($"Got invalid emote from server: [{emoteCommand.Emote}]");
        }

        var completedChatCommand = $"/{emoteCommand.Emote} motion";
        actionQueueService.EnqueueChatAction(emoteCommand.SenderFriendCode, completedChatCommand);
    }

    public void HandleBecomeCommand(BecomeCommandExecute execute)
    {
        // TODO: Client-Side validation???
        logger.Info($"HandleBecomeCommand recieved: {execute}");

        actionQueueService.EnqueueGlamourerAction(execute.SenderFriendCode, execute.GlamourerData, execute.GlamourerApplyType);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
