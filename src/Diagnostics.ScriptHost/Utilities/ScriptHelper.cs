using System.Collections.Immutable;

namespace Diagnostics.ScriptHost.Utilities
{
    internal class ScriptHelper
    {
        public static ImmutableArray<string> GetFrameworkReferences() => ImmutableArray.Create(
                "System.Data",
                "Diagnostics.DataProviders",
                "Diagnostics.ScriptHost"    // TODO: Since the models are in script host project right now, we have to pass this reference.
            );

        public static ImmutableArray<string> GetFrameworkImports() => ImmutableArray.Create(
                "System.Data",
                "System.Threading.Tasks",
                "Diagnostics.DataProviders",
                "Diagnostics.ScriptHost.Models",
                "Diagnostics.ScriptHost.Utilities"  // TODO : just as model, we might want to separate out utilities also that are passed to script.
            );
    }
}
