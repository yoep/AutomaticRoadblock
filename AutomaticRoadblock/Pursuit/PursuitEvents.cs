namespace AutomaticRoadblocks.Pursuit
{
    public static class PursuitEvents
    {
        public delegate void PursuitStateChangedEventHandler(bool isPursuitActive);

        public delegate void PursuitLevelChangedEventHandler(PursuitLevel newPursuitLevel);
    }
}