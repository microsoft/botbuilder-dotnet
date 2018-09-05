@echo off
setlocal EnableDelayedExpansion
for %%f in (*.new) do (
    echo.
    odd %%~nf %%f
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
