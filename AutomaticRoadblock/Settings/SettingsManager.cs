using System;
using System.Windows.Forms;
using AutomaticRoadblocks.AbstractionLayer;
using Rage;

namespace AutomaticRoadblocks.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private const string File = @"./Plugins/LSPDFR/AutomaticRoadblocks.ini";

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
        public void Load()
        {
            _logger.Debug("Loading settings");
            if (System.IO.File.Exists(File))
            {
                _logger.Trace("Settings file " + File + " has been found");
            }
            else
            {
                _logger.Warn("Settings file not found, using defaults instead");
            }

            try
            {
                var settingsFile = new InitializationFile(File);

                ReadGeneralSettings(settingsFile);
                ReadAutomaticRoadblocksSettings(settingsFile);
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
                OpenMenuKey = ValueToKey(file.ReadString("General", "OpenMenuKey", "X")),
                OpenMenuModifierKey = ValueToKey(file.ReadString("General", "OpenMenuModifierKey", "ShiftKey"))
            };
        }

        private void ReadAutomaticRoadblocksSettings(InitializationFile file)
        {
            AutomaticRoadblocksSettings = new AutomaticRoadblocksSettings
            {
                EnableDuringPursuits = file.ReadBoolean("Automatic Roadblocks", "EnableDuringPursuits", true),
                EnableLights = file.ReadBoolean("Automatic Roadblocks", "EnableLights", true),
                DispatchAllowedAfter = file.ReadUInt32("Automatic Roadblocks", "DispatchAllowedAfter", 90),
                DispatchInterval = file.ReadUInt32("Automatic Roadblocks", "DispatchAllowedAfter", 45),
                TimeBetweenAutoLevelIncrements = file.ReadUInt32("Automatic Roadblocks", "TimeBetweenAutoLevelIncrements", 90),
                SlowTraffic = file.ReadBoolean("Automatic Roadblocks", "SlowTraffic", true)
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