using System.Xml.Serialization;
using JetBrains.Annotations;

namespace AutomaticRoadblocks.Lspdfr
{
    public class Backup
    {
        public Backup()
        {
        }

        public Backup(string losSantosCity, string losSantosCounty, string blaineCounty, string northYankton, string cayoPerico)
        {
            LosSantosCity = losSantosCity;
            LosSantosCounty = losSantosCounty;
            BlaineCounty = blaineCounty;
            NorthYankton = northYankton;
            CayoPerico = cayoPerico;
        }

        public Backup(string losSantosCity, string losSantosCounty, string blaineCounty, string northYankton, [CanBeNull] string mountChiliad,
            string cayoPerico)
        {
            LosSantosCity = losSantosCity;
            LosSantosCounty = losSantosCounty;
            BlaineCounty = blaineCounty;
            NorthYankton = northYankton;
            MountChiliad = mountChiliad;
            CayoPerico = cayoPerico;
        }

        public string LosSantosCity { get; internal set; }

        public string LosSantosCounty { get; internal set; }

        public string BlaineCounty { get; internal set; }

        public string NorthYankton { get; internal set; }

        [XmlElement(IsNullable = true)]
        [CanBeNull]
        public string MountChiliad { get; internal set; }

        public string CayoPerico { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(LosSantosCity)}: {LosSantosCity}, {nameof(LosSantosCounty)}: {LosSantosCounty}, {nameof(BlaineCounty)}: {BlaineCounty}, " +
                   $"{nameof(NorthYankton)}: {NorthYankton}, {nameof(MountChiliad)}: {MountChiliad}, {nameof(CayoPerico)}: {CayoPerico}";
        }

        protected bool Equals(Backup other)
        {
            return LosSantosCity == other.LosSantosCity && LosSantosCounty == other.LosSantosCounty && BlaineCounty == other.BlaineCounty &&
                   NorthYankton == other.NorthYankton && MountChiliad == other.MountChiliad && CayoPerico == other.CayoPerico;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Backup)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LosSantosCity != null ? LosSantosCity.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LosSantosCounty != null ? LosSantosCounty.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BlaineCounty != null ? BlaineCounty.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NorthYankton != null ? NorthYankton.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MountChiliad != null ? MountChiliad.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CayoPerico != null ? CayoPerico.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}