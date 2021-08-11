#
# Shows but cannot unlist npm package versions on MyGet.org lower than or equal to $versionToUnlist.
# Cannot unlist because per MyGet support, "we do not support the npm unpublish command".
# 
param
( 
    [string]$versionToUnlist = "4.0.5-1500",
    [string]$unlistOlderVersionsAlso = "false",
    [string[]]$packageNames = @( "adaptive-expressions","botbuilder","botbuilder-dialogs-adaptive-runtime-core" ),
    [string]$myGetFeedName = "botbuilder-v4-js-daily",
    [string]$myGetPersonalAccessToken,
    [string]$unlistPackagesForReal = "false"
)

$feedStateUrl = "https://botbuilder.myget.org/F/$myGetFeedName/auth/$myGetPersonalAccessToken/api/v2/feed-state";
#$feedApiUrl = "https://botbuilder.myget.org/F/$myGetFeedName/npm/";
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
"Package versions to unlist:";

npm config set registry https://botbuilder.myget.org/F/botbuilder-v4-js-daily/npm/;

foreach ($packageName in $packageNames) {
    $versionsToUnlist = $null;

    $package = $result.packages | Where-Object {$_.id -eq $packageName};

    if ($unlistOlderVersionsAlso -eq "true") {
        [string]$unsortedVersions = $package.versions | %{ $_ + "`r`n" };

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
            #$url = "$feedApiUrl$packageName/versions/$version";
            if ($unlistPackagesForReal -eq "true") {
                "    Unlisting $version (nonfunctional";
                #"    npm unpublish $packageName@v$version --loglevel verbose";
                #npm unpublish $packageName@v$version --loglevel verbose;
                #Invoke-RestMethod -Uri $url -Method Delete -ContentType "application/json";
                #"nuget delete $packageName $version -Source $feedApiUrl -apikey $myGetPersonalAccessToken -NonInteractive";
                #nuget delete $packageName $version -Source $feedApiUrl -apikey $myGetPersonalAccessToken -NonInteractive;
            } else {
                "    $version";
            }
        }
    }
}