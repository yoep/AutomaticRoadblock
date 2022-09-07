using Rage;
using Rage.Native;

namespace AutomaticRoadblocks.Sound
{
    public static class SoundHelper
    {
        public static void PlaySound(Entity entity, string name, string audioRef)
        {
            Assert.NotNull(entity, "entity cannot be null");
            Assert.HasText(name, "name cannot be empty");
            Assert.HasText(audioRef, "audioRef cannot be empty");
            NativeFunction.Natives.PLAY_SOUND_FROM_ENTITY<uint>(-1, name, entity, audioRef, false, 0);
        }
    }
}