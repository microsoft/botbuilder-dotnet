@echo off
echo *** Building Microsoft.Bot.Builder
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Connector*.nupkg
erase /s ..\nuget\Microsoft.Bot.Builder*.nupkg
msbuild /property:Configuration=release /p:DefineConstants="NET45" ..\Microsoft.Bot.Connector.NetFramework\Microsoft.Bot.Connector.csproj 
msbuild /property:Configuration=release ..\Microsoft.Bot.Builder.Autofac\Microsoft.Bot.Builder.Autofac.csproj 
msbuild /property:Configuration=release Microsoft.Bot.Builder.csproj 
msbuild /property:Configuration=release ..\..\tools\rview\rview.csproj
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Builder.nuspec -symbols -properties version=%version% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder

