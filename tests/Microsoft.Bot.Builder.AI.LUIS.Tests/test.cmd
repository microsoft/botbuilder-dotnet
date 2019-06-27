@echo off
setlocal
set LUISMOCK=false
if "%1" == "" goto run
set LUISMOCK=%1
:run
echo Running LUIS tests with LUISMOCK=%LUISMOCK%
dotnet test
