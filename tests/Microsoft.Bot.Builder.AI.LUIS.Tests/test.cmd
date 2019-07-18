@echo off
rem This runs LUIS tests against the LUIS service and will diff changes if any.
rem Usage: test <optional true to mock and false to hit server>
setlocal
set LUISMOCK=false
if "%1" == "" goto run
set LUISMOCK=%1
:run
echo Running LUIS tests with LUISMOCK=%LUISMOCK%
dotnet test
if %errorlevel% == 0 goto done
cd TestData
echo Reviewing changes
call review.cmd
:done