using Diagnostics.ScriptHost.Models;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Diagnostics.ScriptHost.Utilities
{
    internal class AttributeHelper
    {
        public static Definition CreateDefinitionAttribute(AttributeData attr)
        {
            // TODO : Maybe there is a better way to create class object using  attr.AttributeClass (INamedTypeSymbol)
            // Need to explore that. Right now, a poor man solution below

            var def = new Definition
            {
                Name = attr.NamedArguments.Where(p => p.Key == "Name").FirstOrDefault().Value.Value.ToString(),
                Id = attr.NamedArguments.Where(p => p.Key == "Id").FirstOrDefault().Value.Value.ToString(),
                Description = attr.NamedArguments.Where(p => p.Key == "Description").FirstOrDefault().Value.Value.ToString()
            };

            return def;
        }
    }
}
