using AutomaticRoadblocks.Localization;

namespace AutomaticRoadblocks.Models
{
    /// <summary>
    /// The generic data model information for all configurable data models.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// The (display) name of the model.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The script name of the model used in references.
        /// </summary>
        string ScriptName { get; }

        /// <summary>
        /// The localization key to use for this barrier.
        /// </summary>
        LocalizationKey LocalizationKey { get; }
    }
}