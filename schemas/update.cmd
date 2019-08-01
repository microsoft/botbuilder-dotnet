@echo off
setlocal

rem ensure we are in the right directory
cd /D "%~dp0"

rem Get the current branch
set current=
for /f "delims=" %%a in ('git rev-parse --abbrev-ref HEAD') do @set current=%%a 

rem Ensure we have a new branch or 4.Future by default
set branch=%1
if "%branch%" neq "" goto switch
set branch=4.Future

rem Switch to new branch
:switch
echo *** This will checkout branch %branch% do a pull and update schemas to point to it. ***
set /p yes=Do you want to do this [y/n]? 
if "%yes%" neq "y" goto usage
echo Switching to branch %branch% from %current%
git checkout -b %branch%
git branch --set-upstream-to %branch%
git pull

rem Update .schema
:update
echo Updating .schema files and building sdk.schema
call dialogSchema ../libraries/**/*.schema -u -b %branch% -o sdk.schema

rem Commit
echo Committing
git commit -a -m "Update .schema files to point to branch %branch%"

rem Optionally push
set /p yes=Do you want to push your .schema updates to %branch% [y/n]? 
if "%yes%" neq "y" goto push
echo Pushing schema changes to branch %branch% and switching back to %current%
git push
git checkout %current%
goto done

:push
echo *** You must do "git push" which will update branch %branch% directly ***
goto done

:usage
echo Usage: update [branch]
echo Schema files have a problem in that they need to be present in order to be referred to, but we want them to be release specific.
echo This batch file simplifies that, but make sure you understand what you are doing.  
echo By default it will modify 4.Future which is where development happens.  
echo To do a new release you need to run this tool to put schemas into the new release branch.
echo It will take the following steps:
echo 1) Switch to branch.  If your enlistment is not clean this will fail.
echo 2) Run dialogSchema to modify component.schema and all .schema files to point to new branch and be aggregated in sdk.schema
echo 3) Commit the resulting changes locally.
echo 4) Optionally push the results if not, it is up to you to push the resulting changes directly into the branch
goto done

:done
