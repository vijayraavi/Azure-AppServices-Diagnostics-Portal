using Diagnostics.Scripts.Models;
using System;
using System.Collections.Generic;
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
    }
}
