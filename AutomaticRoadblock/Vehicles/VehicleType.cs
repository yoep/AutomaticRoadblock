using System.Collections.Generic;
using AutomaticRoadblocks.Localization;

namespace AutomaticRoadblocks.Vehicles
{
    public class VehicleType
    {
        public static readonly VehicleType LocalUnit = new(LocalizationKey.VehicleTypeLocal);
        public static readonly VehicleType StateUnit = new(LocalizationKey.VehicleTypeState);
        public static readonly VehicleType SwatTeam = new(LocalizationKey.VehicleTypeFbi);
        public static readonly VehicleType NooseTeam = new(LocalizationKey.VehicleTypeSwat);
        public static readonly VehicleType PrisonerTransport = new(LocalizationKey.VehicleTypeTransporter);
        public static readonly VehicleType None = new(LocalizationKey.None);

        public static readonly IEnumerable<VehicleType> Values = new[]
        {
            LocalUnit,
            StateUnit,
            SwatTeam,
            NooseTeam,
            PrisonerTransport,
            None
        };

        private VehicleType(LocalizationKey localizationKey)
        {
            LocalizationKey = localizationKey;
        }
        
        /// <summary>
        /// The localization key for this type.
        /// </summary>
        public LocalizationKey LocalizationKey { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return LocalizationKey.DefaultText;
        }
    }
}