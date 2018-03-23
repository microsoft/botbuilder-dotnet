copy Microsoft.Bot.Builder.Classic.nuspec temp.nuspec
..\..\..\nuget\rep -find:$version$ -replace:%version% temp.nuspec
..\..\..\packages\NuGet.CommandLine.4.1.0\tools\NuGet.exe pack -symbols temp.nuspec
erase temp.nuspec
