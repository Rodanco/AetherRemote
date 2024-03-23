using AetherRemoteClient.Domain;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class FriendSettingsWindow : Window
{
    private readonly Friend friend;

    private const ImGuiWindowFlags FriendSettingsWindowFlags =
        ImGuiWindowFlags.None;

    public FriendSettingsWindow(string windowName, Friend friendToEdit) : base(windowName, FriendSettingsWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(0, 0),
            MaximumSize = new Vector2(2000, 2000)
        };

        friend = friendToEdit;
    }

    public override void Draw()
    {
        SharedUserInterfaces.MediumText($"Editting settings for {friend.NoteOrFriendCode}", ImGuiColors.ParsedOrange);
        if (friend.Note != null)
        {
            ImGui.SameLine();
            SharedUserInterfaces.MediumText($"({friend.NoteOrFriendCode})", ImGuiColors.DalamudGrey);
        }
    }
}
