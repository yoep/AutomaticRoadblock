using System;
using System.Windows.Forms;
using AutomaticRoadblock.AbstractionLayer;
using Rage;

namespace AutomaticRoadblock.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private const string File = @"./Plugins/LSPDFR/AutomaticRoadblock.ini";

        private readonly ILogger _logger;

        public SettingsManager(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public GeneralSettings GeneralSettings { get; private set; }

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
                OpenMenuModifierKey = ValueToKey(file.ReadString("General", "OpenMenuModifierKey", "Shift"))
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