using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.AbstractionLayer.Implementation;
using AutomaticRoadblocks.Debug.Menu;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.ManualPlacement.Menu;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.Pursuit.Menu;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.RedirectTraffic.Menu;
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
            InitializeSettings();
            InitializeDutyListener();
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
                .RegisterSingleton<IGame>(typeof(RageImpl))
                .RegisterSingleton<ILogger>(typeof(RageLogger))
                .RegisterSingleton<ILocalizer>(typeof(Localizer))
                .RegisterSingleton<ISettingsManager>(typeof(SettingsManager))
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IModelProvider>(typeof(LspdfrModelProvider))
                .RegisterSingleton<IPursuitManager>(typeof(PursuitManager))
                .RegisterSingleton<IRoadblockDispatcher>(typeof(RoadblockDispatcher))
                .RegisterSingleton<IManualPlacement>(typeof(ManualPlacement.ManualPlacement))
                .RegisterSingleton<IRedirectTrafficDispatcher>(typeof(RedirectTrafficDispatcher));
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
            IoC.Instance.GetInstance<ISettingsManager>().Load();
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
                .Register<IMenuSwitchItem>(typeof(RedirectTrafficMenuSwitchItem))
                // pursuit components
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitLevelComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DispatchNowComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitEnableDuringPursuitComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableAutomaticLevelIncreaseComponent))
                // manual placement components
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualRoadblockPlaceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PlacementTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableCopsComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableSpeedLimitComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementBarrierComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementLightTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementVehicleTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementOffsetComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementRemoveComponentItem))
                // redirect traffic components
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficPlaceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficLaneTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficConeDistanceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficOffsetComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficConeTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficVehicleTypeComponentType))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableRedirectArrowComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficRemoveComponentItem));
        }

        private static void OnDutyStateChanged(bool onDuty)
        {
            var ioC = IoC.Instance;
            var logger = ioC.GetInstance<ILogger>();
            var pursuitListener = ioC.GetInstance<IPursuitManager>();
            logger.Trace($"On duty state changed to {onDuty}");

            if (onDuty)
            {
                pursuitListener.StartListener();

                var game = ioC.GetInstance<IGame>();
                ioC.GetInstance<IMenu>().Activate();
                game.DisplayPluginNotification($"{Assembly.GetExecutingAssembly().GetName().Version}, by ~b~yoep~s~, has been loaded");
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
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitToggleComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitForceOnFootComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CalloutEndComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RoadInfoComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RoadPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ZoneInfoComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitDispatchSpawnComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DispatchPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CleanRoadblocksComponent));
        }
    }
}