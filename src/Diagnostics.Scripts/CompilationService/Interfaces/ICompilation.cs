using Diagnostics.Scripts.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;

namespace Diagnostics.Scripts.CompilationService.Interfaces
{
    public interface ICompilation
    {
        Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync();

        EntityMethodSignature GetEntryPointSignature();

        Task<Assembly> EmitAsync();
    }
}
