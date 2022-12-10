using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.CloseRoad;
using AutomaticRoadblocks.CloseRoad.Menu;
using AutomaticRoadblocks.Debug.Menu;
using AutomaticRoadblocks.Integrations;
using AutomaticRoadblocks.Integrations.PoliceSmartRadio;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.ManualPlacement.Menu;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Menu.Switcher;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.Pursuit.Menu;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.RedirectTraffic.Menu;
using AutomaticRoadblocks.Roadblock.Data;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.Roadblock.Menu;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.ShortKeys;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Utils;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

// Source code: https://github.com/yoep/AutomaticRoadblock
namespace AutomaticRoadblocks
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Main : Plugin
    {
        private const int WaitTimeForIntegrationsInitialization = 3 * 1000;
        private const string PoliceSmartRadioPluginName = "PoliceSmartRadio";
        private const string PoliceSmartRadioPluginVersion = "1.2.0.0";

        public Main()
        {
            InitializeIoC();
        }

        #region Properties

        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        #endregion

        #region Plugin

        public override void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LSPDFRResolveEventHandler;
            InitializeSettings();
            InitializeDutyListener();
            InitializeMenuComponents();
            InitializeDebugComponents();
            InitializeMenu();
            InitializeIntegrations();

            AttachDebugger();
        }

        public override void Finally()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var disposables = IoC.Instance.GetInstances<IDisposable>();

            try
            {
                logger.Debug($"Starting disposal of {disposables.Count} instances");
                foreach (var instance in disposables)
                {
                    instance.Dispose();
                }

                GameUtils.DisplayPluginNotification("~g~has been unloaded");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while unloading the plugin", ex);
                GameUtils.DisplayPluginNotification("~r~failed to correctly unload the plugin, see logs for more info");
            }
        }

        #endregion

        private static void InitializeIoC()
        {
            IoC.Instance
                .RegisterSingleton<ILogger>(typeof(RageLogger))
                .RegisterSingleton<ILocalizer>(typeof(Localizer))
                .RegisterSingleton<ISettingsManager>(typeof(SettingsManager))
                .RegisterSingleton<IPluginIntegrationManager>(typeof(PluginIntegrationManager))
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IBarrierData>(typeof(BarrierDataFile))
                .RegisterSingleton<IRoadblockData>(typeof(RoadblockDataFile))
                .RegisterSingleton<ILightSourceData>(typeof(LightSourceDataFile))
                .RegisterSingleton<IModelProvider>(typeof(ModelProvider))
                .RegisterSingleton<IPursuitManager>(typeof(PursuitManager))
                .RegisterSingleton<IRoadblockDispatcher>(typeof(RoadblockDispatcher))
                .RegisterSingleton<IManualPlacement>(typeof(ManualPlacement.ManualPlacement))
                .RegisterSingleton<IRedirectTrafficDispatcher>(typeof(RedirectTrafficDispatcher))
                .RegisterSingleton<ISpikeStripDispatcher>(typeof(SpikeStripDispatcher))
                .RegisterSingleton<ICleanAll>(typeof(CleanAll))
                .RegisterSingleton<ICloseRoadDispatcher>(typeof(CloseRoadDispatcher));
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
                .Register<IMenuSwitchItem>(typeof(CloseRoadMenuSwitchItem))
                // pursuit components
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitLevelComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitDispatchNowComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitEnableDuringPursuitComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitEnableAutomaticLevelIncreaseComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitEnableSpikeStripComponentItem))
                // manual placement components
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualRoadblockPlaceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementRemoveComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PlacementTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableCopsComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementDirectionComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementMainBarrierComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementSecondaryBarrierComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementLightTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementVehicleTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(ManualPlacementOffsetComponentItem))
                // redirect traffic components
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficPlaceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficRemoveComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficLaneTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficConeDistanceComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficOffsetComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficConeTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(RedirectTrafficVehicleTypeComponentType))
                .Register<IMenuComponent<UIMenuItem>>(typeof(EnableRedirectArrowComponentItem))
                // close road components
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadNearbyComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadUnitTypeComponentItem))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadLightSourceComponentItem));
        }

        private static void InitializeIntegrations()
        {
            var ioc = IoC.Instance;
            var logger = ioc.GetInstance<ILogger>();

            GameUtils.NewSafeFiber(() =>
            {
                GameFiber.Sleep(WaitTimeForIntegrationsInitialization);
                logger.Trace("Verifying available plugin integrations");

                // register each integration in the IoC
                if (IsLSPDFRPluginRunning(PoliceSmartRadioPluginName, Version.Parse(PoliceSmartRadioPluginVersion)))
                {
                    ioc.RegisterSingleton<IPoliceSmartRadio>(typeof(PoliceSmartRadioIntegration));
                    logger.Info($"Integration for {PoliceSmartRadioPluginName} is enabled");
                }
                else
                {
                    logger.Info($"Integration for {PoliceSmartRadioPluginName} is disabled, plugin not detected");
                }

                // load all registered plugin integrations
                ioc.GetInstance<IPluginIntegrationManager>().LoadPlugins();
            }, "Main.Integrations");
        }

        private static void OnDutyStateChanged(bool onDuty)
        {
            var ioC = IoC.Instance;
            var logger = ioC.GetInstance<ILogger>();
            var listeners = ioC.GetInstances<IOnDutyListener>();
            logger.Trace($"On duty state changed to {onDuty}");

            if (onDuty)
            {
                listeners.ToList().ForEach(x => x.OnDutyStarted());
                ioC.GetInstance<IMenu>().Activate();

                GameUtils.NewSafeFiber(() =>
                {
                    logger.Info($"Loaded version {Version}");

                    GameFiber.Wait(2 * 1000);
                    GameUtils.DisplayPluginNotification($"{Version}, by ~b~yoep~s~, has been loaded");
                }, "Main.DisplayPluginNotification");
            }
            else
            {
                listeners.ToList().ForEach(x => x.OnDutyEnded());
            }
        }

        private static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args)
        {
            return Functions.GetAllUserPlugins()
                .FirstOrDefault(assembly => args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()));
        }

        private static bool IsLSPDFRPluginRunning(string plugin, Version minVersion = null)
        {
            return Functions
                .GetAllUserPlugins()
                .Select(assembly => assembly.GetName())
                .Where(assemblyName => string.Equals(assemblyName.Name, plugin, StringComparison.CurrentCultureIgnoreCase))
                .Select(assemblyName => assemblyName.Version)
                .Any(assemblyVersion => minVersion == null || assemblyVersion.CompareTo(minVersion) >= 0);
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
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugPursuitToggleComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugPursuitForceOnFootComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugRoadPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugDeploySpikeStripComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugPreviewSpikeStripComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugReloadSettingsComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugReloadDataFilesComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitDispatchSpawnComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(PursuitDispatchPreviewComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CleanRoadblocksComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(DebugCleanEntitiesComponent))
                .Register<IMenuComponent<UIMenuItem>>(typeof(CloseRoadNearbyPreviewComponentItem));
        }
    }
}