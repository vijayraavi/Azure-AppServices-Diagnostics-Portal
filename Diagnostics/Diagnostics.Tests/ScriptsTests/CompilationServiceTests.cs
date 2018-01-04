using Diagnostics.Scripts.CompilationService;
using Diagnostics.Scripts.Models;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Diagnostics.Tests.ScriptsTests
{
    public class CompilationServiceTests
    {
        #region Compilation Service Factory Tests

        [Theory]
        [InlineData(EntityType.Signal, typeof(SignalCompilationService))]
        [InlineData(EntityType.Detector, typeof(DetectorCompilationService))]
        [InlineData(EntityType.Analysis, typeof(AnalysisCompilationService))]
        public void GetCompilationServiceBasedOnType(EntityType type, object value)
        {
            EntityMetadata metaData = ScriptTestDataHelper.GetRandomMetadata(type);
            var compilationServiceInstance = CompilationServiceFactory.CreateService(metaData, ScriptOptions.Default);
            Assert.Equal(compilationServiceInstance.GetType(), value);
        }

        [Fact]
        public void TestFactoryForNullEntityMetadata()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var instance = CompilationServiceFactory.CreateService(null, ScriptOptions.Default);
            });
        }

        [Fact]
        public void TestFactoryForNullScriptOptions()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var instance = CompilationServiceFactory.CreateService(ScriptTestDataHelper.GetRandomMetadata(), null);
            });
        }

        [Fact]
        public void TestFactoryForUnsupportedEntityType()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var instance = CompilationServiceFactory.CreateService(new EntityMetadata(), ScriptOptions.Default);
            });
        }

        #endregion
    }
}
