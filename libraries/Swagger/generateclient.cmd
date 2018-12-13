call npm install

call npx autorest README.md --csharp --use=".\node_modules\@microsoft.azure\autorest.csharp"

pushd generated
call npx replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Schema" . -r --include="*.cs"
call npx replace "using Models;" "using Microsoft.Bot.Schema;"  . -r --include="*.cs"
call npx replace "FromProperty" "From" . -r --include="*.cs"
call npx replace "fromProperty" "from" . -r --include="*.cs"
popd

copy generated\Models\*.* ..\Microsoft.Bot.Schema
move ..\Microsoft.Bot.Schema\ErrorResponseException.cs ..\Microsoft.Bot.Connector
copy generated\*.* ..\Microsoft.Bot.Connector
rd /q /s generated

