@echo off
if not "%1" == "" goto usage
if not exist collect.cmd goto usage
setlocal 

echo *** Installing npm tools
call npm update luis-apis qnamaker botdispatch jspath-cli -g -dev

call collect

echo.
call luisimport ..\weather.json

echo.
call luisimport ..\homeautomation.json

:qna
echo.
echo *** Importing QnA Maker database faq.json
call qnaid faq
if "%qnaid%" neq "" goto dispatcher
call qnamaker create kb --in ..\faq.json
call qnaid faq
call qnamaker publish kb --kbId %qnaid%
goto done

:dispatcher
echo.
echo *** Building language dispatch model
rem for /f %%k in ('jspath ".authoringKey" .luisrc') do set luisKey=%%k
for /f %%k in ('jspath ".subscriptionKey" .qnamakerrc') do set qnaKey=%%k

echo .
echo *** Add LUIS weather to dispatch
call dispatch add -type luis -name weather

echo.
echo *** Add LUIS homeautomation to dispatch
call dispatch add -type luis -name homeautomation

echo.
echo *** Add QnA Maker faq KB to dispatch
call dispatch add -type qna -name faq -key %qnaKey% -id %qnaid%

echo.
echo *** Building language dispatcher
call dispatch create

:summary
summary.html

echo.
echo *** Successfully created services
goto done

:usage
echo This will setup services for this example if run in the scripts directory.
echo It will ask you for the required information and when it is done you will have:
echo 1) Two LUIS services: weather and homeautomation.
echo 2) A QnA Maker KB called 'faq'.
echo 3) A language dispatch LUIS model called 'dispatchSample'.

:done
