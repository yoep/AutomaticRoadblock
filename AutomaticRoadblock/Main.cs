using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutomaticRoadblock.AbstractionLayer;
using AutomaticRoadblock.AbstractionLayer.Implementation;
using AutomaticRoadblock.Listeners;
using AutomaticRoadblock.Menu;
using AutomaticRoadblock.Settings;
using LSPD_First_Response.Mod.API;

namespace AutomaticRoadblock
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
            InitializeMenu();

            AttachDebugger();
        }

        public override void Finally()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var notification = IoC.Instance.GetInstance<INotification>();
            var disposables = IoC.Instance.GetInstances<IDisposable>();

            try
            {
                logger.Debug($"Starting disposal of {disposables.Count} instances");
                foreach (var instance in disposables)
                {
                    instance.Dispose();
                }

                notification.DisplayPluginNotification("~g~has been unloaded");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while unloading the plugin", ex);
                notification.DisplayPluginNotification("~r~failed to correctly unload the plugin, see logs for more info");
            }
        }

        private static void InitializeIoC()
        {
            IoC.Instance
                .Register<INotification>(typeof(RageNotification))
                .Register<IGameFiber>(typeof(RageFiber))
                .Register<ILogger>(typeof(RageLogger))
                .RegisterSingleton<ISettingsManager>(typeof(SettingsManager))
                .RegisterSingleton<IMenu>(typeof(MenuImpl))
                .RegisterSingleton<IPursuitListener>(typeof(PursuitListener));
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
            var menuComponents = IoC.Instance.GetInstances<IMenuComponent>();
            
            logger.Trace("Initializing menu components");
            foreach (var menuComponent in menuComponents)
            {
                menu.RegisterComponent(menuComponent);
            }
            
            logger.Debug($"Initialized a total of {menu.TotalItems} menu component(s)");
        }

        private static void OnDutyStateChanged(bool onDuty)
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            var pursuitListener = IoC.Instance.GetInstance<IPursuitListener>();
            logger.Trace($"On duty state changed to {onDuty}");

            if (onDuty)
            {
                pursuitListener.StartListener();
                
                var notification = IoC.Instance.GetInstance<INotification>();
                notification.DisplayPluginNotification($"{Assembly.GetExecutingAssembly().GetName().Version}, developed by ~b~yoep~s~, has been loaded");
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
    }
}