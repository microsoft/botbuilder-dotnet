#
# Unlists package versions on npm feed lower than or equal to $versionToUnlist.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
#
param
( 
    [string]$versionToUnlist = "0.6.0",
    [string]$unlistOlderVersionsAlso = "false",
    [string[]]$packageNames = @( "adaptive-expressions","botbuilder","botbuilder-dialogs-adaptive-runtime-core" ),
    [string]$npmPersonalAccessToken, # Not currently used.
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

"versionToUnlist: " + $versionToUnlist;
"unlistOlderVersionsAlso: " + $unlistOlderVersionsAlso;
"unlistPackagesForReal: " + $unlistPackagesForReal;
" ";
"Package versions to unlist:"

foreach ($packageName in $packageNames) {
    $versionsToUnlist = $null;

    $versions = Get-Versions-From-Npm $packageName;
    $versionsArray = $versions -split " " | Where-Object {$_};

    if ($unlistOlderVersionsAlso -eq "true") {

        #Set index to $versionToUnlist
        $index = (0..($versionsArray.Count-1)) | where {$versionsArray[$_].StartsWith($versionToUnlist)};

        if ($index -ne $Null) {
            [string[]]$versionsToUnlist = $versionsArray | select -First ($index[-1] + 1);
        }
    } else {
        [string[]]$versionsToUnlist = $versionsArray.Where({$_ -eq $versionToUnlist});
    }

    if ($versionsToUnlist.Count -gt 0) {
        "-----------------------------------------";
        $packageName + ":";
        "-----------------------------------------";

    foreach ($version in $versionsToUnlist) {
        if ($unlistPackagesForReal -eq "true") {
            "Unlisting $version"
            "npm unpublish $packageName@$version --loglevel verbose";
            npm unpublish $packageName@$version --loglevel verbose;
            #"npm unpublish @microsoft/$packageName@$version --loglevel verbose";
            #npm unpublish @microsoft/$packageName@$version --loglevel verbose;
            } else {
                "    $version"
            }
        }
    }
}
"-----------------------------------------";
