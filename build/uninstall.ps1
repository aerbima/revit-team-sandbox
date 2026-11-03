param([Parameter(Mandatory)][ValidateSet('2021','2022','2023','2024','2025')][string]$RevitVersion)  # версия Revit
$ErrorActionPreference = 'Stop'                                   # любая ошибка — остановка
$root = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion"  # каталог надстроек этой версии
Remove-Item (Join-Path $root 'Sandbox.addin') -ErrorAction SilentlyContinue  # манифест: без него Revit плагин не грузит
Remove-Item (Join-Path $root 'Sandbox') -Recurse -Force -ErrorAction SilentlyContinue  # сборки плагина
Write-Host "Песочница удалена из Revit $RevitVersion"             # подтверждение
