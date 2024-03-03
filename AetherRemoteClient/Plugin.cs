using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Components;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.UI.Windows;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XivCommon;
using XivCommon.Functions;

namespace AetherRemoteClient;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/remote";

    /// <summary>
    /// Disables interacting with the server in any way, and returns mocked successes and the line when
    /// the server is invoked.
    /// </summary>
    public static readonly bool DeveloperMode = true;
    
    // Injected
    public DalamudPluginInterface PluginInterface { get; init; }
    public ICommandManager CommandManager { get; init; }

    // Instantiated
    public Configuration Configuration { get; init; }
    public SharedUserInterfaces SharedUserInterfaces { get; init; }

    // Accessors
    public GlamourerAccessor GlamourerAccessor { get; init; }

    // Providers
    public ActionQueueProvider ActionQueueProvider { get; init; }

    // Windows
    public WindowSystem WindowSystem  { get; init; }
    public MainWindow MainWindow { get; init; }
    public ConfigWindow ConfigWindow { get; init; }

    public Plugin(
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IAddonLifecycle addonLifecycle,
        ITargetManager targetManager,
        IClientState clientState,
        IDataManager dataManager,
        IPluginLog logger,
        IChatGui chatGUI)
    {
        CommandManager = commandManager;
        PluginInterface = pluginInterface;

        WindowSystem = new WindowSystem("AetherRemote");

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(pluginInterface);

        // Used in UI events
        SharedUserInterfaces = new SharedUserInterfaces(logger, pluginInterface);

        // Used to send messages to the server
        var Chat = new XivCommonBase(pluginInterface).Functions.Chat;

        // Accessors
        GlamourerAccessor = new GlamourerAccessor(logger, pluginInterface);

        // Providers
        var EmoteProvider = new EmoteProvider(dataManager);
        var FriendListProvider = new FriendListProvider(pluginInterface);
        var SecretProvider = new SecretProvider(pluginInterface);
        var NetworkProvider = new NetworkProvider(logger);
        ActionQueueProvider = new ActionQueueProvider(logger, clientState, Chat, GlamourerAccessor);

        // Services
        var NetworkService = new NetworkService(logger, pluginInterface, NetworkProvider, ActionQueueProvider, EmoteProvider);
        var FriendListService = new FriendListService(logger, NetworkProvider, FriendListProvider, SecretProvider);
        var SessionManagerService = new SessionManagerService(logger, targetManager, WindowSystem, 
            GlamourerAccessor, NetworkProvider, EmoteProvider, SecretProvider);
        
        // Windows
        ConfigWindow = new ConfigWindow();
        MainWindow = new MainWindow(logger, pluginInterface, ConfigWindow, Configuration, NetworkProvider, SecretProvider, FriendListService, SessionManagerService);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Aether Remote login / dashboard"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        MainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        GlamourerAccessor.Dispose();

        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);

        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.IsOpen = true;
    }

    private void DrawUI()
    {
        // Convenient way to do this
        ActionQueueProvider.Update();

        WindowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}
