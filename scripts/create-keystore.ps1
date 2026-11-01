# Создаёт ключ подписи Android вне репозитория.
#
# Пример:
#   powershell -ExecutionPolicy Bypass -File scripts/create-keystore.ps1
#
# Скрипт никогда не печатает пароль: он сохраняется в файл рядом с ключом.
# Перенесите пароль в менеджер паролей и удалите файл.

[CmdletBinding()]
param(
    [string]$Alias = 'winnerchooser',
    [string]$OutDir = (Join-Path $env:USERPROFILE '.winnerchooser'),
    [string]$Keytool
)

$ErrorActionPreference = 'Stop'

if (-not $Keytool) {
    $candidates = @(
        'C:\Program Files (x86)\Android\openjdk\jdk-17.0.14\bin\keytool.exe',
        'C:\Program Files\Android\openjdk\jdk-17.0.14\bin\keytool.exe'
    )
    $Keytool = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    if (-not $Keytool) {
        $Keytool = (Get-Command keytool -ErrorAction SilentlyContinue).Source
    }
}

if (-not $Keytool -or -not (Test-Path -LiteralPath $Keytool)) {
    throw 'keytool не найден. Укажите путь параметром -Keytool.'
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$keystore = Join-Path $OutDir 'winnerchooser.keystore'
$passwordFile = Join-Path $OutDir 'keystore-password.txt'

if (Test-Path -LiteralPath $keystore) {
    throw "Ключ уже существует: $keystore. Скрипт не перезаписывает ключи: потеря ключа блокирует обновления приложения."
}

# 32 символа без похожих друг на друга букв и цифр.
$alphabet = 'abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789'
$password = -join (1..32 | ForEach-Object { $alphabet[(Get-Random -Maximum $alphabet.Length)] })

& $Keytool -genkeypair -v `
    -keystore $keystore `
    -alias $Alias `
    -keyalg RSA `
    -keysize 2048 `
    -validity 10000 `
    -storepass $password `
    -keypass $password `
    -dname 'CN=WinnerChooser, O=WinnerChooser, C=RU'

if ($LASTEXITCODE -ne 0) {
    throw "keytool завершился с кодом $LASTEXITCODE"
}

Set-Content -LiteralPath $passwordFile -Value $password -NoNewline -Encoding ASCII

Write-Host "Ключ создан: $keystore"
Write-Host "Пароль сохранён в: $passwordFile"
Write-Host 'Перенесите пароль в менеджер паролей и удалите файл. Сделайте две резервные копии ключа.'
