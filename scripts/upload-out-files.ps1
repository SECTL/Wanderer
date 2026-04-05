Import-Module .\xes-uploader.psm1 -Force

$outDir = "./out"
if (-not (Test-Path $outDir)) {
    throw "Output directory not found: $outDir"
}

$files = Get-ChildItem -Path $outDir -File | Sort-Object Name
if (-not $files) {
    throw "No files found in $outDir"
}

foreach ($file in $files) {
    Send-FileToXesCoding $file
}
