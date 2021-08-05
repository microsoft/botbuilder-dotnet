#
# Unlists NuGet package versions on MyGet.org lower than or equal to $versionToUnlist.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
#
param
( 
    [string]$versionToUnlist = "4.7.2-",
    [string[]]$packageNames = @( "AdaptiveExpressions","Microsoft.Bot.Builder","Microsoft.Bot.Builder.Integration.AspNet.Core" ),
    [string]$myGetFeedName = "botbuilder-v4-dotnet-daily",
    [string]$myGetPersonalAccessToken,
    [string]$unlistPackagesForReal = "false"
)

$feedStateUrl = "https://botbuilder.myget.org/F/$myGetFeedName/auth/$myGetPersonalAccessToken/api/v2/feed-state";
$feedApiUrl = "https://botbuilder.myget.org/F/$myGetFeedName/api/v3/index.json";

Function Sort-Versions
{
    param ( [string]$versions );

    New-Item -Path .\xxx.csv -ItemType "file" -Value $versions -Force | Out-Null;

    $Header = 'Major', 'Minor', 'Build', 'Revision', 'p5', 'p6';

    $P = Import-Csv -Path .\xxx.csv -Delimiter . -Header $Header;
    $P | % { $_.Major = [int]$_.Major };
    $P | % { $_.Minor = [int]$_.Minor };
    $P = $P | Sort -Property Major,Minor,Build,Revision;
    #$P | Format-Table;

    $Q = $P | % {  ($_.PSObject.Properties | % { $_.Value }) -join '.'};

    return $Q.TrimEnd(".");
}

$result = Invoke-RestMethod -Uri $feedStateUrl -Method Get -ContentType "application/json";

"unlistPackagesForReal: " + $unlistPackagesForReal;
"Target version: " + $versionToUnlist;
" ";
"Package versions to unlist:"

foreach ($packageName in $packageNames) {
    "========================";
    $packageName;
    "========================";

    $package = $result.packages | Where-Object {$_.id -eq $packageName};

    #$package.versions | Select -Last 30;

    [string]$unsortedVersions = $package.versions | %{ $_ + "`r`n" };

    $sortedVersions = Sort-Versions $unsortedVersions;

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
            "nuget delete $packageName $version -Source $feedApiUrl -apikey $myGetPersonalAccessToken -NonInteractive"
            nuget delete $packageName $version -Source $feedApiUrl -apikey $myGetPersonalAccessToken -NonInteractive
        } else {
            "What-if: Unlisting $version"
            "nuget delete $packageName $version -Source $feedApiUrl -apikey $myGetPersonalAccessToken -NonInteractive"
        }
    }
}