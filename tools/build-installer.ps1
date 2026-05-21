param(
    [string] $Configuration = "Release",
    [string] $Runtime = "win-x64",
    [string] $InnoCompilerPath = ""
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $root "dotnet\MultiSnap\MultiSnap.csproj"
$publishDir = Join-Path $root "dotnet\MultiSnap\bin\$Configuration\net8.0-windows\$Runtime\publish"
$installerScript = Join-Path $root "installer\MultiSnap.iss"

dotnet publish $project -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=false -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

if (-not $InnoCompilerPath) {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 5\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 5\ISCC.exe")
    )

    $InnoCompilerPath = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if (-not $InnoCompilerPath -or -not (Test-Path $InnoCompilerPath)) {
    throw "ISCC.exe was not found. Pass -InnoCompilerPath or install Inno Setup."
}

& $InnoCompilerPath $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compiler failed with exit code $LASTEXITCODE."
}
