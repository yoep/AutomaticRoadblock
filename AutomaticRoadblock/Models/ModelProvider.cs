using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutomaticRoadblocks.AbstractionLayer;
using AutomaticRoadblocks.Barriers;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Models
{
    public class ModelProvider : IModelProvider
    {
        private readonly IGame _game;
        private readonly ILogger _logger;
        private readonly IBarrierData _barrierModelData;

        public ModelProvider(IGame game, ILogger logger, IBarrierData barrierModelData)
        {
            _game = game;
            _logger = logger;
            _barrierModelData = barrierModelData;
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
        public IEnumerable<BarrierModel> BarrierModels { get; private set; }

        /// <inheritdoc />
        public event ModelProviderEvents.BarrierModelsChanged BarrierModelsChanged;

        /// <summary>
        /// The available models within this provider.
        /// </summary>
        private List<IModel> Models => new(BarrierModels);

        #endregion

        #region Methods

        /// <inheritdoc />
        public T RetrieveModelByScriptName<T>(string scriptName) where T : IModel
        {
            var model = this[scriptName, typeof(T)];
            if (model == null)
            {
                throw new ModelNotFoundException(scriptName);
            }

            return (T)model;
        }

        /// <inheritdoc />
        public void Reload()
        {
            _logger.Trace("Loading available data models");
            BarrierModels = TrySafeModelLoading(nameof(BarrierModels), () => _barrierModelData.Barriers.Items
                .Select(BarrierModel.From)
                .ToList(), InvokeBarrierModelsChanged, new List<BarrierModel> { BarrierModel.None });
            _logger.Info("Data models have been loaded into the model provider");
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            _game.NewSafeFiber(Reload, "ModelProvider.Load");
        }

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

        private static bool IsModelScriptName(IModel model, string expectedScriptName)
        {
            return model.ScriptName.Equals(expectedScriptName, StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion
    }
}