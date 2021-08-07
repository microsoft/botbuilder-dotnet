#
# Unlists package versions on npm feed lower than or equal to $versionToUnlist.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
#
param
( 
    [string]$versionToUnlist = "0.6.0",
    [string[]]$packageNames = @( "adaptive-expressions","botbuilder","botbuilder-dialogs-adaptive-runtime-core" ),
    [string]$adoPersonalAccessToken,
    [string]$unlistPackagesForReal = "false"
)

$RegistryUrlSource = "https://pkgs.dev.azure.com/ConversationalAI/BotFramework/_packaging/SDK/npm/registry/";

Function Get-Versions-From-Npm 
{
    param ( [string]$packageName );

    $result = npm view $packageName versions
    $versions = $result | ConvertFrom-Json;

    return $versions;
}

"unlistPackagesForReal: " + $unlistPackagesForReal;
"Target version: " + $versionToUnlist;
" ";
"Package versions to unlist:"

foreach ($packageName in $packageNames) {
    "========================";
    $packageName;
    "========================";

    $versions = Get-Versions-From-Npm $packageName;
    $versionsArray = $versions -split " " | Where-Object {$_};

    #Get versions equal to or older than $versionToUnlist
    $index = (0..($versionsArray.Count-1)) | where {$versionsArray[$_].StartsWith($versionToUnlist)};

    if ($index -ne $Null) {
        $versionsToUnlist = $versionsArray | select -First ($index[-1] + 1);
        $versionsToUnlist;
    } else {
        $versionsToUnlist = $null;
        "[none]";
    }
    "------------------------";

    # Do the unlisting
    foreach ($version in $versionsToUnlist) {
        if ($unlistPackagesForReal -eq "true") {
            "Unlisting $version"
            "npm unpublish @microsoft/$packageName@$version --loglevel verbose";
            npm unpublish @microsoft/$packageName@$version --loglevel verbose;
        } else {
            "What-if: Unlisting $version"
            "npm unpublish @microsoft/$packageName@$version --dry-run --loglevel verbose";
            npm unpublish @microsoft/$packageName@$version --dry-run --loglevel verbose;
        }
    }
}