using Dalamud.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Represents a session used in the Session Tab
/// </summary>
public class Session(string id, FontAwesomeIcon icon, Vector4 color, string? name = null)
{
    /// <summary>
    /// Session Id
    /// </summary>
    public string Id = id;

    /// <summary>
    /// Name of the session.
    /// </summary>
    public string Name = name ?? id;

    /// <summary>
    /// The icon for the session
    /// </summary>
    // TODO: Calculate IconPool before hand, and pass in the result to avoid constant Random instantiation
    public FontAwesomeIcon Icon = icon;

    /// <summary>
    /// The color of the icon
    /// </summary>         
    // TODO: Calculate ColorPool before hand, and pass in the result to avoid constant Random instantiation
    public Vector4 Color = color;

    /// <summary>
    /// List of friends locked into the session
    /// </summary>
    public List<Friend> TargetFriends = [];

    public string TargetFriendsAsList()
    {
        return string.Join(", ", TargetFriends.Select(friend => friend.NoteOrFriendCode));
    }
}
