copy Microsoft.Bot.Builder.Classic.nuspec temp.nuspec
..\..\..\nuget\rep -find:$version$ -replace:%version% temp.nuspec
NuGet.exe pack -symbols temp.nuspec
erase temp.nuspec
