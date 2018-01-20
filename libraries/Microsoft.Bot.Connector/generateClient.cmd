call npm install replace@0.3.0

call autorest --input-file=.\Swagger\ConnectorAPI.json --csharp --namespace:Microsoft.Bot.Connector --output-folder=ConnectorAPI --add-credentials --override-client-name=ConnectorClient --use-datetimeoffset

cd ConnectorAPI
call ..\node_modules\.bin\replace "Microsoft.Bot.Connector.Models" "Microsoft.Bot.Connector" . -r --include="*.cs"
call ..\node_modules\.bin\replace "using Models;" ""  . -r --include="*.cs"
call ..\node_modules\.bin\replace "FromProperty" "From" . -r --include="*.cs"
call ..\node_modules\.bin\replace "fromProperty" "from" . -r --include="*.cs"
cd ..

pause