param(
    [ValidateSet("Evaluate", "Commit")]
    [string] $Mode = "Evaluate",
    [string] $ProjectRoot = ""
)

$ErrorActionPreference = "Stop"

if (-not $ProjectRoot) {
    $ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}
else {
    $ProjectRoot = (Resolve-Path $ProjectRoot).Path
}

$projectPath = Join-Path $ProjectRoot "dotnet\MultiSnap\MultiSnap.csproj"
$packageJsonPath = Join-Path $ProjectRoot "package.json"
$packageLockPath = Join-Path $ProjectRoot "package-lock.json"
$installerScriptPath = Join-Path $ProjectRoot "installer\MultiSnap.iss"
$statePath = Join-Path $ProjectRoot "artifacts\dotnet-version-state.json"
$pendingStatePath = Join-Path $ProjectRoot "artifacts\dotnet-version-pending.json"

function Convert-ToUtf8NoBomBytes([string] $Value) {
    return [System.Text.UTF8Encoding]::new($false).GetBytes($Value)
}

function Get-TextHash([string] $Value) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha.ComputeHash((Convert-ToUtf8NoBomBytes $Value))
        return ([System.BitConverter]::ToString($hash)).Replace("-", "")
    }
    finally {
        $sha.Dispose()
    }
}

function Get-RelativePath([string] $Path) {
    $rootUri = [System.Uri]::new(($ProjectRoot.TrimEnd("\") + "\"))
    $pathUri = [System.Uri]::new($Path)
    return [System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()).Replace("/", "\")
}

function Test-IsDotnetInput([System.IO.FileInfo] $File) {
    $relative = Get-RelativePath $File.FullName
    $segments = $relative -split "\\"

    if ($segments[0] -ne "dotnet" -and $relative -ne "installer\MultiSnap.iss" -and $relative -ne "package.json" -and $relative -ne "package-lock.json") {
        return $false
    }

    if ($segments -contains "bin" -or $segments -contains "obj" -or $segments -contains ".idea") {
        return $false
    }

    return $true
}

function Get-NormalizedContent([System.IO.FileInfo] $File) {
    $relative = Get-RelativePath $File.FullName
    $content = Get-Content $File.FullName -Raw

    if ($relative -eq "package.json" -or $relative -eq "package-lock.json") {
        $count = 1
        if ($relative -eq "package-lock.json") {
            $count = 2
        }

        $regex = [System.Text.RegularExpressions.Regex]::new('("version"\s*:\s*")[^"]+(")')
        return $regex.Replace($content, '${1}__VERSION__${2}', $count)
    }

    if ($relative -eq "dotnet\MultiSnap\MultiSnap.csproj") {
        return [System.Text.RegularExpressions.Regex]::Replace($content, '(<Version>)[^<]+(</Version>)', '${1}__VERSION__${2}')
    }

    if ($relative -eq "installer\MultiSnap.iss") {
        return [System.Text.RegularExpressions.Regex]::Replace($content, '(#define\s+MyAppVersion\s+")[^"]+(")', '${1}__VERSION__${2}')
    }

    return $content
}

function Get-SourceFingerprint {
    $entries = Get-ChildItem $ProjectRoot -Recurse -File |
        Where-Object { Test-IsDotnetInput $_ } |
        Sort-Object FullName |
        ForEach-Object {
            $relative = Get-RelativePath $_.FullName
            $hash = Get-TextHash (Get-NormalizedContent $_)
            "$relative=$hash"
        }

    return Get-TextHash ($entries -join "`n")
}

function Get-ProjectVersion {
    [xml] $projectXml = Get-Content $projectPath
    return $projectXml.Project.PropertyGroup.Version
}

function Get-NextPatchVersion([string] $Version) {
    $parts = $Version.Split(".")
    if ($parts.Count -ne 3) {
        throw "Version '$Version' is not a three-part semantic version."
    }

    return "$([int] $parts[0]).$([int] $parts[1]).$(([int] $parts[2]) + 1)"
}

function Set-RegexVersion([string] $Path, [string] $Pattern, [string] $Replacement, [int] $Count = 1) {
    $content = Get-Content $Path -Raw
    $regex = [System.Text.RegularExpressions.Regex]::new($Pattern)
    $updated = $regex.Replace($content, $Replacement, $Count)
    if ($updated -eq $content) {
        throw "Could not update version in $Path."
    }

    [System.IO.File]::WriteAllText($Path, $updated, [System.Text.UTF8Encoding]::new($false))
}

function Set-ProjectVersion([string] $Version) {
    Set-RegexVersion $packageJsonPath '("version"\s*:\s*")[^"]+(")' "`${1}$Version`${2}"
    Set-RegexVersion $packageLockPath '("version"\s*:\s*")[^"]+(")' "`${1}$Version`${2}" 2
    Set-RegexVersion $projectPath '(<Version>)[^<]+(</Version>)' "`${1}$Version`${2}"
    Set-RegexVersion $installerScriptPath '(#define\s+MyAppVersion\s+")[^"]+(")' "`${1}$Version`${2}"
}

function Read-State([string] $Path) {
    if (-not (Test-Path $Path)) {
        return $null
    }

    return Get-Content $Path -Raw | ConvertFrom-Json
}

function Save-State([string] $Path, [string] $Version, [string] $Fingerprint) {
    New-Item -ItemType Directory -Force -Path (Split-Path $Path -Parent) | Out-Null
    $state = [ordered] @{
        version = $Version
        sourceFingerprint = $Fingerprint
        updatedAtUtc = [DateTime]::UtcNow.ToString("o")
    }

    $json = $state | ConvertTo-Json -Depth 4
    [System.IO.File]::WriteAllText($Path, ($json + "`n"), [System.Text.UTF8Encoding]::new($false))
}

$version = Get-ProjectVersion
$fingerprint = Get-SourceFingerprint

if ($Mode -eq "Commit") {
    Save-State $statePath $version $fingerprint
    if (Test-Path $pendingStatePath) {
        Remove-Item $pendingStatePath -Force
    }
    return
}

$state = Read-State $statePath
$pendingState = Read-State $pendingStatePath
$shouldBump = $false

if ($state -and $state.sourceFingerprint -ne $fingerprint) {
    $shouldBump = $true
}
elseif (-not $state -and $pendingState -and $pendingState.sourceFingerprint -ne $fingerprint) {
    $shouldBump = $true
}

if ($shouldBump) {
    $version = Get-NextPatchVersion $version
    Set-ProjectVersion $version
    $fingerprint = Get-SourceFingerprint
    Save-State $pendingStatePath $version $fingerprint
}
elseif (-not $state -and -not $pendingState) {
    Save-State $pendingStatePath $version $fingerprint
}

Write-Output $version
