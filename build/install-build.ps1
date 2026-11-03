param(                                                            # параметры запуска
    [Parameter(Mandatory)][ValidateSet('2021','2022','2023','2024','2025')][string]$RevitVersion,  # версия Revit тестировщика
    [Parameter(Mandatory)][string]$Zip                            # архив build-revitXXXX.zip со страницы прогона CI
)                                                                 # конец параметров
$ErrorActionPreference = 'Stop'                                   # любая ошибка — остановка
if (Get-Process -Name Revit -ErrorAction SilentlyContinue) { throw 'Закройте Revit перед установкой' }  # Revit держит dll
$root = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion"  # каталог надстроек этой версии
New-Item -ItemType Directory -Force -Path $root | Out-Null        # каталога может не быть на чистой машине
$dir  = Join-Path $root 'Sandbox'                                 # каталог сборок плагина
if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }          # убираем прошлую тестовую сборку целиком
Expand-Archive -Path $Zip -DestinationPath $dir                   # распаковываем новую
$manifest = Join-Path $PSScriptRoot '..\installer\addin\Sandbox.addin'  # эталонный манифест из репозитория
[xml]$xml = Get-Content -Raw -Encoding UTF8 $manifest              # читаем как XML
$xml.RevitAddIns.AddIn.Assembly = Join-Path $dir 'Sandbox.App.dll'  # путь к сборке тестовой версии
$xml.Save((Join-Path $root 'Sandbox.addin'))                      # манифест — в корень каталога надстроек
Write-Host "Установлено: Revit $RevitVersion → $dir"              # подтверждение для тестировщика
