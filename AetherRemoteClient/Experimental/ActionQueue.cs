using Dalamud.Plugin.Services;
using System;
using System.Collections.Concurrent;

namespace AetherRemoteClient.Experimental;

public abstract class ActionQueue<T>
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
