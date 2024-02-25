using Dalamud.Interface;
using System.Numerics;

namespace AetherRemoteClient.Domain;

public struct IconButtonArgs
{
    public FontAwesomeIcon Icon;
    public string? Id;
    public Vector2? Size;
    public Vector2? Offset;
    public Vector4? Color;

    public IconButtonArgs(FontAwesomeIcon icon, string? id = null, Vector2? size = null, Vector2? offset = null, Vector4? color = null)
    {
        Icon = icon;
        Id = id;
        Size = size;
        Offset = offset;
        Color = color;
    }

    public static readonly IconButtonArgs CopyClipboard = new()
    {
        Icon = FontAwesomeIcon.Copy,
        Id = "CopyClipboardButton",
        Size = new Vector2(35, 35)
    };

    public static readonly IconButtonArgs Settings = new()
    {
        Icon = FontAwesomeIcon.Wrench,
        Id = "SettingsButton",
        Size = new Vector2(35, 35)
    };

    public static readonly IconButtonArgs Back = new()
    {
        Icon = FontAwesomeIcon.ArrowLeft,
        Id = "CopyClipboardButton",
        Size = new Vector2(35, 35)
    };

    public static readonly IconButtonArgs AddFriend = new()
    {
        Icon = FontAwesomeIcon.Plus,
        Id = "AddFriendButton",
        Size = new Vector2(24, 24),
        Offset = new Vector2(3, 0)
    };
}

