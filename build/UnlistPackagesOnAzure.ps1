#
# Unlists NuGet package versions on Azure ConversationalAI feed lower than or equal to $versionToUnlist.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
# See: https://stackoverflow.com/questions/34958908/where-can-i-find-documentation-for-the-nuget-v3-api
#
param
( 
    [string]$versionToUnlist = "4.11.2",
    [string]$unlistOlderVersionsAlso = "false",
    [string[]]$packageNames = @( "AdaptiveExpressions","Microsoft.Bot.Builder","Microsoft.Bot.Builder.Integration.AspNet.Core" ),
    [string]$adoPersonalAccessToken,
    [string]$unlistPackagesForReal = "false"
)

$RegistryUrlSource = "https://pkgs.dev.azure.com/ConversationalAI/BotFramework/_packaging/SDK/nuget/v3/index.json";

Function Get-Versions-From-Azure 
{
    param ( [string]$packageName );

    $result = nuget list $packageName -Source "$RegistryUrlSource" -PreRelease -AllVersions | Where-Object { $_ -like "$packageName *" };
    $versions = $result | % { $_.Split(" ")[-1] };

    return $versions
}

"versionToUnlist: " + $versionToUnlist;
"unlistOlderVersionsAlso: " + $unlistOlderVersionsAlso;
"unlistPackagesForReal: " + $unlistPackagesForReal;
" ";
"Package versions to unlist:"

foreach ($packageName in $packageNames) {
    $versionsToUnlist = $Null;
    $index = -1;

    $sortedVersions = Get-Versions-From-Azure $packageName;

    [array]::Reverse($sortedVersions);

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
                "    nuget delete $packageName $version -Source $RegistryUrlSource -apikey $adoPersonalAccessToken -NonInteractive";
                nuget delete $packageName $version -Source $RegistryUrlSource -apikey $adoPersonalAccessToken -NonInteractive;
            } else {
                "    $version";
            }
        }
    }
}
"-----------------------------------------";
