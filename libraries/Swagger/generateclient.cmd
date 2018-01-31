call npm install replace@0.3.0

call autorest README.md --csharp

cd generated
call ..\node_modules\.bin\replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Schema" . -r --include="*.cs"
call ..\node_modules\.bin\replace "using Models;" "using Microsoft.Bot.Schema;"  . -r --include="*.cs"
call ..\node_modules\.bin\replace "FromProperty" "From" . -r --include="*.cs"
call ..\node_modules\.bin\replace "fromProperty" "from" . -r --include="*.cs"
cd ..

copy generated\Models\*.* ..\Microsoft.Bot.Schema
move ..\Microsoft.Bot.Schema\ErrorResponseException.cs ..\Microsoft.Bot.Connector
copy generated\*.* ..\Microsoft.Bot.Connector
rd /q /s generated

