using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Windows.Views;
//
public class LoginView : IWindow
{
    private string secretInputBoxValue;
    private bool shouldAutoLoginCheckboxValue;

    private readonly ImGuiInputTextFlags inputTextFlags =
        ImGuiInputTextFlags.AutoSelectAll |
        ImGuiInputTextFlags.EnterReturnsTrue |
        ImGuiInputTextFlags.Password;

    private readonly Configuration configuration;
    private readonly NetworkService networkService;
    private readonly MainWindow mainWindow;
    private readonly IPluginLog logger;
    private readonly SaveService saveService;

    private bool attemptingLogin = false;

    public LoginView(Plugin plugin, MainWindow mainWindow)
    {
        configuration = plugin.Configuration;
        networkService = plugin.NetworkService;
        logger = plugin.Logger;
        saveService = plugin.SaveService;

        this.mainWindow = mainWindow;

        secretInputBoxValue = saveService.Secret;
        shouldAutoLoginCheckboxValue = configuration.AutoConnect;
    }

    public void Draw()
    {
        SharedUserInterfaces.BigTextCentered("Aether Remote", 0, SharedUserInterfaces.Gold);
        SharedUserInterfaces.MediumTextCentered("Version 1.0.0");

        ImGui.Spacing();

        // Read once at the beginning of each draw call to prevent async state changing
        var attemptingLoginThisFrame = attemptingLogin;
        if (attemptingLoginThisFrame)
            ImGui.BeginDisabled(true);

        var pendingLogin = false;
        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 15);
        if (ImGui.InputTextWithHint("###SecretInput", "Enter secret", ref secretInputBoxValue, 
            AetherRemoteConstants.SecretCharLimit, inputTextFlags))
        {
            pendingLogin = true;
        }

        ImGui.Spacing();

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

        ImGui.SetCursorPosY(200);

        var secretLength = secretInputBoxValue.Length;
        if (secretLength <= 0)
            ImGui.BeginDisabled();

        if (ImGui.Button("Login", new Vector2(ImGui.GetWindowWidth() - 15, 40)))
        {
            pendingLogin = true;
        }

        if (secretLength <= 0)
            ImGui.EndDisabled();

        if (attemptingLoginThisFrame)
            ImGui.EndDisabled();

        if (pendingLogin)
        {
            Login();
        }
    }

    private async void Login()
    {
        attemptingLogin = true;

        saveService.Secret = secretInputBoxValue;
        saveService.SaveAll();

        var connectedSuccessfully = await networkService.Connect();
        if (connectedSuccessfully)
        {
            var loggedInSuccessfully = await networkService.Commands.Login();
            if (loggedInSuccessfully)
            {
                mainWindow.SetCurrentViewToDashboard();
            }
            else
            {
                networkService.Disconnect();
            }
        }
        
        attemptingLogin = false;
    }
}
