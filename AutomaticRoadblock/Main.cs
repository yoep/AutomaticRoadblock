using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.CloseRoad;
using AutomaticRoadblocks.Integrations;
using AutomaticRoadblocks.Integrations.PoliceSmartRadio;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Logging;
using AutomaticRoadblocks.ManualPlacement;
using AutomaticRoadblocks.Menu;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Pursuit;
using AutomaticRoadblocks.RedirectTraffic;
using AutomaticRoadblocks.Roadblock.Data;
using AutomaticRoadblocks.Roadblock.Dispatcher;
using AutomaticRoadblocks.Settings;
using AutomaticRoadblocks.ShortKeys;
using AutomaticRoadblocks.SpikeStrip.Dispatcher;
using AutomaticRoadblocks.Utils;
using LSPD_First_Response.Mod.API;
using Rage;

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
        private const string RageNativeUiName = "RAGENativeUI";
        private const string RageNativeUiVersion = "1.9.0.0";

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
            var logger = IoC.Instance.GetInstance<ILogger>();
            logger.Trace($"Initializing {Version}");
            AppDomain.CurrentDomain.AssemblyResolve += LSPDFRResolveEventHandler;
            InitializeSettings();

            if (IsRequiredAssembliesPresent())
            {
                InitializeDutyListener();
                InitializeIntegrations();
                MenuInitializer.InitializeMenuComponents();
                MenuInitializer.InitializeDebugComponents();
                MenuInitializer.InitializeMenu();

                AttachDebugger();
            }
            else
            {
                logger.Error($"Failed to start plugin {Version}, required assemblies are missing");
                GameUtils.DisplayPluginNotification("~r~Unable to load, missing assemblies detected");
            }
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

        private static bool IsRequiredAssembliesPresent()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            
            if (!IsAssemblyPresent(RageNativeUiName, Version.Parse(RageNativeUiVersion)))
            {
                logger.Error($"Unable to load, missing assembly {RageNativeUiName} version {RageNativeUiVersion}+");
                return false;
            }

            return true;
        }

        private static bool IsAssemblyPresent(string assembly, Version minVersion)
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var currentDomain = AppDomain.CurrentDomain;

            logger.Trace($"Checking assembly {assembly} for minimal version {minVersion}");
            return currentDomain.GetAssemblies()
                .Select(x => x.GetName())
                .Where(x => string.Equals(x.Name, assembly, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => x.Version)
                .Any(x => x.CompareTo(minVersion) >= 0);
        }

        [Conditional("DEBUG")]
        private static void AttachDebugger()
        {
            Rage.Debug.AttachAndBreak();
        }
    }
}