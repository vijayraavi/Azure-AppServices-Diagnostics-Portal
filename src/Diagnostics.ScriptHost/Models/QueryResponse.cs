using System.Collections.Generic;

namespace Diagnostics.ScriptHost.Models
{
    public class QueryResponse<T>
    {
        public bool CompilationSucceeded;

        public IEnumerable<string> CompilationOutput;

        public T InvocationOutput;
    }
}
