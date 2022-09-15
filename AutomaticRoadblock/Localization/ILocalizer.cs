namespace AutomaticRoadblocks.Localization
{
    /// <summary>
    /// Provides localized strings when available.
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>
        /// Retrieve the localized string for the given key.
        /// </summary>
        /// <param name="key">The key to retrieve the message from.</param>
        string this[LocalizationKey key] { get; }

        /// <summary>
        /// Retrieve the formatted localized string for the given key.
        /// </summary>
        /// <param name="key">The key to retrieve the message from.</param>
        /// <param name="args">The formatting arguments for the message.</param>
        string this[LocalizationKey key, params object[] args] { get; }

        /// <summary>
        /// Reload the localization data.
        /// </summary>
        void Reload();
    }
}