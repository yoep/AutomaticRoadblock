using Rage;

namespace AutomaticRoadblocks.Utils
{
    public static class AnimationUtils
    {
        /// <summary>
        /// Play the given animation through Rage, but manage it in the custom task executor for more options.
        /// </summary>
        /// <param name="ped">Set the ped to execute the animation on.</param>
        /// <param name="animationDictionary">Set the animation dictionary to load.</param>
        /// <param name="animationName">Set the animation name to play from the dictionary.</param>
        /// <param name="animationFlags">Set the animation flags to use on the animation playback.</param>
        /// <returns>Returns the animation task executor.</returns>
        public static AnimationTask PlayAnimation(Ped ped, string animationDictionary, string animationName, AnimationFlags animationFlags)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            var dictionary = new AnimationDictionary(animationDictionary);
            return ped.Tasks.PlayAnimation(dictionary, animationName, 1.5f, animationFlags);
        }
    }
}