rem @echo off

set SolutionDir=%~1
set ProjectPath=%~2
set TargetDir=%~3
set ConfigurationName=%~4

set Suffix="dev"
if "%ConfigurationName%" NEQ "Debug" (
	set Suffix=""
)

set NuGetPath=%NUGET_PATH%
if "%NuGetPath%" == "" (
   set NuGetPath=%TargetDir%
)

if not exist "%NuGetPath%" mkdir %NuGetPath%

%SolutionDir%..\deploy\nuget.exe pack %ProjectPath% -Suffix %Suffix% -Properties Configuration=%ConfigurationName% -OutputDirectory %NuGetPath%

:exit