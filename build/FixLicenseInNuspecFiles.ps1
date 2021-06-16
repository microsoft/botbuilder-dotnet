#
# Replace "licenseUrl>" with "license>" in .nuspec files. 
# Designed to be run after unpacking and before signing .nupkg file contents.
# It eliminates "WARNING: The <licenseUrl> element is deprecated" when pushing to 
# nuget.org. It compensates for a bug in Visual Studio's .nuspec file generation 
# dating from when <licenseUrl> was deprecated in 2018. 6/15/2021
#
$path = "$(Build.SourcesDirectory)/libraries/*/package.json/signing/*/*.nuspec";
$find = "licenseUrl>";
$replace = "license>";

function GoReplace() {
    Get-ChildItem -Path "$path" | % {
        $_.FullName; 
        $content = Get-Content -Raw $_.FullName;

        $content -Replace "$find", "$replace" | Set-Content $_.FullName;
        '-------------'; get-content $_.FullName; '===================';
    }
}

GoReplace;