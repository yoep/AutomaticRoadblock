namespace AutomaticRoadblocks.Vehicles
{
    public class EVehicleWheel
    {
        public static readonly EVehicleWheel LeftFront = new(0, "wheel_lf");
        public static readonly EVehicleWheel RightFront = new(1, "wheel_rf");
        public static readonly EVehicleWheel LeftMiddle = new(2, "wheel_lm");
        public static readonly EVehicleWheel RightMiddle = new(3, "wheel_rm");
        public static readonly EVehicleWheel LeftRear = new(4, "wheel_lr");
        public static readonly EVehicleWheel RightRear = new(5, "wheel_rr");

        private EVehicleWheel(int index, string boneName)
        {
            Index = index;
            BoneName = boneName;
        }
        
        /// <summary>
        /// The vehicle wheel index.
        /// </summary>
        public int Index { get; }
        
        /// <summary>
        /// The vehicle wheel bone name.
        /// </summary>
        public string BoneName { get; }
    }
}