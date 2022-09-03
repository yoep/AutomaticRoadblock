namespace AutomaticRoadblocks.Utils.Road
{
    public enum EVehicleNodeType
    {
        /// <summary>
        /// Main roads, include junctions
        /// </summary>
        MainRoadsWithJunctions = 0,
        /// <summary>
        /// All Nodes (main roads, slow roads, including junction nodes)
        /// </summary>
        AllNodes = 1,
        /// <summary>
        /// Water nodes only
        /// </summary>
        Water = 3,
        /// <summary>
        /// Main roads, no junction nodes (you get no nodes at a junction)
        /// </summary>
        MainRoads = 4,
        /// <summary>
        /// All roads, no junction nodes
        /// </summary>
        AllRoadNoJunctions = 5
    }
}