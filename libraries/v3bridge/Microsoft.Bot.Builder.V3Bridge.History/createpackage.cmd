@echo off
echo *** Building Microsoft.Bot.Builder.V3Bridge.History
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Builder.V3Bridge.History*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Builder.V3Bridge.History.csproj 
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.V3Bridge.dll).FileVersionInfo.FileVersion"') do set builder=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.V3Bridge.History.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Builder.V3Bridge.History.nuspec -symbols -properties version=%version%;builder=%builder% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder.V3Bridge.History

