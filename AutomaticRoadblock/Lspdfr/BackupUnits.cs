using System;
using System.Xml.Serialization;

namespace AutomaticRoadblocks.Lspdfr
{
    [XmlRoot("BackupUnits")]
    public class BackupUnits
    {
        private Backup this[EBackupUnit unit] => GetBackup(unit);

        public Backup LocalPatrol { get; internal set; }

        public Backup StatePatrol { get; internal set; }

        public Backup LocalSWAT { get; internal set; }

        public Backup NooseSWAT { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(LocalPatrol)}: {LocalPatrol}, {nameof(StatePatrol)}: {StatePatrol}, {nameof(LocalSWAT)}: {LocalSWAT}, " +
                   $"{nameof(NooseSWAT)}: {NooseSWAT}";
        }

        protected bool Equals(BackupUnits other)
        {
            return Equals(LocalPatrol, other.LocalPatrol) && Equals(StatePatrol, other.StatePatrol) && Equals(LocalSWAT, other.LocalSWAT) &&
                   Equals(NooseSWAT, other.NooseSWAT);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BackupUnits)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LocalPatrol != null ? LocalPatrol.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StatePatrol != null ? StatePatrol.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LocalSWAT != null ? LocalSWAT.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NooseSWAT != null ? NooseSWAT.GetHashCode() : 0);
                return hashCode;
            }
        }
        
        private Backup GetBackup(EBackupUnit unit)
        {
            return unit switch
            {
                EBackupUnit.LocalPatrol => LocalPatrol,
                EBackupUnit.StatePatrol => StatePatrol,
                EBackupUnit.Transporter => null,
                EBackupUnit.LocalSWAT => LocalSWAT,
                EBackupUnit.NooseSWAT => NooseSWAT,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "unit is not supported")
            };
        }
    }
}