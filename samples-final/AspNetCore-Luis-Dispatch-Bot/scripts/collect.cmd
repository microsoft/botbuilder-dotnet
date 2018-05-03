@echo off
rem Collect initialization information
if exist .luisrc goto qna
echo.
echo *** Collecting LUIS information
call luis --init

:qna
if exist .qnamakerrc goto dispatch
echo.
echo *** Collecting QnA Maker information
call qnamaker --init

:dispatch
if exist dispatchSample.dispatch goto done
echo.
echo *** Collecting dispatch information
call dispatch init -name dispatchSample

:done
