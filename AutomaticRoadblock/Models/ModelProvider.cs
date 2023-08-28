using System;
using System.Collections.Generic;
using System.Linq;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Logging;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models
{
    public class ModelProvider : IModelProvider
    {
        private readonly ILogger _logger;
        private readonly IBarrierData _barrierModelData;
        private readonly ILightSourceData _lightSourceData;

        public ModelProvider(ILogger logger, IBarrierData barrierModelData, ILightSourceData lightSourceData)
        {
            _logger = logger;
            _barrierModelData = barrierModelData;
            _lightSourceData = lightSourceData;

            // do an initial load as some components might depend on them
            Reload();
        }

        #region Properties

        /// <inheritdoc />
        public IEnumerable<IModel> this[Type type] => Models.Where(x => x.GetType() == type).ToList();

        /// <inheritdoc />
        public IModel this[string scriptName] => Models.FirstOrDefault(x => IsModelScriptName(x, scriptName));

        /// <inheritdoc />
        public IModel this[string scriptName, Type type] => Models
            .Where(x => x.GetType() == type)
            .FirstOrDefault(x => IsModelScriptName(x, scriptName));

        /// <inheritdoc />
        public IEnumerable<BarrierModel> BarrierModels { get; private set; } = new List<BarrierModel> { BarrierModel.None };

        /// <inheritdoc />
        public IEnumerable<LightModel> LightModels { get; private set; } = new List<LightModel> { LightModel.None };

        /// <inheritdoc />
        public event ModelProviderEvents.BarrierModelsChanged BarrierModelsChanged;

        /// <inheritdoc />
        public event ModelProviderEvents.LightModelsChanged LightModelsChanged;

        /// <summary>
        /// The available models within this provider.
        /// </summary>
        private IEnumerable<IModel> Models => new List<IModel>()
            .Concat(BarrierModels)
            .Concat(LightModels)
            .ToList();

        #endregion

        #region Methods

        /// <inheritdoc />
        public T FindModelByScriptName<T>(string scriptName) where T : IModel
        {
            var model = this[scriptName, typeof(T)];
            if (model == null)
            {
                throw new ModelNotFoundException(scriptName);
            }

            return (T)model;
        }

        /// <inheritdoc />
        public T TryFindModelByScriptName<T>(string scriptName) where T : IModel
        {
            var model = this[scriptName, typeof(T)] ?? this[typeof(T)].First(x => x.IsNone);
            return (T)model;
        }

        /// <inheritdoc />
        public void Reload()
        {
            _logger.Trace("Loading available data models");
            BarrierModels = TrySafeModelLoading(nameof(BarrierModels), () => _barrierModelData.Barriers.Items
                .Select(BarrierModel.From)
                .ToList(), InvokeBarrierModelsChanged, new List<BarrierModel> { BarrierModel.None });
            LightModels = TrySafeModelLoading(nameof(LightModels), () => _lightSourceData.Lights.Items
                .Select(LightModel.From)
                .ToList(), InvokeLightModelsChanged, new List<LightModel> { LightModel.None });
            _logger.Info("Data models have been loaded into the model provider");
        }

        #endregion

        #region Functions

        [CanBeNull]
        private IEnumerable<T> TrySafeModelLoading<T>(string property, Func<List<T>> action, Action<IEnumerable<T>> invokeEvent, List<T> defaults = null)
        {
            // initialize the defaults list if none is given
            defaults ??= new List<T>();

            try
            {
                _logger.Trace($"Loading models for {property}");
                var loadedModels = action.Invoke();
                _logger.Debug($"Loaded a total of {loadedModels.Count} {property}");

                // add the loaded models to the defaults
                defaults.AddRange(loadedModels);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load models of {property}, {ex.Message}", ex);
            }

            // notify the listeners that the models have been changed
            invokeEvent.Invoke(defaults);
            return defaults;
        }

        private void InvokeBarrierModelsChanged(IEnumerable<BarrierModel> models)
        {
            BarrierModelsChanged?.Invoke(models);
        }

        private void InvokeLightModelsChanged(IEnumerable<LightModel> models)
        {
            LightModelsChanged?.Invoke(models);
        }

        private static bool IsModelScriptName(IModel model, string expectedScriptName)
        {
            return string.Equals(model.ScriptName, expectedScriptName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}