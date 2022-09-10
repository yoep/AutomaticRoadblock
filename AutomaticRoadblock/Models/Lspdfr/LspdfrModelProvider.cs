using AutomaticRoadblocks.AbstractionLayer;
using LSPD_First_Response.Mod.API;
using Rage;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    internal class LspdfrModelProvider : IModelProvider
    {
        private readonly ILogger _logger;
        private readonly IModelData _modelData;

        public LspdfrModelProvider(ILogger logger, IModelData modelData)
        {
            _logger = logger;
            _modelData = modelData;
        }

        public Model LocalCopPed(Vector3 position, EUnitType type)
        {
            var modelInfo = _modelData.Ped(EUnitType.LocalPatrol, Functions.GetZoneAtPosition(position).County);
            
            return null;
        }

        public Model LocalCopVehicle(Vector3 position, EUnitType type)
        {
            var zone = Functions.GetZoneAtPosition(position);
            return null;
        }
    }
}