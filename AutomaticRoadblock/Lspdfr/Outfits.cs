using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Lspdfr
{
    [XmlRoot("Outfits")]
    public class Outfits
    {
        /// <summary>
        /// Retrieve an outfit by the given script name.
        /// </summary>
        /// <param name="scriptName">The script name of the outfit to retrieve.</param>
        [XmlIgnore]
        public Outfit this[string scriptName] => Items.First(x => x.ScriptName.Equals(scriptName, StringComparison.CurrentCultureIgnoreCase));

        [XmlUnwrapContents] public List<Outfit> Items { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Items)}: [{string.Join(", ", Items)}]";
        }
    }
}