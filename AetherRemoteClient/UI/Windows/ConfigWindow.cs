using AetherRemoteClient.Providers;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
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
        this.configuration = configuration;
        this.networkProvider = networkProvider;
        shouldAutoLoginCheckboxValue = configuration.AutoConnect;

        Size = new Vector2(300, 300);
        SizeCondition = ImGuiCond.Always;
    }

    public override void Draw()
    {
        SharedUserInterfaces.MediumText("Settings", SharedUserInterfaces.Gold);
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
            ImGui.PushStyleColor(ImGuiCol.Button, SharedUserInterfaces.Red);
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
