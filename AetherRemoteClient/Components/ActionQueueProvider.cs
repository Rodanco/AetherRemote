using AetherRemoteClient.Accessors.Glamourer;
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

    /// <summary>
    /// Enqueues a glamourer become command.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="data"></param>
    /// <param name="applyType"></param>
    public void EnqueueGlamourerAction(string sender, string data, GlamourerApplyType applyType)
    {
        var glamourerAction = new GlamourerAction(sender, data, applyType);
        glamourerActionQueue.EnqueueAction(glamourerAction);
    }

    /// <summary>
    /// Enqueues a chat speak or emote command.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="command"></param>
    public void EnqueueChatAction(string sender, string command)
    {
        var chatAction = new ChatAction(sender, command);
        chatActionQueue.EnqueueAction(chatAction);
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

    private class ChatAction : QueueAction
    {
        public string Command;

        public ChatAction(string sender, string command) : base(sender)
        {
            Command = command;
        }

        public override string ToString()
        {
            return $"ChatAction[Sender={Sender}, Command={Command}]";
        }
    }

    private class GlamourerAction : QueueAction
    {
        public string Data;
        public GlamourerApplyType ApplyType;

        public GlamourerAction(string sender, string data, GlamourerApplyType applyType) : base(sender)
        {
            Data = data;
            ApplyType = applyType;
        }

        public override string ToString()
        {
            return $"GlamourerAction[Sender={Sender}, Data={Data}, GlamourerApplyType={ApplyType}]";
        }
    }

    private class QueueAction
    {
        public string Sender;

        public QueueAction(string sender)
        {
            Sender = sender;
        }

        public override string ToString()
        {
            return $"QueueAction[Sender={Sender}]";
        }
    }
}
