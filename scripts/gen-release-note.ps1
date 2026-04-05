$modulePath = Join-Path $PSScriptRoot "xes-uploader.psm1"
Import-Module $modulePath -Force

$tag = $env:tagName
if ([string]::IsNullOrWhiteSpace($tag)) {
    throw "Environment variable 'tagName' is required."
}

$changelogPath = "./changelogs/$tag.md"
$releaseNotePath = "./release-note.md"
$outDir = "./out"

if (-not (Test-Path $outDir)) {
    throw "Output directory not found: $outDir"
}

$files = Get-ChildItem -Path $outDir -File | Sort-Object Name
if (-not $files) {
    throw "No files found in $outDir"
}

$md5Summary = @"
> [!important]
> 下载时请核对文件 MD5。

| 文件名 | MD5 |
| --- | --- |
"@

$hashes = [ordered]@{}
foreach ($file in $files) {
    $url = Send-FileToXesCoding $file
    $hash = (Get-FileHash $file.FullName -Algorithm MD5).Hash
    
    $md5Summary += "`n| $($file.Name) [高速下载]($url) | ``$hash`` |"
    $hashes[$file.Name] = $hash
}

$json = ConvertTo-Json $hashes -Compress
$md5Summary += "`n`n<!-- WANDERERATTENDANCE_PKG_MD5 $json -->"

$changelog = if (Test-Path $changelogPath) {
    Get-Content $changelogPath -Raw
} else {
    "## $tag`n`n- 发布说明待补充。"
}

$fullContent = "$changelog`n`n$md5Summary"
Set-Content -Path $releaseNotePath -Value $fullContent -Encoding utf8

Write-Host "Release Note generated: $releaseNotePath"
