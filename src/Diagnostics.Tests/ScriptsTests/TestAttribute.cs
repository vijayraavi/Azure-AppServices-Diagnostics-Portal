using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Diagnostics.Tests.ScriptsTests
{
    public class TestAttribute : Attribute, IEquatable<TestAttribute>
    {
        [DataMember]
        public string Name { get; set; }

        public bool Equals(TestAttribute other)
        {
            return this.Name == other.Name;
        }
    }
}
