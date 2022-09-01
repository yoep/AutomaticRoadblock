namespace AutomaticRoadblocks.Pursuit
{
    public enum EPursuitState
    {
        /// <summary>
        /// Indicates that there is currently no active pursuit ongoing.
        /// </summary>
        Inactive,
        /// <summary>
        /// Indicates that a pursuit is ongoing and vehicle(s) are currently being chased.
        /// </summary>
        ActiveChase,
        /// <summary>
        /// Indicates that a pursuit is ongoing and suspects are chased on foot.
        /// </summary>
        ActiveOnFoot,
        /// <summary>
        /// Indicates that the visual on the suspects of the pursuit are lost.
        /// </summary>
        ActiveVisualLost
    }
}