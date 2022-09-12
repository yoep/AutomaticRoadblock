using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class Backup
    {
        [XmlIgnore]
        public List<BackupAgency> this[string zone] => GetType().GetProperties()
            .Where(x => x.Name.Equals(zone))
            .Select(x => x.GetValue(this))
            .Select(x => (List<BackupAgency>)x)
            .FirstOrDefault();

        public List<BackupAgency> LosSantosCity { get; internal set; }

        public List<BackupAgency> LosSantosCounty { get; internal set; }

        public List<BackupAgency> BlaineCounty { get; internal set; }

        public List<BackupAgency> NorthYankton { get; internal set; }

        [XmlElement(IsNullable = true)] public List<BackupAgency> MountChiliad { get; internal set; }

        public List<BackupAgency> CayoPerico { get; internal set; }
    }
}