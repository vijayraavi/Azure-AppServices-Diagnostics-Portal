using System;
using System.Runtime.Serialization;

namespace Diagnostics.ScriptHost.Models
{
    public class Definition : Attribute, IEquatable<Definition>
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        public bool Equals(Definition other)
        {
            return this.Id == other.Id;
        }
    }
}
