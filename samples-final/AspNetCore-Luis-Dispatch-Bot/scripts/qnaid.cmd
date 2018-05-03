@echo off
if "%1" == "" goto usage
set qnaid=
call qnamaker list kbs  > %temp%\qnakbs.json
if %ERRORLEVEL% EQU 0 for /f %%i in ('jspath ".knowledgebases{.name === \"%1\"}[0].id" %temp%\qnakbs.json') do set qnaid=%%i
if not "%qnaid%" == "" echo %qnaid%
goto done

:usage
echo "qnaid QnAKBName"
echo "Reports the QnA Maker knowledge base id and sets the environment variable qnaid to the id."

:done
