using AutomaticRoadblocks.AbstractionLayer;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Utils
{
    public static class LspdfrUtils
    {
        /// <summary>
        /// Extension on the <see cref="Functions"/> function which prevents audio from overlapping.
        /// As an addition, it can wait for the audio to complete before continuing.
        /// </summary>
        /// <param name="sound">The sound string to play.</param>
        /// <param name="waitForAudioCompletion">The indication if the function must wait for the audio to complete (default = false).</param>
        public static void PlayScannerAudio(string sound, bool waitForAudioCompletion = false)
        {
            WaitForAudioCompletion();
            Functions.PlayScannerAudio(sound);
            
            if (waitForAudioCompletion)
                WaitForAudioCompletion();
        }

        /// <summary>
        /// Extension on the <see cref="Functions.PlayScannerAudioUsingPosition"/> function which prevents audio from overlapping.
        /// As an addition, it can wait for the audio to complete before continuing.
        /// </summary>
        /// <param name="sound">The sound string to play.</param>
        /// <param name="position">The position to play in the audio.</param>
        /// <param name="waitForAudioCompletion">The indication if the function must wait for the audio to complete (default = false).</param>
        public static void PlayScannerAudioUsingPosition(string sound, Vector3 position, bool waitForAudioCompletion = false)
        {
            WaitForAudioCompletion();
            Functions.PlayScannerAudioUsingPosition(sound, position);

            if (waitForAudioCompletion)
                WaitForAudioCompletion();
        }

        /// <summary>
        /// Play the given sound without blocking the current game thread.
        /// This plays <see cref="PlayScannerAudio"/> in a different thread.
        /// </summary>
        /// <param name="sound">The sound string to play.</param>
        public static void PlayScannerAudioNonBlocking(string sound)
        {
            IoC.Instance.GetInstance<IGame>().NewSafeFiber(() => PlayScannerAudio(sound), "LspdfrUtils.Audio");
        }

        /// <summary>
        /// Wait for LSPDFR to complete playing the current audio (if any is playing at the moment).
        /// This will pause the current game fiber.
        /// </summary>
        public static void WaitForAudioCompletion()
        {
            while (Functions.GetIsAudioEngineBusy())
            {
                GameFiber.Yield();
            }
        }
    }
}