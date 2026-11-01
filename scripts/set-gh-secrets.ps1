# Загружает секреты подписи в GitHub Actions.
#
# Пример:
#   powershell -ExecutionPolicy Bypass -File scripts/set-gh-secrets.ps1
#   powershell -ExecutionPolicy Bypass -File scripts/set-gh-secrets.ps1 -Repo Vanish-q/WinnerChooser
#
# Значения не печатаются и не попадают в вывод.

[CmdletBinding()]
param(
    [string]$Repo = '',
    [string]$Alias = 'winnerchooser',
    [string]$SecretsDir = (Join-Path $env:USERPROFILE '.winnerchooser')
)

$ErrorActionPreference = 'Stop'

$keystore = Join-Path $SecretsDir 'winnerchooser.keystore'
$passwordFile = Join-Path $SecretsDir 'keystore-password.txt'

if (-not (Test-Path -LiteralPath $keystore)) {
    throw "Не найден ключ: $keystore. Сначала запустите scripts/create-keystore.ps1"
}

if (-not (Test-Path -LiteralPath $passwordFile)) {
    throw "Не найден файл пароля: $passwordFile"
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'Не найден gh CLI. Установите GitHub CLI и выполните gh auth login.'
}

$password = (Get-Content -LiteralPath $passwordFile -Raw).Trim()
$base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($keystore))

function Set-Secret {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Value
    )

    if ($Repo) {
        gh secret set $Name --repo $Repo --body $Value
    } else {
        gh secret set $Name --body $Value
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Не удалось загрузить секрет $Name"
    }

    Write-Host "Секрет $Name загружен"
}

Set-Secret -Name 'ANDROID_KEYSTORE_BASE64' -Value $base64
Set-Secret -Name 'ANDROID_KEYSTORE_PASSWORD' -Value $password
Set-Secret -Name 'ANDROID_KEY_ALIAS' -Value $Alias
Set-Secret -Name 'ANDROID_KEY_PASSWORD' -Value $password
