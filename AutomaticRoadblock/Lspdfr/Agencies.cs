using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Lspdfr
{
    [XmlRoot("Agencies")]
    public class Agencies
    {
        [XmlIgnore]
        public Agency this[string scriptName] => GetAgencyByScriptName(scriptName);

        [XmlUnwrapContents]
        public List<Agency> Items { get; internal set; }
        
        private Agency GetAgencyByScriptName(string scriptName)
        {
            Assert.HasText(scriptName, "scriptName cannot be empty");
            return Items.First(x => x.ScriptName.Equals(scriptName, StringComparison.InvariantCulture));
        }
    }
}