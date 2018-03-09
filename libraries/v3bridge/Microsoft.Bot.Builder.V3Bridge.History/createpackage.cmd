@echo off
echo *** Building Microsoft.Bot.Builder.History
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Builder.History*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Builder.History.csproj 
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.dll).FileVersionInfo.FileVersion"') do set builder=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.History.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack Microsoft.Bot.Builder.History.nuspec -symbols -properties version=%version%;builder=%builder% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder.History

