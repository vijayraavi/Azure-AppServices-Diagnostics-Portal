using System;
using System.Collections.Generic;
using System.Text;

namespace Diagnostics.Scripts
{
    public class ScriptCompilationException : Exception
    {
        public ScriptCompilationException()
        {
        }

        public ScriptCompilationException(string message)
        : base(message)
        {
        }

        public ScriptCompilationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
