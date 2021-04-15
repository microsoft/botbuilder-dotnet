<#
  .SYNOPSIS
  Runs ApiCompat tool against built libraries. Fails if there are breaking changes between the current implementation and the published contracts.

  .DESCRIPTION
  Takes the name of a project, a version to compare to (or fetches latest) and a version of apiCompat to use
  to match API definitions between provided version and current build for provided project name.

  .PARAMETER Path
  Specifies the path to project root folder.

  .PARAMETER Name
  Specifies the name for the specific project we are building.

  .PARAMETER Version
  Specifies the version of the library to compare the implementation with.

  .PARAMETER ApiCompatVersion
  Specifies the version of ApiCompat to use. You can see available versions here https://dev.azure.com/dnceng/public/_packaging?_a=package&feed=dotnet-eng&view=versions&package=Microsoft.DotNet.ApiCompat&protocolType=NuGet

  .PARAMETER DebugBuild
  Specifies to retrieve the Dll files from the 'Debug' build configuration folder. If the parameter is not specified, this will be the default configuration value.

  .PARAMETER ReleaseBuild
  Specifies to retrieve the Dll files from the 'Release' build configuration folder. If the parameter is not specified, 'Debug' will be the default configuration value.

  .OUTPUTS
  Debugging information is printed on console.
  Creates an incremental log file where all differences between APIs are stored. path: ./ApiCompat/ApiCompatResult.log

  .EXAMPLE
  PS> .\ExecuteApiCompat.ps1 -Path 'C:\Code\botbuilder-dotnet' -Name 'Microsoft.Bot.Builder' -Version 4.11.0 -ApiCompatVersion 6.0.0-beta.21179.2

  .EXAMPLE
  PS> .\ExecuteApiCompat.ps1 -Path 'C:\Code\botbuilder-dotnet' -Name 'Microsoft.Bot.Builder' -Version 4.11.0 -ApiCompatVersion 6.0.0-beta.21179.2 -DebugBuild

  .EXAMPLE
  PS> .\ExecuteApiCompat.ps1 -Path 'C:\Code\botbuilder-dotnet' -Name 'Microsoft.Bot.Builder' -ApiCompatVersion 6.0.0-beta.21179.2 -ReleaseBuild

#>

using namespace System.IO.Compression

[CmdletBinding(DefaultParameterSetName = 'DefaultConfiguration')]
param
( 
    [string]$Path,
    [Parameter(Mandatory=$True)]
    [string]$Name,
    [string]$Version,
    [Parameter(Mandatory=$True)]
    [string]$ApiCompatVersion,
    [Parameter(Mandatory=$True, ParameterSetName="DebugConfiguration")]
    [switch]$DebugBuild,
    [Parameter(Mandatory=$True, ParameterSetName="ReleaseConfiguration")]
    [switch]$ReleaseBuild
)

# Get path from param or use current
if (![string]::IsNullOrEmpty($Path)) {
    Set-Location -Path $Path
    $Path = $Path.TrimEnd('\')
} else {
    $Path = Get-Location
}

$BuildConfiguration = ""
if ($DebugBuild -eq $True) {
    $BuildConfiguration = "Debug"
} elseif ($ReleaseBuild -eq $True) {
    $BuildConfiguration = "Release"
} else {
    $BuildConfiguration = 'Debug'
}

try {
    nuget > $null
} catch {
    Write-Error "The term 'nuget' is not recognized. Check if the path was included as an environment variable. If not, download nuget from here: https://www.nuget.org/downloads"
    exit 3
}

$ApiCompatPath = "$Path\ApiCompat"

$ZipFile = "ApiCompat.zip"
$ZipPath = "$ApiCompatPath\$ZipFile"

$InstallResult = $false

$ApiCompatDownloadRequestUri = "https://pkgs.dev.azure.com/dnceng/public/_apis/packaging/feeds/dotnet-eng/nuget/packages/Microsoft.DotNet.ApiCompat/versions/$ApiCompatVersion/content"
# ApiCompat versions can be seen here -> https://dev.azure.com/dnceng/public/_packaging?_a=package&feed=dotnet-eng&view=versions&package=Microsoft.DotNet.ApiCompat&protocolType=NuGet

$DownloadLatestPackageVersion = {    
    # Get latest version suffix
    $DllData = nuget search $DllName -PreRelease
    $LatestVersion = [regex]::match($DllData,"(?<=$DllName \| ).*?(?=\s)").Value
    $script:Version = $LatestVersion

    Write-Host ">> Attempting to download latest version $LatestVersion" -ForegroundColor cyan
    
    # Store command into a variable to handle error output from nuget
    $NugetInstallCommand = 'nuget install $DllName -OutputDirectory "$Path\ApiCompat\Contracts" -Version $LatestVersion'
    
    # Run command and store outputs into variables
    Invoke-Expression $NugetInstallCommand -ErrorVariable InstallCommandError -OutVariable InstallCommandOutput 2>&1 >$null
    
    $script:InstallResult = ($InstallCommandOutput -match 'Added package' -or $InstallCommandOutput -match 'already installed')
    if ($InstallResult) {
        Write-Host ">> Successfully downloaded latest version: $LatestVersion" -ForegroundColor green        
    }
}

$DownloadFixedPackageVersions = {
    # Remove version sufix if any
    $script:Version = $Version -replace '-local'
    
    Write-Host ">> Attempting to download GA specific version = $Version" -ForegroundColor cyan
    
    # Install corresponding nuget package to "Contracts" folder    
    # Store command into a variable to handle error output from nuget
    $NugetInstallCommand = 'nuget install $DllName -Version $Version -OutputDirectory "$Path\ApiCompat\Contracts" -Verbosity detailed'
    
    # Run command and store outputs into variables
    Invoke-Expression $NugetInstallCommand -ErrorVariable InstallCommandError -OutVariable InstallCommandOutput 2>&1 >$null
    
    # Check package existance by searching on the output strings that would match only if the package is installed
    $script:InstallResult = ($InstallCommandOutput -match 'Added package' -or $InstallCommandOutput -match 'already installed')
    
    # If GA version doesn't exist, attempt to download specific preview version
    if(!$InstallResult) {
        Write-Host ">> Failed download GA specific version: $Version. Downloading specific preview version..." -ForegroundColor red
        Write-Host ">> Attempting to download specific preview version = $Version-preview" -ForegroundColor cyan
        
        # Store command into a variable to handle error output from nuget
        $NugetInstallCommand = 'nuget install $DllName -Version "$Version-preview" -OutputDirectory "$Path\ApiCompat\Contracts" -Verbosity detailed'
        
        # Run command and store outputs into variables
        Invoke-Expression $NugetInstallCommand -ErrorVariable InstallCommandError -OutVariable InstallCommandOutput 2>&1 >$null
        $script:InstallResult = ($InstallCommandOutput -match 'Added package' -or $InstallCommandOutput -match 'already installed')
        
        # If specific preview version doesn't exist, attempt to download latest version (including preview)
        if ($InstallResult) {
            Write-Host ">> Successfully downloaded preview version: $Version-preview" -ForegroundColor green
            
            # If previous install is successful, we append -preview to version.
            $script:Version = "$Version-preview"
        } else {
            # If specific versions failed, download latest
            Write-Host ">> Failed downloading specific preview version: $Version-preview. Downloading latest available version..." -ForegroundColor red
            &$DownloadLatestPackageVersion
        }
    } else {
        Write-Host ">> Success" -ForegroundColor green
    }
}

$DownloadApiCompat = {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    Set-Location $ApiCompatPath
    
    # Get Zipfile for ApiCompat
    $DestinationPath= "$Path\ApiCompat"
    
    # If file doesn't exist, download it
    if (!(Test-Path "$ApiCompatPath\ApiCompat.zip" -PathType Leaf)) {
        try { 
            # Download
            Write-Host "Downloading ApiCompat version $ApiCompatVersion as zip"
            
            try {
                (New-Object System.Net.WebClient).DownloadFile($ApiCompatDownloadRequestUri, $ZipPath)
            } catch { 
                Write-Error "Download attempt failed"
                Write-Error $_
                exit 1 
            }
            
            $Zip = [ZipFile]::OpenRead($ZipPath)
            # Extract files
            try {
                # Get parent folder from latest apiCompat framework version
                $ZipApiCompatParentFolder = $zip.Entries | Where-Object {$_.Name -like 'Microsoft.DotNet.ApiCompat.exe'} | Select-Object -Last 1
                $ZipApiCompatParentFolder = split-path -parent $ZipApiCompatParentFolder 
                $ZipApiCompatParentFolder = split-path -leaf $ZipApiCompatParentFolder

                # Extract all files in latest framework version folder
                $Zip.Entries.Where{ $_.FullName -match "$ZipApiCompatParentFolder.*[^/]$" }.ForEach{
                    $NewFile = [IO.FileInfo]($DestinationPath,$_.FullName -join "/")
                    $NewFile.Directory.Create()
                    [ZipFileExtensions]::ExtractToFile($_, $NewFile)
                }
            } catch {
                Write-Error "Failed to extract files from $ZipPath"
                exit 6
            } finally {
                $Zip.Dispose()
            }

            Write-Host "$ZipFile successfully downloaded and extracted" -ForegroundColor green
        } finally {
            # Remove downloaded zip file.
            Remove-Item $ZipFile
        }
    }

    Set-Location $Path
}

$WriteToLog = {
    $ResultMessage = " '$DllName': Binary Compatibility compared against version: $Version => $ApiCompatResult"
    Write-Host $ResultMessage -ForegroundColor green
    
    # Create a Mutex for all process to be able to share same log file
    $mutexName = "LogFileMutex" #'A unique name shared/used across all processes that need to write to the log file.'
    $mutex = New-Object 'Threading.Mutex' $false, $mutexName
    
    #Grab the mutex. Will block until this process has it.
    $mutex.WaitOne() | Out-Null;
    
    try {
        # Now it is safe to write to log file
        Add-Content $OutputDirectory $ResultMessage
    } finally {
        $mutex.ReleaseMutex()
    }
}

# Get specific dll file from built solution
$Dll = Get-ChildItem "$Path\**\$Name\bin\$BuildConfiguration\**\**" -Filter "$Name.dll" | ForEach-Object { $_.FullName } | Select-Object -First 1
if ([string]::IsNullOrEmpty($Dll)){
    $Dll = Get-ChildItem "$Path\**\**\$Name\bin\$BuildConfiguration\**\**" -Filter "$Name.dll" | ForEach-Object { $_.FullName } | Select-Object -First 1

    if ([string]::IsNullOrEmpty($Dll)) {
        Write-Error ">> Local dll was not found. Try building your project or solution."
        exit 3
    }
}

$ImplementationPath = split-path -parent $Dll

$DllName = [IO.Path]::GetFileNameWithoutExtension($Dll)

if ([string]::IsNullOrEmpty($Version)) {
    &$DownloadLatestPackageVersion
} else {
    &$DownloadFixedPackageVersions
}

# No reason to continue if package could not be installed
if (!$InstallResult) {
    Write-Error "Failed to download package $DllName with version $Version`n"
    exit 2
}

# Get specific dll file from nuget package
$PackageName = "$DllName.$Version"

$ContractPath = Get-ChildItem "$ApiCompatPath\Contracts\$PackageName\lib\**\" | Select-Object -First 1

# Download ApiCompat
# Create a Mutex to prevent race conditions while downloading apiCompat.zip
# Important notice: All threads should wait for download and extraction to finish before moving past this point.
$mutexName = "DownloadApiCompatMutex" # A unique name shared/used across all processes.
$mutex = New-Object 'Threading.Mutex' $false, $mutexName

# Grab the mutex. Will block until this process has it.
# All processes will wait for the mutex to be freed to prevent cases were 'tools' folder exists but has not finished extracting.
# If download and extraction of ApiCompat.zip is done, the other processes will exit by the if condition and wait a minimum amount of time.
$mutex.WaitOne() | Out-Null;

try {
    # TODO: We should add a check that compares apicompat parameter version and installed version (\ApiCompat\tools\netcoreapp3.1\Microsoft.DotNet.ApiCompat.runtimeconfig.json) and re-download if they differ.
    # Clean possible orphan files from aborted previous run.
    if (Test-Path $ZipPath -PathType Leaf) {
        Remove-Item $ZipPath

        if (Test-Path "$ApiCompatPath\tools") {
            Remove-Item -Recurse -Force "$ApiCompatPath\tools"
        }
    }

    if (!(Test-Path "$ApiCompatPath\tools")) {
        &$DownloadApiCompat
    }
} finally {
    $mutex.ReleaseMutex()
}

# Get ApiCompat executable from downloaded folder
$ApiCompatExe = Get-ChildItem "$ApiCompatPath\tools\**\Microsoft.DotNet.ApiCompat.exe" | Select-Object -Last 1

# Run ApiCompat
$ApiCompatCommand = "& `"$ApiCompatExe`" $ContractPath --impl-dirs $ImplementationPath --resolve-fx"
$ApiCompatResult = (Invoke-Expression $ApiCompatCommand)  -replace " in the contract.", " in the contract.`n" -replace "${Name}:", "${Name}:`n"
$OutputDirectory = if (Test-Path "$ApiCompatPath\ApiCompatResult.log") { "$ApiCompatPath\ApiCompatResult.log" } else { New-item -Name "ApiCompatResult.log" -Type "file" -Path $ApiCompatPath }

Write-Host ">> Saving ApiCompat output to $OutputDirectory" -ForegroundColor cyan

# Add result to txt file for better accessibility
&$WriteToLog

# Check result from ApiCompat comparison
if ($ApiCompatResult -notlike "*Total Issues: 0*") {
    Write-Error ">> ApiCompat failed matching implementation and contract."
    exit 4 # ApiCompat failed
}
