using Diagnostics.ScriptHost.Models;
using Diagnostics.ScriptHost.SourceWatcher.Interfaces;
using System;

namespace Diagnostics.ScriptHost.SourceWatcher
{
    public class SourceWatcherFactory
    {
        public static ISourceWatcher CreateSourceWatcher(SourceWatcherType type)
        {
            // This would be needed when we support mulitple source watchers.
            throw new NotImplementedException(); 
        }
    }
}
