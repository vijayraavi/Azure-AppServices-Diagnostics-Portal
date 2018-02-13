using System.Threading.Tasks;

namespace Diagnostics.ScriptHost.SourceWatcher.Interfaces
{
    public interface ISourceWatcher
    {
        Task WaitForCompletion();
    }
}
