@echo off
if "%1" == "" goto usage

setlocal
echo *** Importing LUIS app %1
set luisapp=%~n1
call luisid %luisapp% > NUL
if "%luisid%" == "" (
    luis import application --in %1
    call luisid %luisapp%
    call luis train version --versionId 0.1 --appId %luisid% --wait
    call luis publish version --versionId 0.1 --appId %luisid%
) else (
    echo LUIS application %luisapp% already exists
)
goto done

:usage
echo luisimport LUIS_APP.json
echo Will import, train and publish LUIS_APP into LUIS unless it already exists.

:done