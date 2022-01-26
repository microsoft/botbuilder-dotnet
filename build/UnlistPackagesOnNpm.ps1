#
# Unlists package versions on npm feed lower than or equal to $versionToUnlist.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
#
param
( 
    [string]$versionToUnlist = "0.6.2",
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
    $versionsToUnlist = $Null;
    $index = -1;

    $versions = Get-Versions-From-Npm $packageName;
    $sortedVersions = $versions -split " " | Where-Object {$_};

    if ($unlistOlderVersionsAlso -eq "true") {
        for ([int]$i = 0; $i -lt $sortedVersions.Count; $i++)
        {
            if ($sortedVersions[$i] -ge $versionToUnlist) {
                $index = $i;
                if ($sortedVersions[$i] -gt $versionToUnlist) { $index--; }
                break;
            }
        }

        if ($index -ne $Null -and $index -ge 0) {
            [string[]]$versionsToUnlist = $sortedVersions | select -First ($index + 1);
        }
    } else {
        [string[]]$versionsToUnlist = $sortedVersions.Where({$_ -eq $versionToUnlist});
    }

    if ($versionsToUnlist.Count -gt 0) {
        "-----------------------------------------";
        $packageName + ":";
        "-----------------------------------------";

        foreach ($version in $versionsToUnlist) {
            if ($unlistPackagesForReal -eq "true") {
                "    Unlisting $version";
                "    npm unpublish $packageName@$version --loglevel verbose";
                npm unpublish $packageName@$version --loglevel verbose;
                #"npm unpublish @microsoft/$packageName@$version --loglevel verbose";
                #npm unpublish @microsoft/$packageName@$version --loglevel verbose;
            } else {
                "    $version";
            }
        }
    }
}
"-----------------------------------------";
