Function Info($msg) {
    Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
    Write-Host `n`n
    Write-Error $msg
    exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
    if(-Not $?) {
        Error "${msg}. Error code: $LastExitCode"
    }
}

Function GetVersion() {
    $gitCommand = Get-Command -Name git

    try { $tag = & $gitCommand describe --exact-match --tags HEAD } catch { }
    if(-Not $?) {
        Info "The commit is not tagged. Use 'v0.0' as a version instead"
        $tag = "v0.0"
    }

    $commitHash = & $gitCommand rev-parse --short HEAD
    CheckReturnCodeOfPreviousCommand "Failed to get git commit hash"

    return "$($tag.Substring(1))"
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot"
$buildDir = "$root/build"
$publishDir = "$buildDir/Publish"
$version = GetVersion

Info "Version: '$version'"

dotnet build `
    --configuration Release `
    /property:DebugType=None `
    /property:Version=$version `
    $root/ExceptionGraphVisualizer.sln
CheckReturnCodeOfPreviousCommand "'dotnet build' command failed"

New-Item $publishDir -Force -ItemType "directory" > $null
Copy-Item -Force -Path "$buildDir/Release/Installer/Installer.msi" -Destination "$publishDir/ExceptionGraphVisualizer_vs2022_plugin.msi"
