using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using AutomaticRoadblocks.Logging;
using Rage;

namespace AutomaticRoadblocks.Localization
{
    public class Localizer : ILocalizer
    {
        private const string LocaleFile = @"./plugins/LSPDFR/Automatic Roadblocks/Localization.{0}.ini";
        private const string DefaultLang = "en";
        private const string Section = "Messages";
        private static readonly string Lang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName.ToLower();

        private readonly ILogger _logger;
        private readonly IDictionary<LocalizationKey, string> _messages = new Dictionary<LocalizationKey, string>();

        private InitializationFile _localizationFile;

        public Localizer(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public string this[LocalizationKey key] => FindLocalizedMessage(key, Array.Empty<object>());

        /// <inheritdoc />
        public string this[LocalizationKey key, params object[] args] => FindLocalizedMessage(key, args);

        /// <inheritdoc />
        public void Reload()
        {
            if (LoadLocalizationFile())
            {
                ParseLocalizationData();
            }
        }

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            Reload();
        }

        private bool LoadLocalizationFile()
        {
            var localizationFileLocation = string.Format(LocaleFile, Lang);

            try
            {
                if (!File.Exists(localizationFileLocation))
                {
                    _logger.Warn($"Localization data file not found for '{Lang}', using fallback '{DefaultLang}' language instead");
                    localizationFileLocation = string.Format(LocaleFile, DefaultLang);
                }

                _logger.Trace($"Trying to load localization file {localizationFileLocation}");
                _localizationFile = new InitializationFile(localizationFileLocation);
                _logger.Info($"Loaded localization file {localizationFileLocation}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load localization file {localizationFileLocation}, {ex.Message}", ex);
            }

            return false;
        }

        private void ParseLocalizationData()
        {
            var missingDataKeys = 0;

            // clear the current cache
            lock (_messages)
            {
                _messages.Clear();
            }

            try
            {
                // try to find all predefined keys in the file and load them into memory
                // also try to detect any potential missing keys
                // this will not preload dynamic keys from the bariers
                foreach (var key in LocalizationKey.Values)
                {
                    if (!AddLocalizationKeyToCacheIfFound(key))
                    {
                        missingDataKeys++;
                        _logger.Warn($"Missing localization key {key.Identifier} in Localization ini");
                    }
                }

                _logger.Info($"Localization data has been loaded from {_localizationFile.FileName}");
                if (missingDataKeys > 0)
                    _logger.Warn($"Detected {missingDataKeys} missing data keys in the {_localizationFile.FileName}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to parse localization data, {ex.Message}", ex);
            }
        }

        private string FindLocalizedMessage(LocalizationKey key, object[] args)
        {
            var textValue = "";

            lock (_messages)
            {
                // verify if the key exists in the cache
                // if not, try to search it in the localization file and add it
                if (!_messages.ContainsKey(key) && !AddLocalizationKeyToCacheIfFound(key))
                {
                    if (string.IsNullOrWhiteSpace(key.DefaultText))
                    {
                        _logger.Error($"Unable to load localized message for {key.Identifier}, key not found in localization data nor default text provided");
                        throw new LocalizationNotFound(key);
                    }

                    textValue = key.DefaultText;
                    _logger.Warn($"Missing localization message for {key.Identifier}, using default text ({textValue}) instead");
                }
                else
                {
                    textValue = _messages[key];
                }
            }

            try
            {
                return string.Format(textValue, args);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to format localized message of {key}, {ex.Message}", ex);
                return key.DefaultText;
            }
        }

        private bool AddLocalizationKeyToCacheIfFound(LocalizationKey key)
        {
            if (!IsKeyKnownInLocalizationFile(key))
                return false;

            lock (_messages)
            {
                _messages[key] = _localizationFile.ReadString(Section, key.Identifier, key.DefaultText);
                _logger.Trace($"Added localization key {key.Identifier} from {_localizationFile.FileName}");
            }

            return true;
        }

        private bool IsKeyKnownInLocalizationFile(LocalizationKey key)
        {
            return _localizationFile.DoesKeyExist(Section, key.Identifier);
        }
    }
}