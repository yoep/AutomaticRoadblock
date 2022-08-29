using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using AutomaticRoadblocks.AbstractionLayer;
using Rage;

namespace AutomaticRoadblocks.Localization
{
    public class Localizer : ILocalizer
    {
        private const string LocaleFile = @"./Plugins/LSPDFR/Automatic Roadblocks/Localization.{0}.ini";
        private const string DefaultLang = "en";
        private const string Section = "Messages";
        private static readonly string Lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();

        private readonly ILogger _logger;
        private readonly IDictionary<LocalizationKey, string> _messages = new Dictionary<LocalizationKey, string>();

        public Localizer(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public string this[LocalizationKey key] => FindLocalizedMessage(key, Array.Empty<object>());

        /// <inheritdoc />
        public string this[LocalizationKey key, params object[] args] => FindLocalizedMessage(key, args);

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            try
            {
                InitializationFile localeFile;
                var expectedLocalizationFile = string.Format(LocaleFile, Lang);
                var fallbackFile = string.Format(LocaleFile, DefaultLang);

                if (File.Exists(expectedLocalizationFile))
                {
                    _logger.Trace($"Loading localization data for {Lang} from {expectedLocalizationFile}");
                    localeFile = new InitializationFile(expectedLocalizationFile);
                }
                else
                {
                    _logger.Warn($"Localization data not found for {Lang}, using fallback {fallbackFile} instead");
                    localeFile = new InitializationFile(fallbackFile);
                }

                // try to find all keys in the file
                foreach (var key in LocalizationKey.Values)
                {
                    _logger.Trace($"Loading localization key {key.Identifier} from ini");
                    _messages[key] = localeFile.ReadString(Section, key.Identifier, key.DefaultText);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load localization data, {ex.Message}", ex);
            }
        }

        private string FindLocalizedMessage(LocalizationKey key, object[] args)
        {
            if (!_messages.ContainsKey(key))
            {
                _logger.Error($"Localization key {key} has not been created in the localizer");
                throw new LocalizationNotFound(key);
            }

            return string.Format(_messages[key], args);
        }
    }
}