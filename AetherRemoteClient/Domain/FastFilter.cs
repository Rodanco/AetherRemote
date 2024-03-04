using System;
using System.Collections.Generic;
using System.Linq;
using XivCommon.Functions.FriendList;

namespace AetherRemoteClient.Domain;

[Obsolete("This should be replaced by a simplier system")]
public class FastFilter<T>
{
    private static readonly Type TypeString = typeof(string);
    private static readonly Type TypeFriend = typeof(Friend);

    private readonly List<T> sourceList;
    private List<T> filteredList;

    private string lastFilteredPhase = string.Empty;

    public FastFilter(List<T> sourceList)
    {
        this.sourceList = sourceList;
        filteredList = new List<T>();
    }

    public List<T> Filter(string filterPhrase)
    {
        if (filterPhrase == string.Empty) return sourceList;
        if (filterPhrase == lastFilteredPhase) return filteredList;

        lastFilteredPhase = filterPhrase;

        var type = typeof(T);
        if (type == TypeString)
        {
            filteredList = sourceList.OfType<string>().Where(s => s.Contains(filterPhrase)).Cast<T>().ToList();
        }
        else if (type == TypeFriend)
        {
            filteredList = sourceList.OfType<Friend>().Where(s => s.NoteOrId.Contains(filterPhrase)).Cast<T>().ToList();
        }

        return filteredList;
    }
}

public sealed class CustomFilter<T>
{
    private const int DelayStartFilter = 500;
    private string searchTerm = "";
    private readonly IList<T> originalList;
    private readonly List<T> filteredList;
    private System.Threading.Thread filterThread;

    private long restartDelay = 0;
    private bool forceStopFilter = false, useFilterList = false;

    private Func<T, string,bool> predicateEvent;
    public IList<T> List => useFilterList ? filteredList : originalList;

    /// <summary>
    /// The FUNC is receving the item and the search term. You return a bool if I need to add on the filtered list or not
    /// </summary>
    /// <param name="originalList"></param>
    /// <param name="predicate">A function for the item and the search term. You return a bool if I need to add on the filtered list or not</param>
    public CustomFilter(IList<T> originalList, Func<T, string, bool> predicate)
    {
        this.originalList = originalList;
        filteredList = new List<T>();
        filterThread = new System.Threading.Thread(Filter);
        predicateEvent = predicate;
    }

    private void Filter()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < DelayStartFilter)
        {
            if (System.Threading.Interlocked.Read(ref restartDelay) > 0L)
            {
                stopwatch.Restart();
                System.Threading.Interlocked.Decrement(ref restartDelay);
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
        string copyFilter = searchTerm;
        useFilterList = true;
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
    }

    public void Restart(string newTerm)
    {
        if (newTerm == searchTerm)
            return;
        searchTerm = newTerm;
        if (searchTerm.Length > 0)
        {
            System.Threading.Interlocked.Increment(ref restartDelay);
            if (filterThread.ThreadState != System.Threading.ThreadState.Unstarted)
                filterThread = new System.Threading.Thread(Filter);
            filterThread.Start();
        }
        else
        {
            if (filterThread.IsAlive)
                forceStopFilter = true;
            else
                filteredList.Clear();
            System.Threading.Interlocked.Exchange(ref restartDelay, 0L);
            useFilterList = false;
        }
    }

    public void ForceStop()
    {
        forceStopFilter = true;
        useFilterList = false;
        restartDelay = 0;
        System.Threading.Interlocked.Exchange(ref restartDelay, 0L);
    }
}
