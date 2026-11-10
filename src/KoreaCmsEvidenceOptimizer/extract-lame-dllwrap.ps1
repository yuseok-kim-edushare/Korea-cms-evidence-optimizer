param(
    [Parameter(Mandatory = $true)][string]$PackageDll,
    [Parameter(Mandatory = $true)][string]$ResourceName,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$bytes = [System.IO.File]::ReadAllBytes($PackageDll)
$assembly = [System.Reflection.Assembly]::Load($bytes)
$stream = $assembly.GetManifestResourceStream($ResourceName)
if (-not $stream) {
    throw "Embedded resource '$ResourceName' was not found in $PackageDll"
}

$directory = Split-Path $OutputPath -Parent
if (-not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}

$out = [System.IO.File]::Create($OutputPath)
try {
    $stream.CopyTo($out)
}
finally {
    $out.Close()
    $stream.Close()
}
