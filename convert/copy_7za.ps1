$targetDir = "bin\Debug\net6.0-windows\7za"
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force
}

Copy-Item "7za\*" -Destination $targetDir -Force
Write-Host "Файлы 7za скопированы в $targetDir" 