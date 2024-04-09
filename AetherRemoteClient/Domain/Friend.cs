using AetherRemoteCommon.Domain.CommonFriendPreferences;
using System;

namespace AetherRemoteClient.Domain;

[Serializable]
public class Friend(string id, string? note = null, FriendPreferences? preferences = null)
{
    /// <summary>
    /// Id of the friend (UserId)
    /// </summary>
    public string FriendCode = id;

    /// <summary>
    /// A name set by the client to identify a friend more easily
    /// </summary>
    public string? Note = note;

    /// <summary>
    /// Friend preferences
    /// </summary>
    public FriendPreferences Preferences = preferences ?? new();

    /// <summary>
    /// Returns a friend's given note, or their id
    /// </summary>
    public string NoteOrFriendCode => Note ?? FriendCode;

    /// <summary>
    /// Is this friend currently connected to the server
    /// </summary>
    [NonSerialized]
    public bool Online = false;

    /// <summary>
    /// Is this friend selected in the dashboard for controlling
    /// </summary>
    [NonSerialized]
    public bool Selected = false;

    public Friend Copy()
    {
        return new Friend(FriendCode, Note, Preferences);
    }

    public override string ToString()
    {
        return $"Friend[FriendCode={FriendCode}, Note={Note}, Preferences={Preferences}]";
    }
}
