using AutomaticRoadblocks.Logging;
using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Animation
{
    public static class AnimationHelper
    {
        private static readonly ILogger Logger = IoC.Instance.GetInstance<ILogger>();

        /// <summary>
        /// Play the given animation through Rage, but manage it in the custom task executor for more options.
        /// </summary>
        /// <param name="ped">Set the ped to execute the animation on.</param>
        /// <param name="animationDictionary">Set the animation dictionary to load.</param>
        /// <param name="animationName">Set the animation name to play from the dictionary.</param>
        /// <param name="animationFlags">Set the animation flags to use on the animation playback.</param>
        /// <returns>Returns the animation task executor.</returns>
        public static AnimationTask PlayAnimation(Ped ped, AnimationDictionary animationDictionary, string animationName, AnimationFlags animationFlags)
        {
            Assert.NotNull(ped, "ped cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            return ped.Tasks.PlayAnimation(animationDictionary, animationName, 1.5f, animationFlags);
        }

        public static AnimationExecutor PlayAnimation(Entity entity, AnimationDictionary animationDictionary, string animationName,
            AnimationFlags animationFlags)
        {
            Assert.NotNull(entity, "entity cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            animationDictionary.Load();
            var success = NativeFunction.Natives.PLAY_ENTITY_ANIM<bool>(entity, animationName, animationDictionary.Name, 1000f,
                animationFlags.HasFlag(AnimationFlags.Loop),
                animationFlags.HasFlag(AnimationFlags.StayInEndFrame), false, 0.0f, 0U);
            
            if (success)
            {
                Logger.Trace($"Started playing animation {animationDictionary.Name}: {animationName}");
            }
            else
            {
                Logger.Warn($"Failed to start animation {animationDictionary.Name}: {animationName}");
            }

            return new AnimationExecutor(entity, animationDictionary, animationName);
        }

        public static bool IsAnimationPlaying(Entity entity, AnimationDictionary animationDictionary, string animationName)
        {
            Assert.NotNull(entity, "entity cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            return NativeFunction.Natives.IS_ENTITY_PLAYING_ANIM<bool>(entity, animationDictionary.Name, animationName, 3);
        }

        public static float GetAnimationCurrentTime(Entity entity, AnimationDictionary animationDictionary, string animationName)
        {
            Assert.NotNull(entity, "entity cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            return NativeFunction.Natives.GET_ENTITY_ANIM_CURRENT_TIME<float>(entity, animationDictionary.Name, animationName);
        }

        public static void StopAnimation(Entity entity, AnimationDictionary animationDictionary, string animationName)
        {
            Assert.NotNull(entity, "entity cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            // ENTITY::STOP_ENTITY_ANIM
            NativeFunction.CallByHash<int>(0x28004F88151E03E0, entity, animationDictionary.Name, animationName, 3);
        }

        public static void SetAnimationSpeed(Entity entity, AnimationDictionary animationDictionary, string animationName, float speed)
        {
            Assert.NotNull(entity, "entity cannot be null");
            Assert.HasText(animationDictionary, "animationDictionary cannot be empty");
            Assert.HasText(animationName, "animationName cannot be empty");
            // ENTITY::SET_ENTITY_ANIM_SPEED
            NativeFunction.CallByHash<uint>(0x28D1A16553C51776, entity, animationDictionary.Name, animationName, speed);
        }
    }
}