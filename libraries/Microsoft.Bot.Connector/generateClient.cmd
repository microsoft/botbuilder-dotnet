rd /s /q Client
call npm install replace@0.3.0

..\..\packages\autorest.0.16.0\tools\AutoRest -namespace Microsoft.Bot.Connector -input swagger\ConnectorApi.json -outputDirectory ConnectorAPI -AddCredentials -ClientName ConnectorClient
..\..\packages\autorest.0.16.0\tools\AutoRest -namespace Microsoft.Bot.Connector -input swagger\StateAPI.json -outputDirectory StateApi -AddCredentials -ClientName StateClient


rd /s /q Payments\Models\
..\..\packages\autorest.0.16.0\tools\AutoRest -namespace Microsoft.Bot.Connector.Payments -input swagger\Connector-Payments.json -outputDirectory Payments\Models -AddCredentials -ClientName PaymentsClient
erase Payments\Models\*.cs
move Payments\Models\Models\* Payments\Models
rd /s /q Payments\Models\Models

cd ConnectorAPI
call ..\node_modules\.bin\replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Connector" . -r --include="*.cs"
call ..\node_modules\.bin\replace "using Models;" ""  . -r --include="*.cs"
call ..\node_modules\.bin\replace "FromProperty" "From" . -r --include="*.cs"
call ..\node_modules\.bin\replace "fromProperty" "from" . -r --include="*.cs"
cd ..
cd StateAPI
call ..\node_modules\.bin\replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Connector" . -r --include="*.cs"
call ..\node_modules\.bin\replace "using Models;" "" . -r --include="*.cs"
cd ..
cd Payments\Models\
call ..\..\node_modules\.bin\replace "namespace Microsoft.Bot.Connector.Payments.Models" "namespace Microsoft.Bot.Connector.Payments" . -r --include="*.cs"
call ..\..\node_modules\.bin\replace "using Models;" "" . -r --include="*.cs"
call ..\..\node_modules\.bin\replace "using Microsoft.Rest;" "" . -r --include="*.cs"
call ..\..\node_modules\.bin\replace "using Microsoft.Rest.Serialization;" "" . -r --include="*.cs"
cd ..\..
@echo !!!!! Please review ConversationsExtensions.cs and BotStateExtensions.cs for custom throw code
pause
