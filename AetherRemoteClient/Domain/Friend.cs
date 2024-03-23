using AetherRemoteCommon.Domain;
using System;

namespace AetherRemoteClient.Domain;

[Serializable]
public class Friend
{
    /// <summary>
    /// Id of the friend (UserId)
    /// </summary>
    public string FriendCode = string.Empty;

    /// <summary>
    /// A name set by the client to identify a friend more easily
    /// </summary>
    public string? Note = null;

    /// <summary>
    /// Friend preferences
    /// </summary>
    public FriendPreferences Preferences = new();

    /// <summary>
    /// Returns a friend's given note, or their id
    /// </summary>
    public string NoteOrFriendCode => Note ?? FriendCode;

    /// <summary>
    /// Is this friend currently connected to the server
    /// </summary>
    [NonSerialized]
    public bool Online = true;

    /// <summary>
    /// Is this friend selected in the dashboard for controlling
    /// </summary>
    [NonSerialized]
    public bool Selected = false;

    public Friend(string id, string? note = null, FriendPreferences? preferences = null)
    { 
        FriendCode = id;
        Note = note;
        Preferences = preferences ?? new();
    }

    public Friend Copy()
    {
        return new Friend(FriendCode, Note, Preferences);
    }

    public override string ToString()
    {
        return $"Friend[FriendCode={FriendCode}, Note={Note}, Preferences={Preferences}]";
    }
}
