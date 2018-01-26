using Diagnostics.Scripts.CompilationService.Interfaces;
using Diagnostics.Scripts.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Diagnostics.Scripts.CompilationService
{
    public abstract class CompilationBase : ICompilation
    {
        private readonly Compilation _compilation;
        private readonly EntryPointResolutionType _resolutionType;
        private readonly string _entryPointName;

        protected abstract ImmutableArray<DiagnosticAnalyzer> GetCodeAnalyzers();

        public CompilationBase(Compilation compilation, EntryPointResolutionType resolutionType, string entryPointName)
        {
            _compilation = compilation;
            _resolutionType = resolutionType;
            _entryPointName = entryPointName;
        }

        public Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync()
        {
            ImmutableArray<DiagnosticAnalyzer> analyzers = GetCodeAnalyzers();
            if (analyzers.IsEmpty)
            {
                return Task.Factory.StartNew(() => _compilation.GetDiagnostics());
            }

            return _compilation.WithAnalyzers(GetCodeAnalyzers()).GetAllDiagnosticsAsync();
        }

        public EntityMethodSignature GetEntryPointSignature()
        {
            var methods = _compilation.ScriptClass
                .GetMembers()
                .OfType<IMethodSymbol>();

            IMethodSymbol entryPointReference = default(IMethodSymbol);

            switch (_resolutionType)
            {
                case EntryPointResolutionType.Attribute:
                    break;
                case EntryPointResolutionType.MethodName:
                default:
                    entryPointReference = GetMethodByName(methods, _entryPointName);
                    break;
            }

            if (entryPointReference == default(IMethodSymbol))
            {
                throw new EntryPointNotFoundException($"No Entry point found. Entry point resoultion type : {_resolutionType} , value : {_entryPointName}");
            }

            var methodParameters = entryPointReference.Parameters.Select(p => new EntityParameter(p.Name, GetFullTypeName(p.Type), p.IsOptional, p.RefKind));

            return new EntityMethodSignature(
                entryPointReference.ContainingType.ToDisplayString(),
                entryPointReference.Name,
                ImmutableArray.CreateRange(methodParameters.ToArray()),
                GetFullTypeName(entryPointReference.ReturnType));
        }

        public Task<Assembly> EmitAsync()
        {
            return Task.Factory.StartNew<Assembly>(() =>
            {
                try
                {
                    using (var assemblyStream = new MemoryStream())
                    using (var pdbStream = new MemoryStream())
                    {
                        _compilation.Emit(assemblyStream, pdbStream);
                        return Assembly.Load(assemblyStream.GetBuffer(), pdbStream.GetBuffer());
                    }
                }
                catch (Exception)
                {
                    // TODO : Need to throw custom exception?
                    throw;
                }
            });
        }

        private IMethodSymbol GetMethodByName(IEnumerable<IMethodSymbol> methods, string methodName)
        {
            var namedMethods = methods
                       .Where(m => m.DeclaredAccessibility == Accessibility.Public && string.Compare(m.Name, methodName, StringComparison.Ordinal) == 0)
                       .ToList();

            if (namedMethods.Count == 1)
            {
                return namedMethods.First();
            }

            // If we have multiple public methods matching the provided name, throw a compilation exception
            if (namedMethods.Count > 1)
            {
                throw new ScriptCompilationException($"Multiple Entry Point Methods with name {methodName} found.");
            }

            return default(IMethodSymbol);
        }

        private string GetFullTypeName(ITypeSymbol type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            return type.ContainingAssembly == null
                ? type.ToDisplayString()
                : string.Format(CultureInfo.InvariantCulture, "{0}, {1}", type.ToDisplayString(), type.ContainingAssembly.ToDisplayString());
        }
    }
}
