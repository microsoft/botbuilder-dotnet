erase /y nuget\*.*
pushd Microsoft.Bot.Builder
call createpackage.cmd
popd
pushd Microsoft.Bot.Builder.FormFlow.Json
call createpackage.cmd
popd
pushd Microsoft.Bot.Builder.Calling
call createpackage.cmd
popd
pushd Microsoft.Bot.Builder.History
call createpackage.cmd
popd
pushd Microsoft.Bot.Connector.NetFramework
call createpackage.cmd
popd
pushd Microsoft.Bot.Connector.AspNetCore
call createpackage.cmd
popd
pushd Microsoft.Bot.Connector.AspNetCore2
call createpackage.cmd
popd
