erase /y nuget\*.*
pushd Microsoft.Bot.Builder.Classic
call createpackage.cmd
popd
pushd Microsoft.Bot.Builder.Classic.FormFlow.Json
call createpackage.cmd
popd
pushd Microsoft.Bot.Builder.Classic.History
call createpackage.cmd
popd
