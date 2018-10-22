call npm install

call .\node_modules\.bin\autorest README.md --csharp

pushd generated
call ..\node_modules\.bin\replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Schema" . -r --include="*.cs"
call ..\node_modules\.bin\replace "using Models;" "using Microsoft.Bot.Schema;"  . -r --include="*.cs"
call ..\node_modules\.bin\replace "FromProperty" "From" . -r --include="*.cs"
call ..\node_modules\.bin\replace "fromProperty" "from" . -r --include="*.cs"
popd

copy generated\Models\*.* ..\Microsoft.Bot.Schema
move ..\Microsoft.Bot.Schema\ErrorResponseException.cs ..\Microsoft.Bot.Connector
copy generated\*.* ..\Microsoft.Bot.Connector
rd /q /s generated

