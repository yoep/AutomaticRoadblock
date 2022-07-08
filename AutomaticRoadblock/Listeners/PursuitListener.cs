using AutomaticRoadblock.AbstractionLayer;
using LSPD_First_Response.Mod.API;

namespace AutomaticRoadblock.Listeners
{
    public class PursuitListener : IPursuitListener
    {
        public void StartListener()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            
            logger.Trace("Registering pursuit listener to LSPD_First_Response Api");
            Events.OnPursuitStarted += PursuitStarted;
        }

        public void StopListener()
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            
            logger.Trace("Removing pursuit listener from LSPD_First_Response Api");
            Events.OnPursuitStarted -= PursuitStarted;
        }

        private void PursuitStarted(LHandle pursuitHandle)
        {
            var logger = IoC.Instance.GetInstance<ILogger>();
            logger.Debug("Pursuit has been started");
        }
    }
}