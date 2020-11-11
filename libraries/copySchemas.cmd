@echo off
echo Copying schemas from C# to node
set dest=%1
if "%dest%" EQU "" set dest=..\..\botbuilder-js\libraries

echo Copying to %dest%
echo No JS equivalent of teams
xcopy /s /y Microsoft.Bot.Builder.AI.Luis\Schemas\* %dest%\botbuilder-ai\schemas\
xcopy /s /y Microsoft.Bot.Builder.AI.QnA\Schemas\* %dest%\botbuilder-ai\schemas\
xcopy /s /y Microsoft.Bot.Builder.AI.Orchestrator\Schemas\* %dest%\botbuilder-ai-orchestrator\schemas\
xcopy /s /y Microsoft.Bot.Builder.Dialogs.Adaptive\Schemas\* %dest%\botbuilder-dialogs-adaptive\schemas\
xcopy /s /y Microsoft.Bot.Builder.Dialogs.Adaptive.Testing\Schemas\* %dest%\botbuilder-dialogs-adaptive-testing\schemas\
xcopy /s /y Microsoft.Bot.Builder.Dialogs.Declarative\Schemas\* %dest%\botbuilder-dialogs-declarative\schemas\
