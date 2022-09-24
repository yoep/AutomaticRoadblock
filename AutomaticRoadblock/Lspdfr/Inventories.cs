using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using AutomaticRoadblocks.Xml.Attributes;

namespace AutomaticRoadblocks.Lspdfr
{
    [XmlRoot("Inventories")]
    public class Inventories
    {
        /// <summary>
        /// Retrieve the inventory by the given script name.
        /// </summary>
        /// <param name="scriptName">The script name of the inventory.</param>
        [XmlIgnore]
        public Inventory this[string scriptName] => Items.First(x => x.ScriptName.Equals(scriptName, StringComparison.CurrentCultureIgnoreCase));

        [XmlUnwrapContents] public List<Inventory> Items { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Items)}: {string.Join(", ", Items)}";
        }

        protected bool Equals(Inventories other)
        {
            return Items.All(x => other.Items.Contains(x));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Inventories)obj);
        }

        public override int GetHashCode()
        {
            return (Items != null ? Items.GetHashCode() : 0);
        }
    }
}