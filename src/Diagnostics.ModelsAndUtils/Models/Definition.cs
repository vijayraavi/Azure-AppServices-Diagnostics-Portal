using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Diagnostics.ModelsAndUtils
{
    public class Definition : Attribute
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
        
    }
}
