@echo off
echo *** Building Microsoft.Bot.Connector.Common
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Connector.Common*nupkg
msbuild /property:Configuration=release /t:pack;copyPackage Microsoft.Bot.Connector.Common.csproj
echo *** Finished building Microsoft.Bot.Connector.Common
