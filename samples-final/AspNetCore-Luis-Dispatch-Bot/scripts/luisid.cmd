@echo off
set luisid=
if "%1" == "" goto usage

call luis list applications > %temp%\luisid.json
if %ERRORLEVEL% EQU 0 for /f %%i in ('jspath ".{.name === \"%1\"}.id" %temp%\luisid.json') do set luisid=%%i
if "%luisid%" == "" (
    echo LUIS application %1 does not exist.
) else (
    echo %luisid%
)
goto done

:usage
echo "luisid LUIS_APP_NAME"
echo "Reports the LUIS app id and sets the environment variable luisid to the id."

:done