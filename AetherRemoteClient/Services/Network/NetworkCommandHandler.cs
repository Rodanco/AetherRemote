using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
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
            (string senderFriendCode, string channel, ChatMode message, string? extra) => { HandleSpeakCommand(senderFriendCode, channel, message, extra); });

        connection.On(AetherRemoteConstants.ApiEmote,
            (string senderFriendCode, string emote) => { HandleEmoteCommand(senderFriendCode, emote); });

        connection.On(AetherRemoteConstants.ApiBecome,
            (string senderFriendCode, string glamourerData, GlamourerApplyType glamourerApplyType) => { HandleBecomeCommand(senderFriendCode, glamourerData, glamourerApplyType); });
    }

    public void HandleSpeakCommand(string senderFriendCode, string message, ChatMode chatMode, string? extra)
    {
        logger.Info($"HandleSpeakCommand recieved: Sender: {senderFriendCode}, Message: {message}, Channel: {chatMode}, Extra: {extra}");

        var chatCommand = new StringBuilder();

        chatCommand.Append('/');
        chatCommand.Append(chatMode.ToChatCommand());

        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
            chatCommand.Append(extra);

        chatCommand.Append(' ');

        if (chatMode == ChatMode.Tell)
            chatCommand.Append(extra);

        var sanitizedMessage = SanitizeMessage(message);
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

    public void HandleEmoteCommand(string senderFriendCode, string emote)
    {
        logger.Info($"HandleEmoteCommand recieved: Sender: {senderFriendCode}, Emote: {emote}");

        var validEmote = emoteService.GetEmotes().Contains(emote);
        if (validEmote == false)
        {
            logger.Info($"Got invalid emote from server: [{emote}]");
        }

        var completedChatCommand = $"/{emote} mo";

        if (completedChatCommand.Length > AetherRemoteConstants.SpeakCommandCharLimit)
        {
            logger.Warning($"Emote too big! Emote={completedChatCommand}");
            return;
        }

        try
        {
            logger.Info($"Final command to be sent: {completedChatCommand}");
            Task.Run(() => chat.SendMessage(completedChatCommand)).Wait();
        }
        catch (Exception ex)
        {
            logger.Warning($"Something went wrong trying to emote: {ex.Message}");
        }
    }

    public void HandleBecomeCommand(string senderFriendCode, string glamourerData, GlamourerApplyType glamourerApplyType)
    {
        try
        {
            var player = clientState.LocalPlayer;
            if (player == null)
            {
                logger.Info("Could not handle become command because the local player was not loaded at the time the command was recieved");
                return;
            }

            glamourerAccessor.ApplyDesign(player.Name.ToString(), glamourerData, glamourerApplyType);
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
