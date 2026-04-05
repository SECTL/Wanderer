function ConvertTo-Hashtable {
    param([Parameter(ValueFromPipeline)] $InputObject)
    process {
        if ($InputObject -is [PSCustomObject]) {
            $hash = @{}
            $InputObject.PSObject.Properties | ForEach-Object { $hash[$_.Name] = $_.Value }
            $hash
        } else {
            $InputObject
        }
    }
}

function Send-FileToXesCoding {
    param([string]$Path)

    $fileHash = (Get-FileHash $Path -Algorithm MD5).Hash
    $fileName = Split-Path $Path -Leaf

    $ossParamsUrl =
    "https://code.xueersi.com/api/assets/get_oss_upload_params" +
            "?scene=offline_python_assets&md5=$fileHash&filename=$fileName"
    $ossParamsResponse = Invoke-WebRequest `
        -Uri $ossParamsUrl `
        -Headers @{
        "Authorization" = "e7e380401dc9a31fce2117a60c99ba04"
    }
    $ossParams = ($ossParamsResponse.Content | ConvertFrom-Json).data


    $uploadResponse = Invoke-WebRequest `
        -Uri $ossParams.host `
        -Headers (ConvertTo-Hashtable $ossParams.headers) `
        -Method Put `
        -InFile $Path

    if ($uploadResponse.StatusCode -eq 200) {
        Write-Host Successfully uploaded $fileName to $ossParams.url -ForegroundColor Green
        $ossParams.url
    } else {
        Write-Host Failed to upload $fileName -ForegroundColor Red
        ""
    }
}

Export-ModuleMember -Function Send-FileToXesCoding