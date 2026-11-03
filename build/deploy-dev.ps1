<#
.SYNOPSIS
    Раскладка отладочной сборки песочницы в пользовательский каталог надстроек Revit.
.DESCRIPTION
    Вызывается целью PostBuild из Sandbox.App.csproj только в Debug-конфигурациях и только не в CI.
    Сборки кладутся в %AppData%\Autodesk\Revit\Addins\<версия>\Sandbox\,
    манифест Sandbox.addin — в корень %AppData%\Autodesk\Revit\Addins\<версия>\.
    Файл сохранён в UTF-8 с BOM: без BOM Windows PowerShell 5.1 читает кириллицу как ANSI.
.EXAMPLE
    .\deploy-dev.ps1 -RevitVersion 2025 -AssemblyPath E:\repo\build\bin\2025\Sandbox.App\Sandbox.App.dll
#>
[CmdletBinding()]                                                         # включает -Verbose и общие параметры
param(                                                                    # параметры приходят из цели PostBuild
    [Parameter(Mandatory)]                                                # версия обязательна
    [ValidateSet('2021','2022','2023','2024','2025')]                     # только поддерживаемые версии (ТР-У.00)
    [string]$RevitVersion,                                                # значение $(RevitVersion) из Directory.Build.props

    [Parameter(Mandatory)]                                                # путь обязателен
    [string]$AssemblyPath                                                 # значение $(TargetPath): полный путь к Sandbox.App.dll
)                                                                         # конец параметров

$ErrorActionPreference = 'Stop'                                           # любая ошибка -> исключение -> код выхода 1 -> сборка красная

# --- 1. Предусловия -------------------------------------------------------
$revit = Get-Process -Name 'Revit' -ErrorAction SilentlyContinue |        # все запущенные процессы Revit
    Where-Object { -not $_.Path -or $_.Path -like "*\Revit $RevitVersion\*" } # dll блокирует только Revit этой же версии; путь не прочитан — считаем совпадением
if ($revit) {                                                             # Revit удерживает загруженные dll
    throw "Revit $RevitVersion запущен (PID $($revit.Id -join ', ')). Закройте Revit и пересоберите."  # понятная ошибка вместо MSB3021
}                                                                         # конец проверки Revit

if (-not (Test-Path -LiteralPath $AssemblyPath -PathType Leaf)) {         # собранная dll должна существовать
    throw "Не найдена сборка: $AssemblyPath"                              # защита от неверного ручного вызова
}                                                                         # конец проверки сборки

$template = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\installer\addin\Sandbox.addin')) # эталонный манифест из репозитория
if (-not (Test-Path -LiteralPath $template -PathType Leaf)) {             # манифест должен лежать в installer\addin
    throw "Не найден манифест-шаблон: $template"                          # без него раскладка бессмысленна
}                                                                         # конец проверки шаблона

# --- 2. Пути ----------------------------------------------------------------
$sourceDir  = [IO.Path]::GetFullPath((Split-Path -Parent $AssemblyPath))  # build\bin\<версия>\Sandbox.App — полный нормализованный путь
$addinsRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$RevitVersion" # пользовательский каталог манифестов этой версии
$targetDir  = Join-Path $addinsRoot 'Sandbox'                             # подкаталог только этого плагина: очищается целиком
$manifest   = Join-Path $addinsRoot 'Sandbox.addin'                       # манифест в корне: подкаталоги Revit не просматривает

# --- 3. Копирование сборок --------------------------------------------------
if (Test-Path -LiteralPath $targetDir) {                                  # каталог остался от прошлой раскладки
    Remove-Item -LiteralPath $targetDir -Recurse -Force                   # очищаем: не должно остаться dll удалённых проектов
}                                                                         # конец очистки
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null           # создаём пустой каталог (и Addins\<версия>, если его нет)

Get-ChildItem -LiteralPath $sourceDir -Recurse -File | ForEach-Object {   # все файлы сборки, включая подкаталоги ресурсов
    $relative = $_.FullName.Substring($sourceDir.Length).TrimStart('\')   # путь файла относительно каталога сборки
    $dest     = Join-Path $targetDir $relative                            # тот же относительный путь в каталоге Revit
    $destDir  = Split-Path -Parent $dest                                  # каталог назначения файла
    if (-not (Test-Path -LiteralPath $destDir)) {                         # подкаталога ещё нет
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null     # создаём его
    }                                                                     # конец создания подкаталога
    Copy-Item -LiteralPath $_.FullName -Destination $dest -Force          # dll, pdb для точек останова, deps.json для 2025
}                                                                         # конец копирования

# --- 4. Манифест ------------------------------------------------------------
[xml]$xml = Get-Content -LiteralPath $template -Raw -Encoding UTF8        # читаем шаблон как XML, а не как текст
$node = $xml.SelectSingleNode('/RevitAddIns/AddIn/Assembly')              # узел пути к сборке
if ($null -eq $node) {                                                    # структура манифеста нарушена
    throw "В $template нет узла /RevitAddIns/AddIn/Assembly"              # останавливаемся, а не пишем битый манифест
}                                                                         # конец проверки узла
$node.InnerText = Join-Path $targetDir (Split-Path -Leaf $AssemblyPath)   # отладочный путь к Sandbox.App.dll
$xml.Save($manifest)                                                      # записываем манифест в корень Addins\<версия>\

Write-Host "deploy-dev: Revit $RevitVersion -> $targetDir"                 # строка для окна Output в Visual Studio
