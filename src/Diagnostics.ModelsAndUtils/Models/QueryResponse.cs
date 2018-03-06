using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.ModelsAndUtils
{
    public class QueryResponse<T>
    {
        public bool CompilationSucceeded;

        public IEnumerable<string> CompilationOutput;

        public T InvocationOutput;
    }
}
