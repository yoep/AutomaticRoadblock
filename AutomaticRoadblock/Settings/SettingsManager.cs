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
                OpenMenuModifierKey = ValueToKey(file.ReadString(GeneralSection, "OpenMenuModifierKey", "ShiftKey")),
                LogLevel = file.ReadEnum(GeneralSection, "Logging", LogLevel.Info)
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
                DispatchAllowedAfter = file.ReadUInt32(AutomaticRoadblocksSection, "DispatchAllowedAfter", 90),
                DispatchInterval = file.ReadUInt32(AutomaticRoadblocksSection, "DispatchAllowedAfter", 45),
                TimeBetweenAutoLevelIncrements = file.ReadUInt32(AutomaticRoadblocksSection, "TimeBetweenAutoLevelIncrements", 90),
                SlowTraffic = file.ReadBoolean(AutomaticRoadblocksSection, "SlowTraffic", true)
            };
        }

        private void ReadManualPlacementSettings(InitializationFile file)
        {
            ManualPlacementSettings = new ManualPlacementSettings
            {
                EnablePreview = file.ReadBoolean(ManualPlacementSection, "EnablePreview"),
                DistanceFromPlayer = (float)file.ReadDouble(ManualPlacementSection, "DistanceFromPlayer", 8.0)
            };
        }

        private void ReadRedirectTrafficSettings(InitializationFile file)
        {
            RedirectTrafficSettings = new RedirectTrafficSettings
            {
                EnablePreview = file.ReadBoolean(RedirectTrafficSection, "EnablePreview"),
                DistanceFromPlayer = (float)file.ReadDouble(RedirectTrafficSection
                    , "DistanceFromPlayer", 10.0)
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