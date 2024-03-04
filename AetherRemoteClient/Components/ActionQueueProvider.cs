using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteCommon.Domain;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Concurrent;
using XivCommon.Functions;

namespace AetherRemoteClient.Components;

/// <summary>
/// Queues actions on the main XIV thread.
/// </summary>
public class ActionQueueProvider
{
    // Data
    private readonly GlamourerActionQueue glamourerActionQueue;
    private readonly ChatActionQueue chatActionQueue;

    public ActionQueueProvider(IPluginLog logger, IClientState clientState, Chat chat, GlamourerAccessor glamourerAccessor)
    {
        glamourerActionQueue = new GlamourerActionQueue(logger, clientState, glamourerAccessor);
        chatActionQueue = new ChatActionQueue(logger, clientState, chat);
    }

    /// <summary>
    /// Must be called frequently to process commands in queue.
    /// </summary>
    public void Update()
    {
        glamourerActionQueue.Update();
        chatActionQueue.Update();
    }

    public void EnqueueBecomeAction(string sender, string data, GlamourerApplyType applyType)
    {

    }

    public void EnqueueEmoteAction(string sender, string emote)
    {

    }

    public void EnqueueSpeakAction(string sender, string message, ChatMode channel, string? extra)
    {

    }

    private class GlamourerActionQueue : ActionQueue<GlamourerAction>
    {
        private readonly IPluginLog logger;
        private readonly IClientState clientState;
        private readonly GlamourerAccessor glamourerAccessor;

        public GlamourerActionQueue(IPluginLog logger, IClientState clientState, GlamourerAccessor glamourerAccessor) : base(logger, clientState)
        {
            this.logger = logger;
            this.clientState = clientState;
            this.glamourerAccessor = glamourerAccessor;
        }

        protected override void Process(GlamourerAction action)
        {
            // TODO: Send to log
            logger.Info($"Action: {action}");

            // TODO: Explore possibility of DLQ
            if (clientState.LocalPlayer == null)
                return;

            var localName = clientState.LocalPlayer.Name.ToString();
            glamourerAccessor.ApplyDesign(localName, action.Data, action.ApplyType);
        }
    }

    private class ChatActionQueue : ActionQueue<ChatAction>
    {
        private readonly IPluginLog logger;
        private readonly IClientState clientState;
        private readonly Chat chat;

        public ChatActionQueue(IPluginLog logger, IClientState clientState, Chat chat) : base(logger, clientState)
        {
            this.logger = logger;
            this.clientState = clientState;
            this.chat = chat;
        }

        protected override void Process(ChatAction action)
        {
            // TODO: Send to log
            logger.Info($"Action: {action}");

            // TODO: Explore possibility of DLQ
            if (clientState.LocalPlayer == null)
                return;

            chat.SendMessage(action.Command);
        }
    }

    private class ChatAction2Queue : ActionQueue<IChatAction>
    {
        private readonly IPluginLog logger;
        private readonly IClientState clientState;
        private readonly Chat chat;

        public ChatAction2Queue(IPluginLog logger, IClientState clientState, Chat chat) : base(logger, clientState)
        {
            this.logger = logger;
            this.clientState = clientState;
            this.chat = chat;
        }

        protected override void Process(IChatAction action)
        {
            // TODO: Explore possibility of DLQ
            if (clientState.LocalPlayer == null)
                return;

            var command = action.Build();

            chat.SendMessage(command);

            action.Log();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private abstract class ActionQueue<T>
    {
        protected virtual int MinProcessTime { get; set; } = 1000;
        protected virtual int MaxProcessTime { get; set; } = 3000;

        private readonly IPluginLog logger;
        private readonly IClientState clientState;

        private readonly ConcurrentQueue<T> queue = new();
        private readonly Random random = new();

        private DateTime timeLastUpdated = DateTime.Now;
        private double timeUntilNextProcess = 0;

        public ActionQueue(IPluginLog logger, IClientState clientState)
        {
            this.logger = logger;
            this.clientState = clientState;
        }

        public void Update()
        {
            if (queue.IsEmpty || clientState.LocalPlayer == null)
                return;

            var now = DateTime.Now;
            var delta = (now - timeLastUpdated).TotalMilliseconds;
            timeLastUpdated = now;

            if (timeUntilNextProcess <= 0)
            {
                TryProcess();
                timeUntilNextProcess = random.Next(MinProcessTime, MaxProcessTime);
            }
            else
            {
                timeUntilNextProcess -= delta;
            }
        }

        public void EnqueueAction(T action)
        {
            queue.Enqueue(action);
        }

        private void TryProcess()
        {
            try
            {
                if (queue.TryDequeue(out var value))
                {
                    if (value == null)
                    {
                        logger.Info($"Action was null for type {typeof(T)}!");
                    }
                    else
                    {
                        Process(value);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Something went wrong processing an action: {e}");
            }
        }

        protected abstract void Process(T action);
    }
}
