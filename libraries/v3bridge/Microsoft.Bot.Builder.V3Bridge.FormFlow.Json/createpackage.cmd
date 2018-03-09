@echo off
echo *** Building Microsoft.Bot.Builder.FormFlow.Json
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Builder.FormFlow.Json*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Builder.FormFlow.Json.csproj
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.FormFlow.Json.dll).FileVersionInfo.FileVersion"') do set version=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.dll).FileVersionInfo.FileVersion"') do set builder=%%v
..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Builder.FormFlow.Json.nuspec -symbols -properties version=%version%;builder=%builder% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder.FormFlow.Json

