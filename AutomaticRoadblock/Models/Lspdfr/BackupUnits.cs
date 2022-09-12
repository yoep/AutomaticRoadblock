using System.Collections.Generic;
using System.Xml.Serialization;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    [XmlRoot("BackupUnits")]
    public class BackupUnits
    {
        public static readonly BackupUnits Default = new()
        {
            LocalPatrol = new Backup
            {
                LosSantosCity = new List<BackupAgency> { new("lspd") },
                LosSantosCounty = new List<BackupAgency> { new("lssd") },
                BlaineCounty = new List<BackupAgency> { new("lssd") },
                NorthYankton = new List<BackupAgency> { new("nysp") },
                MountChiliad = new List<BackupAgency> { new("sapr") },
                CayoPerico = new List<BackupAgency> { new("sapr") },
            },
            StatePatrol = new Backup
            {
                LosSantosCity = new List<BackupAgency> { new("sahp") },
                LosSantosCounty = new List<BackupAgency> { new("sahp") },
                BlaineCounty = new List<BackupAgency> { new("sahp") },
                NorthYankton = new List<BackupAgency> { new("nysp") },
                CayoPerico = new List<BackupAgency> { new("sahp") },
            },
        };

        public Backup LocalPatrol { get; internal set; }

        public Backup StatePatrol { get; internal set; }

        public Backup LocalSWAT { get; internal set; }

        public Backup NooseSWAT { get; internal set; }

        public Backup LocalAir { get; internal set; }

        public Backup NooseAir { get; internal set; }
    }
}