using Dalamud.Plugin.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XivCommon.Functions;

namespace AetherRemoteClient.Services;

/// <summary>
/// Service responsible for processing chat commands on the main XIV thread. <br/>
/// Allows for enqueuing of messages to be executed in chat. Messages are executed in the order <br/>
/// and with a delay to prevent automation detection / spam.
/// </summary>
public class ChatService
{
    private const int MinProcessTimeInMillis = 1000;
    private const int MaxProcessTimeInMillis = 3000;

    private readonly Chat chat;
    private readonly IPluginLog logger;

    private readonly ConcurrentQueue<string> commands;
    private readonly Random random;

    private DateTime lastUpdate = DateTime.Now;
    private double timeUpdateNextProcess = 0;

    public ChatService(Plugin plugin)
    {
        chat = plugin.Chat;
        logger = plugin.Logger;

        commands = new();
        random = new();
    }

    /// <summary>
    /// Enqueues a command to be sent to chat. Only use this method if the command has already been sanitized.
    /// </summary>
    public void EnqueueCommand(string command)
    {
        commands.Enqueue(command);
    }

    /// <summary>
    /// Required to process commands, this must be called frequently to process commands
    /// </summary>
    public void Update()
    {
        if (commands.Count == 0) return;

        var now = DateTime.Now;
        var delta = (now - lastUpdate).TotalMilliseconds;
        lastUpdate = now;

        if (timeUpdateNextProcess <= 0)
        {
            ProcessNextCommand();
            timeUpdateNextProcess = random.Next(MinProcessTimeInMillis, MaxProcessTimeInMillis);
        }
        else
        {
            timeUpdateNextProcess -= delta;
        }
    }

    /// <summary>
    /// Processes the next command in the command queue, if one exists. This method should be called once per frame.
    /// </summary>
    private void ProcessNextCommand()
    {
        try
        {
            if(commands.TryDequeue(out var result))
            {
                if (result == null)
                {
                    logger.Info("Some error");
                }
                else
                {
                    chat.SendMessage(result);
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"Something went wrong sending a message to chat: {e}");
        }
    }
}
