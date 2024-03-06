using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Providers;
using AetherRemoteClient.Services;
using AetherRemoteCommon;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Numerics;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Windows.Views;

public class LoginView : IWindow
{
    // Injected
    private readonly IPluginLog logger;

    //
    private readonly Configuration configuration;
    private readonly MainWindow mainWindow;

    // Provider
    private readonly NetworkProvider networkProvider;
    private readonly SecretProvider secretProvider;

    // Service
    private readonly FriendListService friendListService;

    private string secretInputBoxValue;
    private bool shouldAutoLoginCheckboxValue;

    private readonly ImGuiInputTextFlags inputTextFlags =
        ImGuiInputTextFlags.AutoSelectAll |
        ImGuiInputTextFlags.EnterReturnsTrue |
        ImGuiInputTextFlags.Password;

    private bool pendingLogin = false;
    private bool attemptingLogin = false;

    public LoginView(IPluginLog logger, MainWindow mainWindow, Configuration configuration, NetworkProvider networkProvider,
        SecretProvider secretProvider, FriendListService friendListService)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.mainWindow = mainWindow;
        this.networkProvider = networkProvider;
        this.secretProvider = secretProvider;
        this.friendListService = friendListService;

        secretInputBoxValue = secretProvider.Secret;
        shouldAutoLoginCheckboxValue = configuration.AutoConnect;
    }

    public void Draw()
    {
        SharedUserInterfaces.BigTextCentered("Aether Remote", 0, SharedUserInterfaces.Gold);
        SharedUserInterfaces.MediumTextCentered("Version 1.0.0");

        ImGui.Spacing();

        // Read once at the beginning of each draw call to prevent async state changing
        var attemptingLoginThisFrame = attemptingLogin;
        if (attemptingLoginThisFrame) ImGui.BeginDisabled();

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
        if (secretLength <= 0)ImGui.BeginDisabled();
        if (ImGui.Button("Login", new Vector2(ImGui.GetWindowWidth() - 15, 40)))
        {
            pendingLogin = true;
        }
        if (secretLength <= 0) ImGui.EndDisabled();

        if (attemptingLoginThisFrame) ImGui.EndDisabled();

        if (pendingLogin)
        {
            Task.Run(Login);
            pendingLogin = false;
        }
    }

    public void QueueLogin()
    {
        pendingLogin = true;
    }

    private async void Login()
    {
        attemptingLogin = true;

        secretProvider.Secret = secretInputBoxValue;
        secretProvider.Save();

        var connectResult = await networkProvider.Connect();
        if (connectResult.Success)
        {
            var loginResult = await networkProvider.Login(secretProvider.Secret, friendListService.FriendList);
            if (loginResult.Success)
            {
                mainWindow.SetCurrentViewToDashboard();
            }
            else
            {
                _ = networkProvider.Disconnect();
            }
        }
        
        attemptingLogin = false;
    }
}
