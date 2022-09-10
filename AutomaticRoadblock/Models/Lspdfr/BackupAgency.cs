using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Models.Lspdfr
{
    public class BackupAgency
    {
        public BackupAgency()
        {
        }

        public BackupAgency(string name)
        {
            Name = name;
        }

        [XmlUnwrapContents]
        public string Name { get; internal set; }
    }
}