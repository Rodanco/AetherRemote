using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace AetherRemoteClient.Domain;

public sealed class ThreadedFilter<T>
{
    private const int DelayStartFilter = 192;
    private readonly IList<T> originalList;
    private readonly List<T> filteredList;
    private string searchTerm = "";
    private Thread filterThread;

    private long restartDelay = 0;
    private bool forceStopFilter = false;
    private bool useFilterList = false;

    private readonly Func<T, string,bool> predicateEvent;

    /// <summary>
    /// Returns the filtered list, or the original list if no filtering is required
    /// </summary>
    public IList<T> List => useFilterList ? filteredList : originalList;

    /// <summary>
    /// Filters lists of generics using a deticated thread
    /// </summary>
    /// <param name="originalList">List to filter on.</param>
    /// <param name="predicate">The generic, the search term, and if the generic should be added to the list</param>
    public ThreadedFilter(IList<T> originalList, Func<T, string, bool> predicate)
    {
        this.originalList = originalList;
        filteredList = new List<T>();
        filterThread = new Thread(Filter);
        predicateEvent = predicate;
    }

    private void Filter()
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < DelayStartFilter)
        {
            if (Interlocked.Read(ref restartDelay) > 0L)
            {
                stopwatch.Restart();
                Interlocked.Decrement(ref restartDelay);
            }
            if (forceStopFilter)
            {
                stopwatch.Stop();
                filteredList.Clear();
                forceStopFilter = false;
                return;
            }
        }
        stopwatch.Stop();

        var copyFilter = searchTerm;
        filteredList.Clear();
        foreach(var item in originalList)
        {
            if(predicateEvent.Invoke(item, copyFilter))
                filteredList.Add(item);

            if(forceStopFilter)
            {
                filteredList.Clear();
                forceStopFilter = false;
                return;
            }
        }

        useFilterList = true;
    }

    public void Restart(string newTerm)
    {
        if (newTerm == searchTerm)
            return;

        searchTerm = newTerm;
        if (searchTerm.Length > 0)
        {
            Interlocked.Increment(ref restartDelay);
            if (filterThread.ThreadState != System.Threading.ThreadState.Unstarted)
                filterThread = new Thread(Filter);

            filterThread.Start();
        }
        else
        {
            if (filterThread.IsAlive)
                forceStopFilter = true;
            else
                filteredList.Clear();

            Interlocked.Exchange(ref restartDelay, 0L);
            useFilterList = false;
        }
    }

    public void ForceStop()
    {
        forceStopFilter = true;
        useFilterList = false;
        Interlocked.Exchange(ref restartDelay, 0L);
    }
}
