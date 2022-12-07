using System.Diagnostics.CodeAnalysis;
using Rage;

namespace AutomaticRoadblocks.Animation
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Animations
    {
        public const string SpikeStripDeploy = "p_stinger_s_deploy";
        public const string SpikeStripIdleDeployed = "p_stinger_s_idle_deployed";
        public const string SpikeStripIdleUndeployed = "p_stinger_s_idle_undeployed";
        public const string ObjectPlaceDown = "putdown_low";
        public const string ObjectPickup = "pickup_low";
        public const string ThrowShortLow = "low_l_throw_short";
        
        public static class Dictionaries
        {
            public static AnimationDictionary StingerDictionary;
            public static AnimationDictionary ObjectDictionary;
            public static AnimationDictionary GrenadeDictionary;
            public static AnimationDictionary CarParkDictionary;
            public static AnimationDictionary GuardDictionary;

            static Dictionaries()
            {
                StingerDictionary = new AnimationDictionary("p_ld_stinger_s");
                ObjectDictionary = new AnimationDictionary("pickup_object");
                GrenadeDictionary = new AnimationDictionary("cover@weapon@grenade");
                CarParkDictionary = new AnimationDictionary("amb@world_human_car_park_attendant@male@base");
                GuardDictionary = new AnimationDictionary("amb@world_human_guard_patrol@male@idle_a");
                
                StingerDictionary.LoadAndWait();
                ObjectDictionary.LoadAndWait();
                GrenadeDictionary.LoadAndWait();
                CarParkDictionary.LoadAndWait();
                GuardDictionary.LoadAndWait();
            }
        }
    }
}