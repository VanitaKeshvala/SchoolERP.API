
$jsonPath = "c:\Users\svmim\OneDrive\Desktop\SchoolERP\SchoolERP.Net\wwwroot\language\Arabic\app_files\system_lang.json"
$content = Get-Content $jsonPath -Raw
$matches = [regex]::Matches($content, '"([^"]+)":')
$keys = $matches.Groups | Where-Object { $_.Name -eq '1' } | Select-Object -ExpandProperty Value
$duplicates = $keys | Group-Object | Where-Object { $_.Count -gt 1 }
foreach ($dup in $duplicates) {
    Write-Host "Duplicate key: $($dup.Name) count: $($dup.Count)"
}
