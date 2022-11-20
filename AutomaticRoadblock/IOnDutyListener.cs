namespace AutomaticRoadblocks
{
    public interface IOnDutyListener
    {
        /// <summary>
        /// Invoked when the duty of LSPDFR is started.
        /// </summary>
        void OnDutyStarted();

        /// <summary>
        /// Invoked when the duty of LSPDFR is stopped.
        /// </summary>
        void OnDutyEnded();
    }
}