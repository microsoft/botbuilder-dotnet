Param(
    [string]$coverallsToken
)

Write-Host Install tools
$coverageAnalyzer = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe"
dotnet tool install coveralls.net --version 1.0.0 --tool-path tools
$coverageUploader = ".\tools\csmacnz.Coveralls.exe"

Write-Host Analyze coverage
$coverageFiles = Get-ChildItem -Include "*.coverage" -Recurse | Select -Exp FullName
Write-Host with files: $coverageFiles
."$coverageAnalyzer" analyze /output:coverage.coveragexml $coverageFiles

Write-Host Upload coverage
Write-Host with args: --repoToken $coverallsToken --commitId $env:Build_SourceVersion --commitBranch $env:Build_SourceBranch --commitAuthor $env:Build_RequestedFor --commitEmail $env:Build_RequestedForEmail --commitMessage $env:Build_SourceVersionMessage --jobId $env:Build_BuildId --useRelativePaths --basePath "$env:Build_SourcesDirectory"
."$coverageUploader" --dynamiccodecoverage -i "coverage.coveragexml" --repoToken $coverallsToken --commitId $env:Build_SourceVersion --commitBranch "$env:Build_SourceBranch" --commitAuthor "$env:Build_RequestedFor" --commitEmail "$env:Build_RequestedForEmail" --commitMessage "$env:Build_SourceVersionMessage" --jobId $env:Build_BuildId --useRelativePaths --basePath "$env:Build_SourcesDirectory" -o coverage.json

Write-Host Clean
Remove-Item coverage.coveragexml -ErrorAction Ignore
Remove-Item coverage.json -ErrorAction Ignore
