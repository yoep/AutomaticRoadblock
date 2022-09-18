using System.Collections.Generic;
using System.Xml.Serialization;
using AutomaticRoadblocks.Barriers;
using AutomaticRoadblocks.LightSources;
using AutomaticRoadblocks.Lspdfr;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Roadblock.Data
{
    [XmlRoot("Roadblocks")]
    public class Roadblocks
    {
        public static readonly Roadblocks Defaults = new()
        {
            Items = new List<RoadblockData>
            {
                new()
                {
                    Level = 1,
                    MainBarrier = Barrier.SmallConeStripesScriptName,
                    Lights = { Light.FlaresScriptName },
                    Units = new List<Unit> { new(EBackupUnit.LocalPatrol) }
                },
                new()
                {
                    Level = 2,
                    MainBarrier = Barrier.BigConeScriptName,
                    Lights = { Light.FlaresScriptName },
                    Units = new List<Unit> { new(EBackupUnit.LocalPatrol, 70), new(EBackupUnit.LocalPatrol, 30) }
                },
                new()
                {
                    Level = 3,
                    MainBarrier = Barrier.PoliceDoNotCrossScriptName,
                    Lights = { Light.FlaresScriptName, Light.GroundStandingSpotsScriptName },
                    Units = new List<Unit> { new(EBackupUnit.LocalPatrol, 50), new(EBackupUnit.LocalPatrol, 40), new(EBackupUnit.Transporter, 10) }
                },
                new()
                {
                    Level = 4,
                    MainBarrier = Barrier.PoliceDoNotCrossScriptName,
                    ChaseVehicleBarrier = Barrier.BarrelScriptName,
                    Lights = { Light.SpotsScriptName },
                    Units = new List<Unit> { new(EBackupUnit.LocalSWAT, 70), new(EBackupUnit.StatePatrol, 20), new(EBackupUnit.NooseSWAT, 10) }
                },
                new()
                {
                    Level = 5,
                    MainBarrier = Barrier.PoliceDoNotCrossScriptName,
                    ChaseVehicleBarrier = Barrier.BarrelScriptName,
                    Lights = { Light.SpotsScriptName },
                    Units = new List<Unit> { new(EBackupUnit.NooseSWAT, 70), new(EBackupUnit.LocalSWAT, 30) }
                }
            }
        };

        [XmlUnwrapContents] public List<RoadblockData> Items { get; internal set; }
    }
}