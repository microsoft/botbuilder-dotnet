copy Microsoft.Bot.Builder.Classic.nuspec temp.nuspec
..\..\..\nuget\rep -find:$version$ -replace:%1 temp.nuspec
NuGet.exe pack -symbols temp.nuspec -outputdirectory bin\release
erase temp.nuspec
