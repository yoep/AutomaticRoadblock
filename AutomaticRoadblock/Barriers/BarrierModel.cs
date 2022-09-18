using AutomaticRoadblocks.Localization;
using AutomaticRoadblocks.Models;
using Rage;

namespace AutomaticRoadblocks.Barriers
{
    public class BarrierModel : AbstractModel
    {
        private const string LocalizationKeyPrefix = "BarrierType";

        public static readonly BarrierModel None = new()
        {
            Barrier = new Barrier("None", "none", null, 1.0, EBarrierFlags.All),
            LocalizationKey = LocalizationKey.None,
            Model = null,
            Width = 1f
        };

        private BarrierModel()
        {
        }

        /// <summary>
        /// The barrier model data info.
        /// </summary>
        public Barrier Barrier { get; private set; }

        /// <inheritdoc />
        public override string Name => Barrier.Name;

        /// <inheritdoc />
        public override string ScriptName => Barrier.ScriptName;

        /// <summary>
        /// The actual width of the model.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// The spacing between the barrier model instances. 
        /// </summary>
        public float Spacing => (float)Barrier.Spacing;

        /// <summary>
        /// The vertical offset position of the barrier instance when spawned.
        /// </summary>
        public float VerticalOffset => (float)Barrier.VerticalOffset;

        /// <summary>
        /// Create a model for the given barrier data. 
        /// </summary>
        /// <param name="barrier">The barrier data to create a model from.</param>
        /// <returns>Returns the model data.</returns>
        public static BarrierModel From(Barrier barrier)
        {
            Assert.NotNull(barrier, "barrier cannot be null");
            var model = new Model(barrier.Model);

            // load the asset into memory
            model.LoadAndWait();

            return new BarrierModel
            {
                Barrier = barrier,
                LocalizationKey = new LocalizationKey(LocalizationKeyPrefix + ToCamelCase(barrier.ScriptName), barrier.Name),
                Model = model,
                Width = DimensionOf(model)
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Barrier)}: {Barrier}, {nameof(LocalizationKey)}: {LocalizationKey}, {nameof(Width)}: {Width}";
        }

        protected bool Equals(BarrierModel other)
        {
            return Equals(Barrier, other.Barrier) && Equals(LocalizationKey, other.LocalizationKey) && Width.Equals(other.Width);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BarrierModel)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Barrier != null ? Barrier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LocalizationKey != null ? LocalizationKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                return hashCode;
            }
        }
    }
}