using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Windows;

public class ConfigWindow : Window
{
    private const ImGuiWindowFlags ConfigWindowFlags = 
        ImGuiWindowFlags.NoResize |
        ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse;

    private readonly Configuration configuration;
    private readonly NetworkProvider networkProvider;
    private bool shouldAutoLoginCheckboxValue;

    public ConfigWindow(Configuration configuration, NetworkProvider networkProvider) : base("Aether Remote Config", ConfigWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(300, 300)
        };

        this.configuration = configuration;
        this.networkProvider = networkProvider;

        shouldAutoLoginCheckboxValue = configuration.AutoConnect;

        if (Plugin.DeveloperMode)
        {
            ActionHistory.Log("Joe", "Joe made you do the bees knees emote.", DateTime.Now, LogType.Inbound);
            ActionHistory.Log("Me", "You made Leroy Derp say \"Hello!!!\" in party chat.", DateTime.Now, LogType.Outbound);
            ActionHistory.Log("Me", "Blocked a command from WeirdGuy002.", DateTime.Now, LogType.Error);
        }
    }

    public override void Draw()
    {
        SharedUserInterfaces.MediumText("Settings", ImGuiColors.ParsedOrange);
        ImGui.Separator();

        if (ImGui.Checkbox("###ShouldAutoLoginCheckbox", ref shouldAutoLoginCheckboxValue))
        {
            configuration.AutoConnect = shouldAutoLoginCheckboxValue;
            configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Should the plugin automatically attempt to log you in?");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        ImGui.TextUnformatted("Auto Login");

        DrawDisconnectButton();
    }

    private void DrawDisconnectButton()
    {
        var connectionState = networkProvider.Connection.State;
        if (connectionState == HubConnectionState.Connected)
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudRed);
        else
            ImGui.BeginDisabled();

        if (ImGui.Button("Disconnect"))
        {
            _ = networkProvider.Disconnect();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Disconnects you from the server.");
            ImGui.EndTooltip();
        }

        if (connectionState == HubConnectionState.Connected)
            ImGui.PopStyleColor();
        else
            ImGui.EndDisabled();
    }
}
