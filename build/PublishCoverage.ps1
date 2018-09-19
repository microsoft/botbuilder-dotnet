Param(
    [string]$coverallsToken
)

Write-Host Install tools
$coverageAnalyzer = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.exe"
dotnet tool install coveralls.net --version 1.0.0 --tool-path tools
$coverageUploader = ".\tools\csmacnz.Coveralls.exe"

Write-Host Analyze coverage
$coverageFiles = Get-ChildItem -Path "$env:Build_SourcesDirectory\CodeCoverage" -Include "*.coverage" -Recurse | Select -Exp FullName
Write-Host with files: $coverageFiles
."$coverageAnalyzer" analyze /output:"$env:Build_SourcesDirectory\CodeCoverage\coverage.coveragexml" $coverageFiles

Write-Host Upload coverage
$branchName = $env:Build_SourceBranch -replace "refs/heads/", ""
Write-Host with args: --repoToken $coverallsToken --commitId $env:Build_SourceVersion --commitBranch $branchName --commitAuthor $env:Build_RequestedFor --commitEmail $env:Build_RequestedForEmail --commitMessage $env:Build_SourceVersionMessage --jobId $env:Build_BuildId --useRelativePaths --basePath "$env:Build_SourcesDirectory"
."$coverageUploader" --dynamiccodecoverage -i "$env:Build_SourcesDirectory\CodeCoverage\coverage.coveragexml" --repoToken $coverallsToken --commitId $env:Build_SourceVersion --commitBranch "$branchName" --commitAuthor "$env:Build_RequestedFor" --commitEmail "$env:Build_RequestedForEmail" --commitMessage "$env:Build_SourceVersionMessage" --jobId $env:Build_BuildId --useRelativePaths --basePath "$env:Build_SourcesDirectory" -o "$env:Build_SourcesDirectory\CodeCoverage\coverage.json"
