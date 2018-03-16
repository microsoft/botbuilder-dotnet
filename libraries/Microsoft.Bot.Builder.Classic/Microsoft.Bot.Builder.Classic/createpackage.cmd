@echo off
echo *** Building Microsoft.Bot.Builder.Classic ***
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Builder.Classic*.nupkg
msbuild /property:Configuration=release Microsoft.Bot.Builder.Classic.csproj 
msbuild /property:Configuration=release ..\Microsoft.Bot.Builder.Classic.Autofac\Microsoft.Bot.Builder.Classic.Autofac.csproj 
msbuild /property:Configuration=release ..\rview\rview.csproj
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.Classic.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Builder.Classic.nuspec -symbols -properties version=%version% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder.Classic 
