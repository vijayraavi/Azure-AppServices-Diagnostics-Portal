using Diagnostics.Scripts.CompilationService.Interfaces;
using Diagnostics.Scripts.Models;
using Microsoft.CodeAnalysis.Scripting;
using System;

namespace Diagnostics.Scripts.CompilationService
{
    public static class CompilationServiceFactory
    {
        public static ICompilationService CreateService(EntityMetadata metaData, ScriptOptions scriptOptions)
        {
            switch (metaData.Type)
            {
                case EntityType.Signal:
                    return new SignalCompilationService(metaData, scriptOptions);
                case EntityType.Detector:
                    return new DetectorCompilationService(metaData, scriptOptions);
                case EntityType.Analysis:
                    return new AnalysisCompilationService(metaData, scriptOptions);
                default:
                    throw new ArgumentException($"EntityMetaData with type {metaData.Type} is invalid.");
            }
        }
    }
}
