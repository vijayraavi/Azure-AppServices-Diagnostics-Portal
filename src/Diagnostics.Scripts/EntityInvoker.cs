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
        private ImmutableArray<AttributeData> _attributes;

        public bool IsCompilationSuccessful { get; private set; }

        public IEnumerable<string> CompilationOutput { get; private set; }

        public ImmutableArray<AttributeData> Attributes => _attributes;

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

        /// <summary>
        /// Initializes the entry point by compiling the script and loading/saving the assembly
        /// </summary>
        /// <param name="assemblyInitType"></param>
        /// <returns></returns>
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
                    Assembly assembly = await _compilation.EmitAssemblyAsync();
                    _entryPointMethodInfo = methodSignature.GetMethod(assembly);
                    _attributes = methodSignature.Attributes;
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

        /// <summary>
        /// Initializes the entry point from already loaded assembly.
        /// </summary>
        /// <param name="asm">Assembly</param>
        public void InitializeEntryPoint(Assembly asm)
        {
            if(asm == null)
            {
                throw new ArgumentNullException("Assembly cannot be null");
            }

            // TODO : We might need to create a factory to get compilation object based on type.
            _compilation = new SignalCompilation();

            // If assembly is present, that means compilation was successful.
            IsCompilationSuccessful = true;
            EntityMethodSignature methodSignature = _compilation.GetEntryPointSignature();
            _entryPointMethodInfo = methodSignature.GetMethod(asm);
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

        public async Task<string> SaveAssemblyToDiskAsync(string assemblyPath)
        {
            ICompilationService compilationService = CompilationServiceFactory.CreateService(_entityMetaData, GetScriptOptions(_frameworkReferences));
            _compilation = await compilationService.GetCompilationAsync();
            _diagnostics = await _compilation.GetDiagnosticsAsync();

            IsCompilationSuccessful = !_diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            CompilationOutput = _diagnostics.Select(m => m.ToString());

            if (!IsCompilationSuccessful)
            {
                return string.Empty;
            }

            return await _compilation.SaveAssemblyAsync(assemblyPath);
        }

        public async Task<Tuple<string, string>> GetAssemblyBytesAsync()
        {
            ICompilationService compilationService = CompilationServiceFactory.CreateService(_entityMetaData, GetScriptOptions(_frameworkReferences));
            _compilation = await compilationService.GetCompilationAsync();
            _diagnostics = await _compilation.GetDiagnosticsAsync();
            
            IsCompilationSuccessful = !_diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            CompilationOutput = _diagnostics.Select(m => m.ToString());

            if (!IsCompilationSuccessful)
            {
                return Tuple.Create(string.Empty, string.Empty);
            }

            return await _compilation.GetAssemblyBytesAsync();
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
