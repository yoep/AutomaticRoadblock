using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Logging;

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
            logger.Debug("Initial data is being loaded for ModelProvider");
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
            _logger.Debug("Loading all available data models");
            BarrierModels = TrySafeModelLoading(nameof(BarrierModels), LoadBarrierModels, InvokeBarrierModelsChanged,
                new List<BarrierModel> { BarrierModel.None });
            LightModels = TrySafeModelLoading(nameof(LightModels), LoadLightModels, InvokeLightModelsChanged, new List<LightModel> { LightModel.None });
            _logger.Info($"A total of {BarrierModels.Count() + LightModels.Count()} models have been loaded into the model provider");
        }

        #endregion

        #region Functions

        private List<BarrierModel> LoadBarrierModels()
        {
            if (_barrierModelData == null)
            {
                _logger.Error("Unable to load barrier models, _barrierModelData is null");
                return new List<BarrierModel>();
            }

            if (_barrierModelData.Barriers == null)
            {
                _logger.Error("Unable to load barrier models, _barrierModelData.Barriers is null");
                return new List<BarrierModel>();
            }

            _logger.Debug($"Loading a total of {_barrierModelData.Barriers.Items.Count} barrier models for {_barrierModelData}");
            return _barrierModelData.Barriers.Items
                .Select(BarrierModel.From)
                .ToList();
        }

        private List<LightModel> LoadLightModels()
        {
            if (_lightSourceData == null)
            {
                _logger.Error("Unable to load light models, _lightSourceData is null");
                return new List<LightModel>();
            }

            if (_lightSourceData.Lights == null)
            {
                _logger.Error("Unable to load light models, _lightSourceData.Lights is null");
                return new List<LightModel>();
            }

            _logger.Debug($"Loading a total of {_lightSourceData.Lights.Items.Count} light models for {_lightSourceData}");
            return _lightSourceData.Lights.Items
                .Select(LightModel.From)
                .ToList();
        }

        private IEnumerable<T> TrySafeModelLoading<T>(string property, Func<List<T>> action, Action<IEnumerable<T>> invokeEvent, List<T> defaults = null)
        {
            // initialize the defaults list if none is given
            defaults ??= new List<T>();

            try
            {
                _logger.Trace($"Loading models for {property} on {Thread.CurrentThread.Name}");
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
            _logger.Trace($"Barrier models has changed, invoking listeners");
            BarrierModelsChanged?.Invoke(models);
        }

        private void InvokeLightModelsChanged(IEnumerable<LightModel> models)
        {
            _logger.Trace("Light models has changed, invoking listeners");
            LightModelsChanged?.Invoke(models);
        }

        private static bool IsModelScriptName(IModel model, string expectedScriptName)
        {
            return string.Equals(model.ScriptName, expectedScriptName, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}