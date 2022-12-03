using System;
using System.Windows.Forms;
using AutomaticRoadblocks.AbstractionLayer;
using Rage;

namespace AutomaticRoadblocks.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private const string File = @"./Plugins/LSPDFR/AutomaticRoadblocks.ini";
        private const string GeneralSection = "General";
        private const string AutomaticRoadblocksSection = "Automatic Roadblocks";
        private const string ManualPlacementSection = "Manual Placement";
        private const string RedirectTrafficSection = "Redirect Traffic";
        private const string CloseRoadSection = "Close Road";

        private readonly ILogger _logger;

        public SettingsManager(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public GeneralSettings GeneralSettings { get; private set; }

        /// <inheritdoc />
        public AutomaticRoadblocksSettings AutomaticRoadblocksSettings { get; private set; }

        /// <inheritdoc />
        public ManualPlacementSettings ManualPlacementSettings { get; private set; }

        /// <inheritdoc />
        public RedirectTrafficSettings RedirectTrafficSettings { get; private set; }

        /// <inheritdoc />
        public CloseRoadSettings CloseRoadSettings { get; private set; }

        /// <inheritdoc />
        public void Load()
        {
            _logger.Info("Loading plugin settings");
            if (System.IO.File.Exists(File))
            {
                _logger.Trace($"Settings file {File} has been found");
            }
            else
            {
                _logger.Warn($"Settings file {File} not found, using defaults instead");
            }

            try
            {
                var settingsFile = new InitializationFile(File);

                ReadGeneralSettings(settingsFile);
                ReadAutomaticRoadblocksSettings(settingsFile);
                ReadManualPlacementSettings(settingsFile);
                ReadRedirectTrafficSettings(settingsFile);
                ReadCloseRoadSettings(settingsFile);

                // update the log level of the plugin
                _logger.LogLevel = GeneralSettings.LogLevel;
                _logger.Info($"Plugin log level has been set to {_logger.LogLevel}");
                _logger.Info("Settings have been loaded with success");
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while loading the settings", ex);
            }
        }

        private void ReadGeneralSettings(InitializationFile file)
        {
            GeneralSettings = new GeneralSettings
            {
                OpenMenuKey = ValueToKey(file.ReadString(GeneralSection, "OpenMenuKey", "X")),
                OpenMenuModifierKey = ValueToKey(file.ReadString(GeneralSection, "OpenMenuModifierKey", "None")),
                CleanAllKey = ValueToKey(file.ReadString(GeneralSection, "CleanAllKey", "C")),
                CleanAllModifierKey = ValueToKey(file.ReadString(GeneralSection, "CleanAllModifierKey", "ControlKey")),
                LogLevel = file.ReadEnum(GeneralSection, "Logging", ELogLevel.Info)
            };
        }

        private void ReadAutomaticRoadblocksSettings(InitializationFile file)
        {
            AutomaticRoadblocksSettings = new AutomaticRoadblocksSettings
            {
                DispatchNowKey = ValueToKey(file.ReadString(AutomaticRoadblocksSection, "DispatchNowKey", "X")),
                DispatchNowModifierKey = ValueToKey(file.ReadString(AutomaticRoadblocksSection, "DispatchNowModifierKey", "ShiftKey")),
                EnableDuringPursuits = file.ReadBoolean(AutomaticRoadblocksSection, "EnableDuringPursuits", true),
                EnableAutoLevelIncrements = file.ReadBoolean(AutomaticRoadblocksSection, "EnableAutoLevelIncrements", true),
                EnableLights = file.ReadBoolean(AutomaticRoadblocksSection, "EnableLights", true),
                EnableSpikeStrips = file.ReadBoolean(AutomaticRoadblocksSection, "EnableSpikeStrips", true),
                EnableIntersectionRoadblocks = file.ReadBoolean(AutomaticRoadblocksSection, "EnableIntersectionRoadblocks", true),
                DispatchAllowedAfter = file.ReadUInt32(AutomaticRoadblocksSection, "DispatchAllowedAfter", 90),
                DispatchInterval = file.ReadUInt32(AutomaticRoadblocksSection, "DispatchAllowedAfter", 45),
                TimeBetweenAutoLevelIncrements = file.ReadUInt32(AutomaticRoadblocksSection, "TimeBetweenAutoLevelIncrements", 90),
                SlowTraffic = file.ReadBoolean(AutomaticRoadblocksSection, "SlowTraffic", true),
                SpikeStripChance = file.ReadDouble(AutomaticRoadblocksSection, "SpikeStripChance", 0.4),
            };
        }

        private void ReadManualPlacementSettings(InitializationFile file)
        {
            ManualPlacementSettings = new ManualPlacementSettings
            {
                EnablePreview = file.ReadBoolean(ManualPlacementSection, "EnablePreview", true),
                DistanceFromPlayer = (float)file.ReadDouble(ManualPlacementSection, "DistanceFromPlayer", 8.0),
                EnableCops = file.ReadBoolean(ManualPlacementSection, "EnableCops", true),
                DefaultMainBarrier = file.ReadString(ManualPlacementSection, "DefaultMainBarrier"),
                DefaultSecondaryBarrier = file.ReadString(ManualPlacementSection, "DefaultSecondaryBarrier"),
            };
        }

        private void ReadRedirectTrafficSettings(InitializationFile file)
        {
            RedirectTrafficSettings = new RedirectTrafficSettings
            {
                EnablePreview = file.ReadBoolean(RedirectTrafficSection, "EnablePreview", true),
                DistanceFromPlayer = (float)file.ReadDouble(RedirectTrafficSection, "DistanceFromPlayer", 10.0),
                EnableLights = file.ReadBoolean(RedirectTrafficSection, "EnableLights", true),
                DefaultCone = file.ReadString(RedirectTrafficSection, "DefaultCone", "big_cone_stripes"),
            };
        }

        private void ReadCloseRoadSettings(InitializationFile file)
        {
            CloseRoadSettings = new CloseRoadSettings
            {
                MaxDistanceFromPlayer = (float)file.ReadDouble(CloseRoadSection, "MaxDistanceFromPlayer", 75.0),
                Barrier = file.ReadString(CloseRoadSection, "Barrier", "police_do_not_cross"),
            };
        }

        private Keys ValueToKey(string value)
        {
            if (!Enum.TryParse(value, true, out Keys key))
                _logger.Warn("Failed to parse key in settings file with value: " + value);

            return key;
        }
    }
}