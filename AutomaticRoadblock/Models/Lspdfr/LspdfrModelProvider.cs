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

        /// <inheritdoc />
        public Model CopPed(Vector3 position, EUnitType type)
        {
            var modelInfo = _modelData.Ped(type, Functions.GetZoneAtPosition(position).County);
            
            return new Model(modelInfo.Name);
        }

        /// <inheritdoc />
        public Model CopVehicle(Vector3 position, EUnitType type)
        {
            var modelInfo = _modelData.Vehicle(type, Functions.GetZoneAtPosition(position).County);
            return new Model(modelInfo.Name);
        }
    }
}