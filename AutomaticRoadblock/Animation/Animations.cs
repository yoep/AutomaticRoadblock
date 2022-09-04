using Rage;

namespace AutomaticRoadblocks.Animation
{
    public static class Animations
    {
        public const string SpikeStripDeploy = "p_stinger_s_deploy";
        public const string SpikeStripIdleDeployed = "p_stinger_s_idle_deployed";
        public const string SpikeStripIdleUndeployed = "p_stinger_s_idle_undeployed";
        
        public static class Dictionaries
        {
            public static AnimationDictionary StingerDictionary;

            static Dictionaries()
            {
                StingerDictionary = new AnimationDictionary("p_ld_stinger_s");
            }
        }
    }
}