#
# Unlists NuGet package versions on Azure ConversationalAI feed  lower than or equal to $versionToUnlist.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
# See: https://stackoverflow.com/questions/34958908/where-can-i-find-documentation-for-the-nuget-v3-api
#
param
( 
    [string]$versionToUnlist = "4.11.0",
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

"unlistPackagesForReal: " + $unlistPackagesForReal;
"Target version: " + $versionToUnlist;
" ";
"Package versions to unlist:"

foreach ($packageName in $packageNames) {
    "========================";
    $packageName;
    "========================";

    $sortedVersions = Get-Versions-From-Azure $packageName;

    [array]::Reverse($sortedVersions);

    #Get versions equal to or older than $versionToUnlist
    $index = (0..($sortedVersions.Count-1)) | where {$sortedVersions[$_].StartsWith($versionToUnlist)};

    if ($index -ne $Null) {
        $versionsToUnlist = $sortedVersions | select -First ($index[-1] + 1);
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
            "nuget delete $packageName $version -Source $RegistryUrlSource -apikey $adoPersonalAccessToken -NonInteractive"
            nuget delete $packageName $version -Source $RegistryUrlSource -apikey $adoPersonalAccessToken -NonInteractive
        } else {
            "What-if: Unlisting $version"
            "nuget delete $packageName $version -Source $RegistryUrlSource -apikey $adoPersonalAccessToken -NonInteractive"
        }
    }
}