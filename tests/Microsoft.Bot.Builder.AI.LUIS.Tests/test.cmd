@echo off
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

