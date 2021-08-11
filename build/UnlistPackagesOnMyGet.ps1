#
# Unlists NuGet packages on MyGet.org with the specified version number. Option to unlist all older versions as well.
# Run this first with $unlistPackagesForReal = false (default) to verify what versions will be affected.
#
param
( 
    [string]$versionToUnlist = "4.7.0",
    [string]$unlistOlderVersionsAlso = "false",
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

"versionToUnlist: " + $versionToUnlist;
"unlistOlderVersionsAlso: " + $unlistOlderVersionsAlso;
"unlistPackagesForReal: " + $unlistPackagesForReal;
" ";
"Package versions to unlist:"

$result = Invoke-RestMethod -Uri $feedStateUrl -Method Get -ContentType "application/json";

foreach ($packageName in $packageNames) {
    $versionsToUnlist = $Null;
    $index = $Null;

    $packageInfo = $result.packages | Where-Object {$_.id -eq $packageName};

    if ($unlistOlderVersionsAlso -eq "true") {
        [string]$unsortedVersions = $packageInfo.versions | %{ $_ + "`r`n" };

        $sortedVersions = Sort-Versions $unsortedVersions;

        #Set index to $versionToUnlist
        #$index = (0..($sortedVersions.Count-1)) | where {$sortedVersions[$_].StartsWith($versionToUnlist)};
        $index = (($sortedVersions.Count-1)..0) | where {$sortedVersions[$_] -le $versionToUnlist};

        if ($index -ne $Null) {
            [string[]]$versionsToUnlist = $sortedVersions | select -First ($index[0] + 1);
        }
    } else {
        [string[]]$versionsToUnlist = $packageInfo.versions.Where({$_ -eq $versionToUnlist});
    }

    if ($versionsToUnlist.Count -gt 0) {
        "-----------------------------------------";
        $packageName + ":";
        "-----------------------------------------";

        foreach ($version in $versionsToUnlist) {
            if ($unlistPackagesForReal -eq "true") {
                "    Unlisting $version";
                "    nuget delete $packageName $version -Source $feedApiUrl -apikey $myGetPersonalAccessToken -NonInteractive";
                nuget delete $packageName $version -Source $feedApiUrl -apikey $myGetPersonalAccessToken -NonInteractive;
            } else {
                "    $version";
            }
        }
    }
}
"-----------------------------------------";