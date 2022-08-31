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
                var localizationFileLocation = string.Format(LocaleFile, Lang);
                if (!File.Exists(localizationFileLocation))
                {
                    _logger.Warn($"Localization data not found for '{Lang}', using fallback '{DefaultLang}' language instead");
                    localizationFileLocation = string.Format(LocaleFile, DefaultLang);
                }
                
                _logger.Trace($"Trying to load localization file {localizationFileLocation}");
                var localeFile = new InitializationFile(localizationFileLocation);
                _logger.Info($"Loaded localization file {localizationFileLocation}");

                var missingDataKeys = 0;

                // try to find all keys in the file
                foreach (var key in LocalizationKey.Values)
                {
                    if (localeFile.DoesKeyExist(Section, key.Identifier))
                    {
                        _logger.Trace($"Loading localization key {key.Identifier} from ini");
                    }
                    else
                    {
                        missingDataKeys++;
                        _logger.Warn($"Missing localization key {key.Identifier} in ini");
                    }

                    _messages[key] = localeFile.ReadString(Section, key.Identifier, key.DefaultText);
                }
                
                _logger.Info($"Localization data has been loaded from {localizationFileLocation}, detected {missingDataKeys} missing data keys");
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
                _logger.Error($"Unable to load localized message for {key}, key not found in localization data");
                throw new LocalizationNotFound(key);
            }

            return string.Format(_messages[key], args);
        }
    }
}