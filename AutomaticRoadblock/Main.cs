using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.AbstractionLayer.Implementation;
using AutomaticRoadblocks.Debug.Menu;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.ManualPlacement.Menu;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.Pursuit.Menu;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.Roadblock.Menu;
using AutomaticRoadblocks.Settings;
using LSPD_First_Response.Mod.API;
using RAGENativeUI.Elements;

// Source code: https://github.com/yoep/AutomaticRoadblock
namespace AutomaticRoadblocks
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Main : Plugin
    {
        public Main()
        {
            InitializeIoC();
        }

        public override void Initialize()
        {
            InitializeDutyListener();
            InitializeSettings();
            InitializeMenuComponents();
            InitializeDebugComponents();
            InitializeMenu();

            AttachDebugger();
        }

        public override void Finally()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var game = IoC.Instance.GetInstance<IGame>();
            var disposables = IoC.Instance.GetInstances<IDisposable>();

            try
            {
                logger.Debug($"Starting disposal of {disposables.Count} instances");
                foreach (var instance in disposables)
                {
                    instance.Dispose();
                }

                game.DisplayPluginNotification("~g~has been unloaded");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while unloading the plugin", ex);
                game.DisplayPluginNotification("~r~failed to correctly unload the plugin, see logs for more info");
            }
        }

        private static void InitializeIoC()
        {
            IoC.Instance
                .Register<IGame>(typeof(AbstractionLayer.Implementation.Rage))
                .Register<ILogger>(typeof(RageLogger))
                .RegisterSingleton<ISettingsManager>(typeof(SettingsManager))
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IPursuitManager>(typeof(PursuitManager))
                .RegisterSingleton<IRoadblockDispatcher>(typeof(RoadblockDispatcher))
                .RegisterSingleton<IManualPlacement>(typeof(ManualPlacement.ManualPlacement));
        }

        private static void InitializeDutyListener()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();

            logger.Trace("Loading plugin");
            Functions.OnOnDutyStateChanged += OnDutyStateChanged;
            logger.Debug("Registered OnDuty event listener");
        }

        private static void InitializeSettings()
        {
            var settingsManager = IoC.Instance.GetInstance<ISettingsManager>();

            settingsManager.Load();
        }

        private static void InitializeMenu()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var menu = IoC.Instance.GetInstance<IMenu>();
            var menuComponents = IoC.Instance.GetInstances<IMenuComponent<UIMenuItem>>();

            logger.Trace("Initializing menu components");
            foreach (var menuComponent in menuComponents)
            {
                menu.RegisterComponent(menuComponent);
            }

            logger.Debug($"Initialized a total of {menu.TotalItems} menu component(s)");
        }

        private static void InitializeMenuComponents()
        {
            IoC.Instance
                // menu switchers
                .Register<IMenuSwitchItem>(typeof(RoadblockMenuSwitchItem))
                .Register<IMenuSwitchItem>(typeof(ManualPlacementMenuSwitchItem))
                // pursuit components
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableDuringPursuitComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableAutomaticLevelIncreaseComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitLevelComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DispatchNowComponent))
                // manual placement components
                .Register<IMenuComponent<UIMenuItem>>(typeof(PlaceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PlacementTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(BarrierComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(LightComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(VehicleComponentItem));
        }

        private static void OnDutyStateChanged(bool onDuty)
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var pursuitListener = IoC.Instance.GetInstance<IPursuitManager>();
            logger.Trace($"On duty state changed to {onDuty}");

            if (onDuty)
            {
                pursuitListener.StartListener();

                var game = IoC.Instance.GetInstance<IGame>();
                game.DisplayPluginNotification($"{Assembly.GetExecutingAssembly().GetName().Version}, developed by ~b~yoep~s~, has been loaded");
            }
            else
            {
                pursuitListener.StopListener();
            }
        }

        [Conditional("DEBUG")]
        private static void AttachDebugger()
        {
            Rage.Debug.AttachAndBreak();
        }

        [Conditional("DEBUG")]
        private static void InitializeDebugComponents()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();

            logger.Debug("Registering debug menu components");
            IoC.Instance
                .Register<IMenuSwitchItem>(typeof(DebugMenuSwitchItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(StartPursuitComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EndCalloutComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RoadInfoComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RoadPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ZoneInfoComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DispatchSpawnComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DispatchPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CleanRoadblocksComponent));
        }
    }
}