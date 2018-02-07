using Diagnostics.Scripts.CompilationService;
using Diagnostics.Scripts.CompilationService.Interfaces;
using Diagnostics.Scripts.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostics.Scripts
{
    public sealed class EntityInvoker : IDisposable
    {
        private EntityMetadata _entityMetaData;
        private ImmutableArray<string> _frameworkReferences;
        private ImmutableArray<string> _frameworkImports;
        private ICompilation _compilation;
        private ImmutableArray<Diagnostic> _diagnostics;
        private MethodInfo _entryPointMethodInfo;

        public bool IsCompilationSuccessful { get; private set; }

        public IEnumerable<string> CompilationOutput { get; private set; }

        public EntityInvoker(EntityMetadata entityMetadata, ImmutableArray<string> frameworkReferences)
        {
            _entityMetaData = entityMetadata;
            _frameworkReferences = frameworkReferences;
            _frameworkImports = ImmutableArray.Create<string>();
            CompilationOutput = Enumerable.Empty<string>();
        }

        public EntityInvoker(EntityMetadata entityMetadata, ImmutableArray<string> frameworkReferences, ImmutableArray<string> frameworkImports)
        {
            _entityMetaData = entityMetadata;
            _frameworkImports = frameworkImports;
            _frameworkReferences = frameworkReferences;
            CompilationOutput = Enumerable.Empty<string>();
        }

        public async Task InitializeEntryPointAsync()
        {
            ICompilationService compilationService = CompilationServiceFactory.CreateService(_entityMetaData, GetScriptOptions(_frameworkReferences));
            _compilation = await compilationService.GetCompilationAsync();
            _diagnostics = await _compilation.GetDiagnosticsAsync();

            IsCompilationSuccessful = !_diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            CompilationOutput = _diagnostics.Select(m => m.ToString());

            if (IsCompilationSuccessful)
            {
                try
                {
                    EntityMethodSignature methodSignature = _compilation.GetEntryPointSignature();
                    Assembly assembly = await _compilation.EmitAsync();
                    _entryPointMethodInfo = methodSignature.GetMethod(assembly);
                }
                catch(Exception ex)
                {
                    if(ex is ScriptCompilationException)
                    {
                        IsCompilationSuccessful = false;

                        if (!string.IsNullOrWhiteSpace(ex.Message))
                        {
                            CompilationOutput.Concat(new[] { ex.Message });
                        }

                        return;
                    }

                    throw ex;
                }
            }
        }

        public async Task<object> Invoke(object[] parameters)
        {
            if (!IsCompilationSuccessful)
            {
                throw new ScriptCompilationException();
            }

            int actualParameterCount = _entryPointMethodInfo.GetParameters().Length;
            parameters = parameters.Take(actualParameterCount).ToArray();
            
            object result = _entryPointMethodInfo.Invoke(null, parameters);

            if (result is Task)
            {
                result = await ((Task)result).ContinueWith(t => GetTaskResult(t));
            }

            return result;
        }

        private ScriptOptions GetScriptOptions(ImmutableArray<string> frameworkReferences)
        {
            ScriptOptions scriptOptions = ScriptOptions.Default;

            if (!frameworkReferences.IsDefaultOrEmpty)
            {
                scriptOptions = ScriptOptions.Default
                    .WithReferences(frameworkReferences);
            }

            if (!_frameworkImports.IsDefaultOrEmpty)
            {
                scriptOptions = scriptOptions.WithImports(_frameworkImports);
            }

            return scriptOptions;
        }

        internal static object GetTaskResult(Task task)
        {
            if (task.IsFaulted)
            {
                throw task.Exception;
            }

            Type taskType = task.GetType();

            if (taskType.IsGenericType)
            {
                return taskType.GetProperty("Result").GetValue(task);
            }

            return null;
        }

        public void Dispose()
        {
            _compilation = null;
            _entryPointMethodInfo = null;
        }
    }
}
