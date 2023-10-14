using System;
using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Models;
using AutomaticRoadblocks.Utils;
using Rage;

namespace AutomaticRoadblocks.LightSources
{
    public class LightModel : AbstractModel
    {
        private const string WeaponAssetPrefix = "weapon";
        private const string LocalizationKeyPrefix = "LightType";

        public static readonly LightModel None = new()
        {
            Light = new Light("None", "none", null, 1.0, ELightSourceFlags.None),
            LocalizationKey = LocalizationKey.None,
            Model = null,
            WeaponAsset = null,
            Width = 1f
        };

        private LightModel()
        {
        }

        /// <summary>
        /// The light model data.
        /// </summary>
        public Light Light { get; private set; }

        /// <inheritdoc />
        public override string Name => Light.Name;

        /// <inheritdoc />
        public override string ScriptName => Light.ScriptName;

        /// <summary>
        /// The weapon asset which is used for this asset.
        /// </summary>
        public WeaponAsset? WeaponAsset { get; private set; }

        /// <summary>
        /// The actual width of the model.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// The spacing between the light model instances. 
        /// </summary>
        public float Spacing => (float)Light.Spacing;

        /// <summary>
        /// The relative rotation of the light model instance. 
        /// </summary>
        public float Rotation => (float)Light.Rotation;

        /// <inheritdoc />
        public override bool IsNone => Model == null && WeaponAsset == null;

        public static LightModel From(Light light)
        {
            Assert.NotNull(light, "light cannot be null");
            Logger.Trace($"Creating LightModel for {light}");
            var lightModel = new LightModel
            {
                Light = light,
                LocalizationKey = new LocalizationKey(LocalizationKeyPrefix + ToCamelCase(light.ScriptName), light.Name),
            };

            // load the asset into memory
            LoadAssetIntoMemory(lightModel);

            return lightModel;
        }

        public override string ToString()
        {
            return $"{nameof(Light)}: {Light}, {nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Width)}: {Width}, " +
                   $"{nameof(Spacing)}: {Spacing}, {nameof(IsNone)}: {IsNone}";
        }

        private static void LoadAssetIntoMemory(LightModel lightModel)
        {
            GameUtils.NewSafeFiber(() =>
            {
                Logger.Trace($"Creating barrier model {lightModel.Light.Model}");
                var asset = CreateAsset(lightModel);
                Logger.Trace($"Loading light asset for {lightModel}");
                var loadingStartedAt = DateTime.Now.Ticks;
                asset.LoadAndWait();
                var timeTaken = (DateTime.Now.Ticks - loadingStartedAt) / TimeSpan.TicksPerMillisecond;
                Logger.Debug($"Light asset {lightModel} has been loaded in {timeTaken}ms");

                if (IsWeaponModel(lightModel.Light))
                {
                    lightModel.WeaponAsset = (WeaponAsset) asset;
                    lightModel.Width = 0.05f;
                }
                else
                {
                    var model = (Model) asset;
                    lightModel.Model = model;
                    lightModel.Width = DimensionOf(model);
                }
                Logger.Trace($"Updated light model info for {lightModel}");
            }, "LightModel.LoadAssetIntoMemory");
        }

        private static IHashedAsset CreateAsset(LightModel lightModel)
        {
            var modelName = lightModel.Light.Model;
            return IsWeaponModel(lightModel.Light) ? new WeaponAsset(modelName) : new Model(modelName);
        }

        private static bool IsWeaponModel(Light light)
        {
            return light.Model.StartsWith(WeaponAssetPrefix);
        }
    }
}