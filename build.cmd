@echo off

:: Delete existing build drop
@RD /S /Q "build"

:: Build all the projects in the solution
dotnet build Diagnostics.sln

IF %ERRORLEVEL% NEQ 0 (
echo "Build Failed."
exit /b %errorlevel%
)

:: Publish Compiler Host to Build Location
echo "Publishing Compiler Host to build directory"
dotnet publish src\\Diagnostics.CompilerHost\\Diagnostics.CompilerHost.csproj -c Release -o ..\\..\\build\\Diagnostics.CompilerHost.1.0

IF %ERRORLEVEL% NEQ 0 (
echo "Diagnostics.CompilerHost Publish Failed."
exit /b %errorlevel%
)

:: Publish Runtime Host to Build Location
echo "Publishing Runtime Host to build directory"
dotnet publish src\\Diagnostics.RuntimeHost\\Diagnostics.RuntimeHost.csproj -c Release -o ..\\..\\build\\Diagnostics.RuntimeHost.1.0

IF %ERRORLEVEL% NEQ 0 (
echo "Diagnostics.RuntimeHost Publish Failed."
exit /b %errorlevel%
)