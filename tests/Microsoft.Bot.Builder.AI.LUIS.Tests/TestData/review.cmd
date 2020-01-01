@echo off
rem Service tests run against recorded HTTP oracles.  
rem If you rerun against the service and generate <test>.json.new files, this batch file will allow you to review them and if they are OK
rem replace <test>.json with <test>.json.new.
setlocal EnableDelayedExpansion
if "%DIFF%" == "" (
    where odd.exe /q
    if %errorlevel% equ 0 ( 
        set DIFF=odd.exe
    ) else (
        set DIFF=windiff.exe
    )
)
for %%f in (*.new) do (
    echo.
    %DIFF% %%~nf %%f
    set correct=
    set /p correct=Is %%f correct [ynq]? 
    echo !correct!
    if !correct! == y (
        echo Switching to new version
        move %%f %%~nf
    ) else (
        if !correct! == n (
            echo Keeping old version
        ) else (
            goto done
        )
    )
)
:done
endlocal
