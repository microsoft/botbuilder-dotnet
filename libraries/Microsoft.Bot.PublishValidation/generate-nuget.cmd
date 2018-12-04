rmdir .\obj /s /q
rmdir .\bin /s /q
dotnet build -c Release
nuget pack -Properties Configuration=Release -OutputDirectory ..\..\outputpackages