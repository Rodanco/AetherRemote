using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Experimental;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network.Become;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XivCommon.Functions;

namespace AetherRemoteClient.Components;

public class ActionProvider2
{
    private readonly ChatActionQueue actionQueue;

    public event EventHandler<ChatActionProcessedArgs> OnChatActionProcessed;

    public ActionProvider2()
    {

    }

    public void Update()
    {
        actionQueue.Update();
    }

    private class ChatActionQueue : ActionQueue<IChatAction>
    {

    }
}
