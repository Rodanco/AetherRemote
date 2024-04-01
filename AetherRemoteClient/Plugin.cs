using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Providers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.UI.Experimental;
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
    private DalamudPluginInterface pluginInterface { get; init; }
    private ICommandManager commandManager { get; init; }

    // Instantiated
    private Configuration configuration { get; init; }
    private SharedUserInterfaces sharedUserInterfaces { get; init; }
    private Chat chat { get; init; }

    // Accessors
    private GlamourerAccessor glamourerAccessor { get; init; }

    // Providers
    private ActionQueueProvider actionQueueProvider { get; init; }
    private EmoteProvider emoteProvider { get; init; }
    private FriendListProvider friendListProvider { get; init; }
    private NetworkProvider networkProvider { get; init; }
    private SecretProvider secretProvider { get; init; }

    // Services
    private FriendListService friendListService { get; init; }
    private NetworkService networkService { get; init; }
    private SessionManagerService sessionManagerService { get; init; }

    // Windows
    private WindowSystem windowSystem  { get; init; }
    private MainWindow mainWindow { get; init; }
    private LogWindow logWindow { get; init; }
    private ConfigWindow configWindow { get; init; }
    private MainWindowExperiment mainWindowExperiment { get; init; }

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
        this.commandManager = commandManager;
        this.pluginInterface = pluginInterface;

        windowSystem = new WindowSystem("AetherRemote");

        configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        configuration.Initialize(pluginInterface);

        // Used in UI events
        sharedUserInterfaces = new SharedUserInterfaces(logger, pluginInterface);

        // Used to send messages to the server
        chat = new XivCommonBase(pluginInterface).Functions.Chat;

        // Accessors
        glamourerAccessor = new GlamourerAccessor(logger, pluginInterface);

        // Providers
        actionQueueProvider = new ActionQueueProvider(logger, clientState, chat, glamourerAccessor);
        emoteProvider = new EmoteProvider(dataManager);
        friendListProvider = new FriendListProvider(pluginInterface);
        networkProvider = new NetworkProvider(logger);
        secretProvider = new SecretProvider(pluginInterface);

        // Services
        friendListService = new FriendListService(logger, networkProvider, friendListProvider, secretProvider);
        networkService = new NetworkService(logger, pluginInterface, networkProvider, actionQueueProvider, emoteProvider, friendListProvider);
        sessionManagerService = new SessionManagerService(logger, targetManager, windowSystem, 
            glamourerAccessor, networkProvider, emoteProvider, secretProvider);

        // Windows
        logWindow = new LogWindow();
        configWindow = new ConfigWindow(configuration, networkProvider);
        mainWindow = new MainWindow(logger, pluginInterface, configWindow, logWindow, configuration, networkProvider, secretProvider, friendListService, sessionManagerService);
        mainWindowExperiment = new MainWindowExperiment(
            networkProvider, 
            friendListProvider, 
            logger, 
            configuration, 
            secretProvider,
            emoteProvider,
            glamourerAccessor,
            targetManager
            );

        windowSystem.AddWindow(logWindow);
        windowSystem.AddWindow(configWindow);
        windowSystem.AddWindow(mainWindow);
        windowSystem.AddWindow(mainWindowExperiment);

        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Aether Remote dashboard"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        mainWindow.IsOpen = true;
        mainWindowExperiment.IsOpen = true;
    }

    public void Dispose()
    {
        glamourerAccessor.Dispose();


        windowSystem.RemoveAllWindows();
        commandManager.RemoveHandler(CommandName);

        pluginInterface.UiBuilder.Draw -= DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
    }

    private void OnCommand(string command, string args)
    {
        mainWindowExperiment.IsOpen = true;
        mainWindow.IsOpen = true;
    }

    private void DrawUI()
    {
        // Convenient way to do this
        actionQueueProvider.Update();

        windowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        configWindow.IsOpen = true;
    }
}
