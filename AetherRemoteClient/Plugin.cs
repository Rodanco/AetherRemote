using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.UI.Windows;
using Dalamud.Game.Addon.Events;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
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
    public static readonly bool DeveloperMode = false;

    public DalamudPluginInterface PluginInterface { get; init; }
    public ICommandManager CommandManager { get; init; }
    public IClientState ClientState { get; init; }
    public IChatGui ChatGui { get; init; }
    public IDataManager DataManager { get; init; }
    public ITargetManager TargetManager { get; init; }
    public Chat Chat { get; init; }

    public Configuration Configuration { get; init; }

    public SharedUserInterfaces SharedUserInterfaces { get; init; }

    public GlamourerAccessor GlamourerAccessor { get; init; }
    public IPluginLog Logger { get; init; }

    // Services
    public NetworkService NetworkService { get; init; }
    public FriendListService FriendListService { get; init; }
    public SaveService SaveService { get; init; }
    public SessionService SessionService { get; init; }
    public EmoteService EmoteService { get; init; }
    public ChatService ChatService { get; init; }

    public WindowSystem WindowSystem = new("AetherRemote");
    public ConfigWindow ConfigWindow { get; init; }
    public MainWindow MainWindow { get; init; }

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
        Logger = logger;
        ChatGui = chatGUI;
        ClientState = clientState;
        DataManager = dataManager;
        TargetManager = targetManager;
        CommandManager = commandManager;
        PluginInterface = pluginInterface;

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(pluginInterface);

        // Used in UI events
        SharedUserInterfaces = new SharedUserInterfaces(this);

        // Used to send messages to the server
        Chat = new XivCommonBase(pluginInterface).Functions.Chat;

        // https://github.com/KazWolfe/XIVDeck/compare/v0.2.12...v0.2.13

        // Accessors
        GlamourerAccessor = new GlamourerAccessor(this);

        // Services
        ChatService = new ChatService(this);
        EmoteService = new EmoteService(this);
        SaveService = new SaveService(this);
        NetworkService = new NetworkService(this);
        FriendListService = new FriendListService(this);
        SessionService = new SessionService(this);

        // Windows
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        MainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        NetworkService.Dispose();
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
        ChatService.Update();
        WindowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}
