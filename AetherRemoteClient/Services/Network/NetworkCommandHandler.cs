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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XivCommon.Functions;

namespace AetherRemoteClient.Services.Network;

public class NetworkCommandHandler
{
    private readonly Chat chat;
    private readonly EmoteService emoteService;
    private readonly HubConnection connection;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly IPluginLog logger;
    private readonly ISanitizer sanitizer;
    private readonly IClientState clientState;

    public NetworkCommandHandler(
        HubConnection connection,
        Chat chat,
        EmoteService emoteService,
        GlamourerAccessor glamourerAccessor,
        IPluginLog logger, 
        ISanitizer sanitizer, 
        IClientState clientState)
    {
        this.chat = chat;
        this.logger = logger;
        this.sanitizer = sanitizer;
        this.glamourerAccessor = glamourerAccessor;
        this.clientState = clientState;
        this.emoteService = emoteService;
        this.connection = connection;

        connection.On(AetherRemoteConstants.ApiSpeak, 
            (SpeakCommandExecute execute) => { HandleSpeakCommand(execute); });

        connection.On(AetherRemoteConstants.ApiEmote, 
            (EmoteCommandExecute execute) => { HandleEmoteCommand(execute); });

        connection.On(AetherRemoteConstants.ApiBecome,
            (BecomeCommandExecute execute) => { HandleBecomeCommand(execute); });
    }

    public void HandleSpeakCommand(SpeakCommandExecute execute)
    {
        logger.Info($"HandleSpeakCommand: {execute}");

        var chatCommand = new StringBuilder();

        chatCommand.Append('/');
        chatCommand.Append(execute.Channel.ToChatCommand());

        if (execute.Channel == ChatMode.Linkshell || execute.Channel == ChatMode.CrossworldLinkshell)
            chatCommand.Append(execute.Extra);

        chatCommand.Append(' ');

        if (execute.Channel == ChatMode.Tell)
            chatCommand.Append(execute.Extra);

        var sanitizedMessage = SanitizeMessage(execute.Message);
        chatCommand.Append(sanitizedMessage);

        var completedChatCommand = chatCommand.ToString();

        if (completedChatCommand.Length > AetherRemoteConstants.SpeakCommandCharLimit)
        {
            logger.Warning($"Message too big! Message={completedChatCommand}");
            return;
        }
        
        var finalSanitizedString = sanitizer.Sanitize(completedChatCommand);

        try
        {
            chat.SendMessage(finalSanitizedString);
        }
        catch(Exception ex)
        {
            logger.Warning($"Something went wrong when sending a message: {ex.Message}");
        }
    }

    public void HandleEmoteCommand(EmoteCommandExecute execute)
    {
        logger.Info($"HandleEmoteCommand recieved: {execute}");

        var validEmote = emoteService.GetEmotes().Contains(execute.Emote);
        if (validEmote == false)
        {
            logger.Info($"Got invalid emote from server: [{execute.Emote}]");
        }

        var completedChatCommand = $"/{execute.Emote} mo";

        if (completedChatCommand.Length > AetherRemoteConstants.SpeakCommandCharLimit)
        {
            logger.Warning($"Emote too big! Emote={completedChatCommand}");
            return;
        }

        try
        {
            Task.Run(() => chat.SendMessage(completedChatCommand)).Wait();
        }
        catch (Exception ex)
        {
            logger.Warning($"Something went wrong trying to emote: {ex.Message}");
        }
    }

    public void HandleBecomeCommand(BecomeCommandExecute execute)
    {
        logger.Info($"HandleBecomeCommand recieved: {execute}");

        try
        {
            var player = clientState.LocalPlayer;
            if (player == null)
            {
                logger.Info("Could not handle become command because the local player was not loaded at the time the command was recieved");
                return;
            }

            glamourerAccessor.ApplyDesign(player.Name.ToString(), execute.GlamourerData, execute.GlamourerApplyType);
        }
        catch(Exception ex )
        {
            logger.Info($"Something went wrong trying to glamourer data: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes all non-number, non-letter, non-whitespace characters from a message
    /// </summary>
    /// <returns></returns>
    private static string SanitizeMessage(string message)
    {
        return new string(message.Where(c => char.IsLetterOrDigit(c) || char.IsDigit(c)).ToArray());
    }
}
