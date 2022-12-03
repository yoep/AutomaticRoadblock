using System;
using System.Collections.Generic;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.Data;
using AutomaticRoadblocks.LightSources;

namespace AutomaticRoadblocks.Models
{
    /// <summary>
    /// The model provider convert the <see cref="IDataFile"/> into an actual usable <see cref="Rage.Model"/>.
    /// </summary>
    public interface IModelProvider
    {
        /// <summary>
        /// Retrieve all models of the given type.
        /// </summary>
        /// <param name="type">The type to return the available models of.</param>
        IEnumerable<IModel> this[Type type] { get; }

        /// <summary>
        /// Retrieve the model for the given script name.
        /// If multiple models are found with the same name, the first matching model is returned.
        /// </summary>
        /// <param name="scriptName">Returns the model for the script, else null.</param>
        IModel this[string scriptName] { get; }

        /// <summary>
        /// Retrieve the model for the given script name.
        /// If multiple models are found with the same name, the first matching model is returned.
        /// </summary>
        /// <param name="scriptName">Returns the model for the script, else null.</param>
        /// <param name="type">The type of the model to search for.</param>
        IModel this[string scriptName, Type type] { get; }

        /// <summary>
        /// The available barrier models based on the data file.
        /// </summary>
        IEnumerable<BarrierModel> BarrierModels { get; }

        /// <summary>
        /// The available light models based on the data file.
        /// </summary>
        IEnumerable<LightModel> LightModels { get; }

        /// <summary>
        /// Invoked when the barrier models list has changed.
        /// </summary>
        event ModelProviderEvents.BarrierModelsChanged BarrierModelsChanged;

        /// <summary>
        /// Invoked when the light models list has changed.
        /// </summary>
        event ModelProviderEvents.LightModelsChanged LightModelsChanged;

        /// <summary>
        /// Retrieve the model by the matching script name.
        /// </summary>
        /// <param name="scriptName">The script name to match.</param>
        /// <typeparam name="T">The model data type.</typeparam>
        /// <returns>Returns the found model for the script name.</returns>
        /// <exception cref="ModelNotFoundException">Is thrown when the given script name doesn't exist.</exception>
        T FindModelByScriptName<T>(string scriptName) where T : IModel;

        /// <summary>
        /// Try to retrieve the model by matching the given script name.
        /// </summary>
        /// <param name="scriptName">The script name to match.</param>
        /// <typeparam name="T">The model data type.</typeparam>
        /// <returns>Returns the model if found, else the "None" version of the model.</returns>
        T TryFindModelByScriptName<T>(string scriptName) where T : IModel;

        /// <summary>
        /// Reload the models of the provider.
        /// </summary>
        void Reload();
    }
}