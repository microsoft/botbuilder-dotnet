
@echo off
setlocal

set region=%1
if "%region%" EQU "" set region=westus

set key=%2
if "%key%" NEQ "" set key=--authoringKey %key%
 
echo Building Models
 
call bf luis:build --luConfig luconfig.json --region=%region% %key% --out generated --log
 
goto done
 
:help
echo build.cmd [region] [LUISAuthoringKey]
echo Region defaults to westus.
echo Set LUISAuthoringKey default with bf config:set:luis --authoringKey=<yourKey>
:done
