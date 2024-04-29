using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonFriendPermissions;
using System;

namespace AetherRemoteClient.Domain;

[Serializable]
public class Friend(string id, string? note = null, FriendPermissions? permissions = null)
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
    public FriendPermissions Permissions = permissions ?? new();

    /// <summary>
    /// Returns a friend's given note, or their id
    /// </summary>
    public string NoteOrFriendCode => Note ?? FriendCode;

    /// <summary>
    /// Is this friend currently connected to the server
    /// </summary>
    [NonSerialized]
    public bool Online = false;

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("Friend");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Note", Note);
        sb.AddVariable("Permissions", Permissions);
        sb.AddVariable("Online", Online);
        return sb.ToString();
    }
}
