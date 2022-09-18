using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Models;
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
            return light.Model.StartsWith(WeaponAssetPrefix) ? FromWeapon(light) : FromModel(light);
        }

        public override string ToString()
        {
            return $"{nameof(Light)}: {Light}, {nameof(Name)}: {Name}, {nameof(ScriptName)}: {ScriptName}, {nameof(Width)}: {Width}, " +
                   $"{nameof(Spacing)}: {Spacing}, {nameof(IsNone)}: {IsNone}";
        }

        private static LightModel FromWeapon(Light light)
        {
            var model = new WeaponAsset(light.Model);

            // load the asset into memory
            model.LoadAndWait();

            return new LightModel
            {
                Light = light,
                LocalizationKey = new LocalizationKey(LocalizationKeyPrefix + ToCamelCase(light.ScriptName), light.Name),
                Model = null,
                WeaponAsset = model,
                Width = 0.05f
            };
        }

        private static LightModel FromModel(Light light)
        {
            var model = new Model(light.Model);

            // load the asset into memory
            model.LoadAndWait();

            return new LightModel
            {
                Light = light,
                LocalizationKey = new LocalizationKey(LocalizationKeyPrefix + ToCamelCase(light.ScriptName), light.Name),
                Model = model,
                WeaponAsset = null,
                Width = DimensionOf(model)
            };
        }
    }
}