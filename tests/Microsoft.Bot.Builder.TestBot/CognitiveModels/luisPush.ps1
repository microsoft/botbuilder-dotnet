#Requires -Version 6

Param(
	[string] $luFile,
	[string] $luisAppId,
    [string] $luisAuthoringKey,
    [string] $luisSubscriptionKey = $luisAuthoringKey,
    [string] $luisVersion,
    [string] $defaultNamespace,
    [string] $logFile = $(Join-Path $PSScriptRoot "luisPush.log")
)

. $PSScriptRoot\luis_functions.ps1

# Reset log file
if (Test-Path $logFile) {
	Clear-Content $logFile -Force | Out-Null
}
else {
	New-Item -Path $logFile | Out-Null
}

# TODO validate mandatory parameters here

# Get active version if not passed. 
if (-not $luisVersion) {
    $luisVersion = GetActiveVersion -appId $luisAppId -authoringKey $luisAuthoringKey
}

$currentFolder = Split-Path $MyInvocation.MyCommand.Path
$luFileFull = Get-Item (Join-Path $currentFolder $luFile)
$luisJsonFile = Join-Path $currentFolder ($luFileFull.BaseName + ".json")

Write-Host "Pushing" $luFile "to version" $luisVersion

# Update, train and publish the version
UpdateLUIS -lu_file $luFileFull -appId $luisAppId -authoringKey $luisAuthoringKey -subscriptionKey $luisSubscriptionKey -version $luisVersion -log $logFile

# Update cs class for the luis file
Write-Host "Running luisgen to create" ($defaultNamespace + "." + $luFileFull.BaseName)
luisgen $luisJsonFile -cs ($defaultNamespace + "." + $luFileFull.BaseName)