﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.Models
{
    public class DiagnosticEntityResponse<T>
    {
        public bool CompilationSucceeded;

        public IEnumerable<string> CompilationOutput;

        public T InvocationOutput;
    }
}
