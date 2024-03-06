using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteCommon.Domain;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Concurrent;
using System.Text;
using XivCommon.Functions;

namespace AetherRemoteClient.Providers;

/// <summary>
/// Queues actions on the main XIV thread.
/// </summary>
public class ActionQueueProvider
{
    // Data
    private readonly ChatActionQueue chatActionQueue;
    private readonly GlamourerActionQueue glamourerActionQueue;

    public ActionQueueProvider(IPluginLog logger, IClientState clientState, Chat chat, GlamourerAccessor glamourerAccessor)
    {
        chatActionQueue = new ChatActionQueue(logger, clientState, chat);
        glamourerActionQueue = new GlamourerActionQueue(logger, clientState, glamourerAccessor);
    }

    public void Update()
    {
        chatActionQueue.Update();
        glamourerActionQueue.Update();
    }

    public void EnqueueBecomeAction(string sender, string data, GlamourerApplyType applyType)
    {
        var action = new BecomeAction(sender, data, applyType);
        glamourerActionQueue.EnqueueAction(action);
    }

    public void EnqueueEmoteAction(string sender, string emote)
    {
        var action = new EmoteAction(sender, emote);
        chatActionQueue.EnqueueAction(action);
    }

    public void EnqueueSpeakAction(string sender, string message, ChatMode channel, string? extra)
    {
        var action = new SpeakAction(sender, message, channel, extra);
        chatActionQueue.EnqueueAction(action);
    }

    private class ChatActionQueue : ActionQueue<IChatAction>
    {
        private readonly IClientState clientState;
        private readonly Chat chat;

        public ChatActionQueue(IPluginLog logger, IClientState clientState, Chat chat) : base(logger, clientState)
        {
            this.clientState = clientState;
            this.chat = chat;
        }

        protected override void Process(IChatAction action)
        {
            if (clientState.LocalPlayer == null)
                return;

            chat.SendMessage(action.Build());

            action.Log();
        }
    }

    private class GlamourerActionQueue : ActionQueue<BecomeAction>
    {
        private readonly IClientState clientState;
        private readonly GlamourerAccessor glamourerAccessor;

        public GlamourerActionQueue(IPluginLog logger, IClientState clientState, GlamourerAccessor glamourerAccessor) : base(logger, clientState)
        {
            this.clientState = clientState;
            this.glamourerAccessor = glamourerAccessor;
        }

        protected override void Process(BecomeAction action)
        {
            if (clientState.LocalPlayer == null)
                return;

            var localPlayerName = clientState.LocalPlayer.Name.ToString();

            glamourerAccessor.ApplyDesign(localPlayerName, action.Data, action.ApplyType);

            action.Log();
        }
    }

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

        protected abstract void Process(T action);

        public ActionQueue(IPluginLog logger, IClientState clientState)
        {
            this.logger = logger;
            this.clientState = clientState;
        }

        public void EnqueueAction(T action)
        {
            queue.Enqueue(action);
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
                try
                {
                    if (queue.TryDequeue(out var value))
                    {
                        if (value == null)
                            return;

                        Process(value);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Something went wrong processing an action: {e}");
                }

                timeUntilNextProcess = random.Next(MinProcessTime, MaxProcessTime);
            }
            else
            {
                timeUntilNextProcess -= delta;
            }
        }
    }

    private class EmoteAction : IChatAction
    {
        public string Sender;
        public string Emote;

        public EmoteAction(string sender, string emote)
        {
            Sender = sender;
            Emote = emote;
        }

        public string Build()
        {
            return $"/{Emote} motion";
        }

        public void Log()
        {
            var sb = new StringBuilder();
            sb.Append(Sender);
            sb.Append(" made you do the ");
            sb.Append(Emote);
            sb.Append(" emote.");

            ActionHistory.Log(Sender, sb.ToString(), DateTime.Now);
        }
    }

    private class BecomeAction : IQueueAction
    {
        public string Sender;
        public string Data;
        public GlamourerApplyType ApplyType;

        public BecomeAction(string sender, string data, GlamourerApplyType applyType)
        {
            Sender = sender;
            Data = data;
            ApplyType = applyType;
        }

        public void Log()
        {
            var sb = new StringBuilder();
            sb.Append(Sender);
            switch (ApplyType)
            {
                case GlamourerApplyType.EquipmentOnly:
                    sb.Append(" made you wear this outfit: [");
                    break;

                case GlamourerApplyType.CustomizationOnly:
                    sb.Append(" transformed you into this person: [");
                    break;

                case GlamourerApplyType.CustomizationAndEquipment:
                    sb.Append(" transformed you into a perfect copy of this person: [");
                    break;
            }

            sb.Append(Data);
            sb.Append("].");

            ActionHistory.Log(Sender, sb.ToString(), DateTime.Now);
        }
    }

    private class SpeakAction : IChatAction
    {
        public string Sender;
        public string Message;
        public ChatMode Channel;
        public string? Extra;

        public SpeakAction(string sender, string message, ChatMode channel, string? extra)
        {
            Sender = sender;
            Message = message;
            Channel = channel;
            Extra = extra;
        }

        public string Build()
        {
            var chatCommand = new StringBuilder();

            chatCommand.Append('/');
            chatCommand.Append(Channel.ToChatCommand());

            if (Channel == ChatMode.Linkshell || Channel == ChatMode.CrossworldLinkshell)
                chatCommand.Append(Extra);

            chatCommand.Append(' ');

            if (Channel == ChatMode.Tell)
            {
                chatCommand.Append(Extra);
                chatCommand.Append(' ');
            }

            chatCommand.Append(Message);

            return chatCommand.ToString();
        }

        public void Log()
        {
            var sb = new StringBuilder();
            sb.Append(Sender);
            sb.Append(" made you ");
            if (Channel == ChatMode.Tell)
            {
                sb.Append("send a tell to ");
                sb.Append(Extra);
                sb.Append(" saying: \"");
                sb.Append(Message);
                sb.Append("\".");
            }
            else
            {
                sb.Append("say: \"");
                sb.Append(Message);
                sb.Append("\" in ");
                sb.Append(Channel.ToCondensedString());
                if (Channel == ChatMode.Linkshell || Channel == ChatMode.CrossworldLinkshell)
                {
                    sb.Append(Extra);
                }
                sb.Append('.');
            }
            
            ActionHistory.Log(Sender, sb.ToString(), DateTime.Now);
        }
    }

    private interface IChatAction : IQueueAction
    {
        public string Build();
    }

    private interface IQueueAction
    {
        public void Log();
    }
}
