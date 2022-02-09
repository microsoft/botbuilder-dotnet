#
# Shows but cannot unlist npm package versions on MyGet.org lower than or equal to $versionToUnlist.
# Cannot unlist because per MyGet support, "we do not support the npm unpublish command". Unlisting must be done in the UI.
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

"versionToUnlist: " + $versionToUnlist;
"unlistOlderVersionsAlso: " + $unlistOlderVersionsAlso;
"unlistPackagesForReal: " + $unlistPackagesForReal;
" ";
"Package versions to unlist:"

$result = Invoke-RestMethod -Uri $feedStateUrl -Method Get -ContentType "application/json";

npm config set registry https://botbuilder.myget.org/F/botbuilder-v4-js-daily/npm/;

foreach ($packageName in $packageNames) {
    $versionsToUnlist = $Null;
    $index = -1;

    $packageInfo = $result.packages | Where-Object {$_.id -eq $packageName};

    [string]$unsortedVersions = $packageInfo.versions | %{ $_ + "`r`n" };

    $sortedVersions = Sort-Versions $unsortedVersions;

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
"-----------------------------------------";
