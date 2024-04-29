using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Dashboard;

public class DashboardTab(Configuration configuration, FriendListProvider friendListProvider, NetworkProvider networkProvider, SecretProvider secretProvider) : ITab
{
    public readonly Configuration configuration = configuration;
    public readonly FriendListProvider friendListProvider = friendListProvider;
    public readonly NetworkProvider networkProvider = networkProvider;
    public readonly SecretProvider secretProvider = secretProvider;
    
    private static readonly int LoginElementsWidth = 200;
    private static readonly Vector2 Spacing = new(8, 8);

    private string secretInputText = secretProvider.Secret;

    public void Draw()
    {
        if (ImGui.BeginTabItem("Dashboard"))
        {
            if (ImGui.BeginChild("DashboardArea", Vector2.Zero, true))
            {
                SharedUserInterfaces.BigTextCentered("Aether Remote", ImGuiColors.ParsedOrange);
                SharedUserInterfaces.MediumTextCentered($"{Plugin.Stage} V{Plugin.Version}");

                // TODO: Modify this code (everything with state) to use the new hybrid connection state
                // which encapsulates both the SignalR connection and logging in.
                var state = networkProvider.ConnectionState;
                var color = state switch
                {
                    ServerConnectionState.Connected => ImGuiColors.ParsedGreen,
                    ServerConnectionState.Disconnected => ImGuiColors.DPSRed,
                    _ => ImGuiColors.DalamudYellow,
                };

                SharedUserInterfaces.TextCentered(state.ToString(), color);

                if (state == ServerConnectionState.Connected)
                {
                    SharedUserInterfaces.BigTextCentered("My Friend Code");
                    SharedUserInterfaces.BigTextCentered(networkProvider?.FriendCode ?? "");
                }
                else
                {
                    if (state == ServerConnectionState.Connecting || state == ServerConnectionState.Reconnecting)
                        ImGui.BeginDisabled();

                    var shouldLogin = false;

                    var w = ImGui.GetWindowWidth();
                    var h = ImGui.GetWindowHeight();
                    var x = (w / 2) - (LoginElementsWidth / 2);
                    var y = (h / 2) - (h * 0.15f);

                    ImGui.SetCursorPosY(y);
                    SharedUserInterfaces.MediumTextCentered("Login");

                    ImGui.SetCursorPosX(x);
                    ImGui.SetNextItemWidth(LoginElementsWidth);
                    if (ImGui.InputTextWithHint("##SecretInput", "Enter Secret", ref secretInputText, Constants.SecretCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        shouldLogin = true;
                    }

                    ImGui.SetCursorPosX(x);
                    if (ImGui.Checkbox("Auto connect", ref configuration.AutoConnect))
                    {
                        configuration.Save();
                    }

                    ImGui.Dummy(Spacing);

                    ImGui.SetCursorPosX(x);
                    if (ImGui.Button("Login", new Vector2(LoginElementsWidth, 0)))
                    {
                        shouldLogin = true;
                    }

                    if (state == ServerConnectionState.Connecting || state == ServerConnectionState.Reconnecting)
                        ImGui.EndDisabled();

                    if (shouldLogin)
                        Login();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    private async void Login()
    {
        secretProvider.Secret = secretInputText;
        secretProvider.Save();

        var connectResult = await networkProvider.Connect(secretProvider.Secret);
        if (connectResult.Success)
        {
            // TODO: Do something

            // Ideally at this point we can request a sync, and if the syncs are different
            // then we can make a popup asking the user to select if they want what is local
            // or what is stored on the server.
        }
    }
}
