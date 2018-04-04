if "%1" == "" GOTO END
rep -find:"1.0.0.0" -replace:"%1" ..\libraries\Microsoft.Bot.Builder.Classic\Microsoft.Bot.Builder.Classic\Properties\AssemblyInfo.cs
:END