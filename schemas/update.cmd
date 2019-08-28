@echo off
setlocal

rem ensure we are in the right directory
pushd .
cd /D "%~dp0"

rem Get the current branch
set current=
for /f "delims=" %%a in ('git rev-parse --abbrev-ref HEAD') do @set current=%%a 

rem Ensure we have a new branch or 4.Future by default
set branch=%1
if "%branch%" neq "" goto update
set branch=4.Future

rem Update .schema 
:update
echo Updating .schema files and building sdk.schema for branch %branch%
bf dialog:merge ../libraries/**/*.schema -u -b %branch% -o sdk.schema
echo *** Schema files will not be available until branch %current% is merged into %branch% ***

:done
popd