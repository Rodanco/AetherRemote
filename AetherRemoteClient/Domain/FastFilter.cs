using System;
using System.Collections.Generic;
using System.Linq;

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
