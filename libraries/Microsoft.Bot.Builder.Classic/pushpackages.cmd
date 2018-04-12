@echo off
setlocal enabledelayedexpansion
if "%1" == "" goto :needDir
if "%2" == "" goto :needKey
FOR %%G in (%1\*.nupkg) DO (
    set "fname=%%~nG"
    if "!fname!"=="!fname:symbols=!" (
        nuget push %%G -Source https://www.nuget.org/api/v2/package -ApiKey %2
    )
)
@goto end

:needDir
:needKey
echo "pushpackages <signed directory> <nuget.org api key>"
echo "This will publish all *.nupkg including symbols to nuget.org."
echo "The API Key can be found by logging into nuget.org with BotFramework@outlook.com"

:end    
endlocal
