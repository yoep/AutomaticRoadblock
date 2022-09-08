namespace AutomaticRoadblocks.Roadblock.Dispatcher
{
    /// <summary>
    /// The options which should be applied when deploying a new roadblock.
    /// </summary>
    public class DispatchOptions
    {
        /// <summary>
        /// The indication if spike strips should be deployed along the roadblock.
        /// </summary>
        public bool EnableSpikeStrips { get; set; }

        /// <summary>
        /// The indication if this roadblock was requested by the user.
        /// This modifies the behavior of the dispatching with additional audio.
        /// </summary>
        public bool IsUserRequested { get; set; }
        
        /// <summary>
        /// The indication if the roadblock dispatching needs to be forced and all condition checks should be ignored.
        /// </summary>
        public bool Force { get; set; }
        
        /// <summary>
        /// The indication if the roadblock should be dispatched at the current location of the vehicle
        /// instead of calculating a position ahead of the vehicle.
        /// </summary>
        public bool AtCurrentLocation { get; set; }

        public override string ToString()
        {
            return $"{nameof(EnableSpikeStrips)}: {EnableSpikeStrips}, {nameof(IsUserRequested)}: {IsUserRequested}, {nameof(Force)}: {Force}, {nameof(AtCurrentLocation)}: {AtCurrentLocation}";
        }

        protected bool Equals(DispatchOptions other)
        {
            return EnableSpikeStrips == other.EnableSpikeStrips && IsUserRequested == other.IsUserRequested && Force == other.Force && AtCurrentLocation == other.AtCurrentLocation;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DispatchOptions)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EnableSpikeStrips.GetHashCode();
                hashCode = (hashCode * 397) ^ IsUserRequested.GetHashCode();
                hashCode = (hashCode * 397) ^ Force.GetHashCode();
                hashCode = (hashCode * 397) ^ AtCurrentLocation.GetHashCode();
                return hashCode;
            }
        }
    }
}