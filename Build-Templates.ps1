function Resolve-MsBuild {
	$msb2017 = Resolve-Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio\*\*\MSBuild\*\bin\msbuild.exe" -ErrorAction SilentlyContinue
	if($msb2017) {
		Write-Host "Found MSBuild 2017 (or later)."
		Write-Host $msb2017
		return $msb2017
	}

	$msBuild2015 = "${env:ProgramFiles(x86)}\MSBuild\14.0\bin\msbuild.exe"

	if(-not (Test-Path $msBuild2015)) {
		throw 'Could not find MSBuild 2015 or later.'
	}

	Write-Host "Found MSBuild 2015."
	Write-Host $msBuild2015

	return $msBuild2015
}

function Resolve-VsixSignTool {
	$signToolPath = Resolve-Path ".\packages\Microsoft.VSSDK.VsixSignTool*\tools\vssdk\vsixsigntool.exe" -ErrorAction SilentlyContinue
	
	if(-not (Test-Path $signToolPath)) {
		throw 'Could not find VSIX Sign Path.'
	}

    Write-Host "Found VSIX Sign Path."
	Write-Host $signToolPath
	return $signToolPath
}

#Manually generate template from EchoBot-With-State sample

Write-Host 'Generate zip file of template'
Compress-Archive -Force .\templates\EchoBot.Template\ProjectTemplates\EchoBot-With-State\ .\templates\EchoBot.Template\ProjectTemplates\EchoBot-With-State.zip

Write-Host 'Build VSIX project'
$msBuild = Resolve-MsBuild
& $msBuild .\Microsoft.Bot.Builder.Templates.sln /p:Configuration=Release

Write-Host 'Sign VSIX file'
$signTool = Resolve-VsixSignTool
& $signTool sign /v /f .\templates\EchoBot.Template\certificate.pfx /p $env:templates_sign /fd sha256 .\templates\EchoBot.Template\bin\Release\EchoBot.Template.vsix

Write-Host 'Upload to MyGet'
Write-Host 'curl -X POST --verbose --data-binary @"path-to-vsix-file.vsix" -H "X-NuGet-ApiKey: api-key-here" https://www.myget.org/F/<feedname>/vsix/upload'