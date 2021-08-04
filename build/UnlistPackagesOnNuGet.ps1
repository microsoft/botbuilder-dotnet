#
# Unlists NuGet package versions lower than or equal to $versionToUnlist.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
#
param
( 
    [string]$versionToUnlist = "1.0.2",
    [string[]]$packageNames = { "AdaptiveExpressions","Microsoft.Bot.Builder","Microsoft.Bot.Builder.Integration.AspNet.Core" },
    [string]$nuGetPersonalAccessToken,
    [string]$unlistPackagesForReal = "false"
)

$feedApiUrl = "https://api.nuget.org/v3/index.json";

Function Get-Versions-From-Nuget
{
    param ( [string]$packageName );

    $packageBaseAddress = "https://api.nuget.org/v3-flatcontainer/";
    $versionsUri = $packageBaseAddress + $packageName + "/index.json";

    $response2 = Invoke-RestMethod -Uri $versionsUri

    $versions = $response2.versions;

    return $versions;
}

"deletePackagesForReal: " + $unlistPackagesForReal;
"Target version: " + $versionToUnlist;
" ";
"Package versions to unlist:"

foreach ($packageName in $packageNames) {
    "========================";
    $packageName;
    "========================";

    $sortedVersions = Get-Versions-From-Nuget $packageName;

    #Get versions equal to or older than $versionToUnlist
    $index = (0..($sortedVersions.Count-1)) | where {$sortedVersions[$_].StartsWith($versionToUnlist)};

    if ($index -ne $Null) {
        $versionsToUnlist = $sortedVersions | select -First ($index[-1] + 1);
        $versionsToUnlist;
    } else {
        $versionsToUnlist = $null;
        "[none]";
    }

    # Do the unlisting
    foreach ($version in $versionsToUnlist) {
        if ($unlistPackagesForReal -eq "true") {
            "Unlisting $version"
            "nuget delete $packageName $version -Source $feedApiUrl -apikey $nuGetPersonalAccessToken -NonInteractive"
            nuget delete $packageName $version -Source $feedApiUrl -apikey $nuGetPersonalAccessToken -NonInteractive
        } else {
            "What-if: Unlisting $version"
            "nuget delete $packageName $version -Source $feedApiUrl -apikey $nuGetPersonalAccessToken -NonInteractive"
        }
    }
}