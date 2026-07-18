[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $OutputPath = (Join-Path $PSScriptRoot 'Assets-Code-Archive.zip'),

    [Parameter()]
    [ValidateScript({ $_ -gt 0 })]
    [double] $MaxFileSizeMB = 5,

    [Parameter()]
    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$sourceExtensions = @(
    '.asmdef',
    '.asmref',
    '.cginc',
    '.compute',
    '.cs',
    '.glsl',
    '.hlsl',
    '.inputactions',
    '.json',
    '.md',
    '.rsp',
    '.shader',
    '.shadergraph',
    '.shadersubgraph',
    '.uss',
    '.uxml',
    '.xml',
    '.yaml',
    '.yml'
)

$unityYamlExtensions = @(
    '.anim',
    '.asset',
    '.controller',
    '.lighting',
    '.mask',
    '.mat',
    '.overridecontroller',
    '.physicmaterial',
    '.physicsmaterial2d',
    '.playable',
    '.prefab',
    '.preset',
    '.rendertexture',
    '.unity'
)

function Test-UnityYamlFile {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo] $File
    )

    $expectedHeader = '%YAML 1.1'
    $buffer = New-Object byte[] $expectedHeader.Length
    $stream = [System.IO.File]::Open(
        $File.FullName,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read,
        [System.IO.FileShare]::ReadWrite
    )

    try {
        $bytesRead = $stream.Read($buffer, 0, $buffer.Length)
        if ($bytesRead -ne $buffer.Length) {
            return $false
        }

        return [System.Text.Encoding]::ASCII.GetString($buffer) -ceq $expectedHeader
    }
    finally {
        $stream.Dispose()
    }
}

$projectRoot = [System.IO.Path]::GetFullPath($PSScriptRoot)
$assetsRoot = [System.IO.Path]::Combine($projectRoot, 'Assets')

if (-not [System.IO.Directory]::Exists($assetsRoot)) {
    throw "Assets directory was not found: $assetsRoot"
}

if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    $resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
}
else {
    $resolvedOutputPath = [System.IO.Path]::GetFullPath(
        [System.IO.Path]::Combine($projectRoot, $OutputPath)
    )
}

$assetsPrefix = $assetsRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar
) + [System.IO.Path]::DirectorySeparatorChar

if ($resolvedOutputPath.StartsWith($assetsPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputPath must not be inside Assets: $resolvedOutputPath"
}

$outputDirectory = [System.IO.Path]::GetDirectoryName($resolvedOutputPath)
if ([string]::IsNullOrWhiteSpace($outputDirectory) -or
    -not [System.IO.Directory]::Exists($outputDirectory)) {
    throw "Output directory does not exist: $outputDirectory"
}

if ([System.IO.File]::Exists($resolvedOutputPath) -and -not $Force) {
    throw "Output archive already exists. Use -Force to replace it: $resolvedOutputPath"
}

$maxFileSizeBytes = [math]::Floor($MaxFileSizeMB * 1MB)
$includedFiles = New-Object System.Collections.Generic.List[System.IO.FileInfo]
$includedSourceCount = 0
$includedUnityYamlCount = 0
$oversizedCandidateCount = 0

foreach ($file in Get-ChildItem -LiteralPath $assetsRoot -Recurse -Force -File) {
    $extension = $file.Extension.ToLowerInvariant()
    $isSourceFile = $sourceExtensions -contains $extension
    $isUnityYamlFile = $false

    if (-not $isSourceFile -and $unityYamlExtensions -contains $extension) {
        $isUnityYamlFile = Test-UnityYamlFile -File $file
    }

    if (-not $isSourceFile -and -not $isUnityYamlFile) {
        continue
    }

    if ($file.Length -gt $maxFileSizeBytes) {
        $oversizedCandidateCount++
        continue
    }

    $includedFiles.Add($file)
    if ($isSourceFile) {
        $includedSourceCount++
    }
    else {
        $includedUnityYamlCount++
    }
}

$orderedFiles = @($includedFiles | Sort-Object FullName)
$operationId = [guid]::NewGuid().ToString('N')
$temporaryPath = '{0}.{1}.tmp' -f $resolvedOutputPath, $operationId
$backupPath = '{0}.{1}.bak' -f $resolvedOutputPath, $operationId

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

try {
    $fileStream = [System.IO.File]::Open(
        $temporaryPath,
        [System.IO.FileMode]::CreateNew,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None
    )

    try {
        $archive = New-Object System.IO.Compression.ZipArchive(
            $fileStream,
            [System.IO.Compression.ZipArchiveMode]::Create,
            $false
        )

        try {
            foreach ($file in $orderedFiles) {
                $relativePath = $file.FullName.Substring($assetsPrefix.Length)
                $entryName = 'Assets/' + $relativePath.Replace(
                    [System.IO.Path]::DirectorySeparatorChar,
                    '/'
                )

                [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                    $archive,
                    $file.FullName,
                    $entryName,
                    [System.IO.Compression.CompressionLevel]::Optimal
                ) | Out-Null
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $fileStream.Dispose()
    }

    if ([System.IO.File]::Exists($resolvedOutputPath)) {
        [System.IO.File]::Replace($temporaryPath, $resolvedOutputPath, $backupPath)
    }
    else {
        [System.IO.File]::Move($temporaryPath, $resolvedOutputPath)
    }
}
finally {
    if ([System.IO.File]::Exists($temporaryPath)) {
        [System.IO.File]::Delete($temporaryPath)
    }

    if ([System.IO.File]::Exists($backupPath)) {
        [System.IO.File]::Delete($backupPath)
    }
}

$archiveSizeMB = [math]::Round(
    ([System.IO.FileInfo] $resolvedOutputPath).Length / 1MB,
    2
)

Write-Host "Created: $resolvedOutputPath"
Write-Host "Included source/config files: $includedSourceCount"
Write-Host "Included Unity YAML files: $includedUnityYamlCount"
Write-Host "Skipped matching files over $MaxFileSizeMB MB: $oversizedCandidateCount"
Write-Host "Archive size: $archiveSizeMB MB"
