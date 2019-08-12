@echo off
echo This runs LUIS tests against the LUIS service and will diff changes if any.
echo Usage: test [optional mock=true or false] [optional subscription key or LUISSUBSCRIPTIONKEY from environment]
setlocal
set LUISMOCK=false
if "%1" == "" goto key
set LUISMOCK=%1

:key
if "%2" == "" goto check
set LUISSUBSCRIPTIONKEY=%2

:check
if "%LUISSUBSCRIPTIONKEY" neq "" goto run
echo Missing subscription key
goto done

:run
echo Running LUIS tests with LUISMOCK=%LUISMOCK% and LUISSUBSCRIPTIONKEY=%LUISSUBSCRIPTIONKEY%
dotnet test
if %errorlevel% neq 0 goto review

pushd ..\Microsoft.Bot.Builder.Ai.LuisV3.Tests
dotnet test
popd
if %errorlevel% == 0 goto done

:review
cd TestData
echo Reviewing changes
call review.cmd

:done