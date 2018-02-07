using Diagnostics.Scripts.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
namespace Diagnostics.Tests.ScriptsTests
{
    public static class ScriptTestDataHelper
    {
        public static EntityMetadata GetRandomMetadata(EntityType type = EntityType.Signal)
        {
            return new EntityMetadata()
            {
                Name = "RandomEntity",
                scriptText = GetNumSqaureScript(),
                Type = type
            };
        }
        
        public static string GetNumSqaureScript()
        {
            return @"
                public static int Run(int x) {
                    x = x * x;
                    return x;
                }";
        }

        public static string GetAttributedEntryPointMethodScript(TestAttribute attr)
        {
            return $@"
                [TestAttribute(Name=""{attr.Name}"")]
                public static int Run(int x) {{
                    x = x * x;
                    return x;
                }}";
        }

        public static string GetInvalidCsxScript(ScriptErrorType errorType)
        {
            switch (errorType)
            {
                case ScriptErrorType.MissingEntryPoint:
                    return @"
                        public static string SomeMethod() => ""test string"";
                    ";
                case ScriptErrorType.DuplicateEntryPoint:
                    return @"
                        public static int Run(int x) {
                            return x * x;
                        }
                        public static int Run(int x, int y) {
                            return x + y;
                        }
                    ";
                case ScriptErrorType.CompilationError:
                default:
                    return @"
                        public static int Run(int x) {
                        return x * x
                    }";
            }
        }

        public static ImmutableArray<string> GetFrameworkReferences() => ImmutableArray.Create(
                "System.Data",
                "Diagnostics.DataProviders"
            );

        public static ImmutableArray<string> GetFrameworkImports() => ImmutableArray.Create(
                "System.Data",
                "System.Threading.Tasks",
                "Diagnostics.DataProviders"
            );
    }
}
