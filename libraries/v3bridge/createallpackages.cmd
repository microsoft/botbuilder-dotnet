erase /y nuget\*.*
pushd Microsoft.Bot.Builder.V3Bridge
call createpackage.cmd
popd
pushd Microsoft.Bot.Builder.V3Bridge.FormFlow.Json
call createpackage.cmd
popd
pushd Microsoft.Bot.Builder.V3Bridge.History
call createpackage.cmd
popd
