#Requires -Version 6

Param(
	[string] $luisAppId,
    [string] $luisAuthoringKey,
    [string] $luFile,
    [string] $luisVersion,
    [string] $logFile = $(Join-Path $PSScriptRoot "luisPull.log")
)

. $PSScriptRoot\luis_functions.ps1

# TODO validate mandatory parameters here

if ($luFile) {
    $currentFolder = Split-Path $MyInvocation.MyCommand.Path
    $luFileFull = Join-Path $currentFolder $luFile
}
else {
    $luFileFull = ""
}

ExportLuFile -appId $luisAppId -authoringKey $luisAuthoringKey -luFile $luFileFull