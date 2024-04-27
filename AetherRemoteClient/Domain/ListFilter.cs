using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.Domain;

public class ListFilter<T> : IDisposable
{
    private const int DelayStartFilter = 256;
    
    private readonly Timer timer;
    private readonly List<T> source;
    private readonly Func<T, string, bool> filterPredicate;
    
    private List<T> filteredList;
    private string searchTerm = string.Empty;

    public List<T> List
    {
        get
        {
            if (searchTerm == string.Empty)
                return source;

            return filteredList;
        }
    }

    public ListFilter(List<T> source, Func<T, string, bool> filterPredicate)
    {
        timer = new Timer(DelayStartFilter);
        timer.Elapsed += async (sender, e) => await Task.Run(FilterList);

        this.source = source;
        filteredList = source;
        
        this.filterPredicate = filterPredicate;
    }

    private void FilterList()
    {
        var filteredList = new List<T>();
        foreach (var item in source)
        {
            if (filterPredicate.Invoke(item, searchTerm))
                filteredList.Add(item);
        }
        this.filteredList = filteredList;
    }

    public void UpdateSearchTerm(string newSearchTerm)
    {
        if (searchTerm == newSearchTerm)
            return;

        searchTerm = newSearchTerm;
        timer.Stop();
        timer.Start();
    }

    public void Dispose()
    {
        timer.Dispose();
        GC.SuppressFinalize(this);
    }
}
