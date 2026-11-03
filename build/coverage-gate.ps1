param([int]$Threshold = 70, [string]$Assembly = 'Sandbox.Core')  # порог в процентах и сборка, чьё покрытие проверяем
$ErrorActionPreference = 'Stop'                                   # любая ошибка останавливает шаг CI
dotnet tool install --global dotnet-reportgenerator-globaltool | Out-Null  # генератор отчётов покрытия
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"                    # глобальные инструменты .NET — в PATH
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:coverage" "-reporttypes:Cobertura;MarkdownSummaryGithub;Html"  # объединить отчёты всех тестовых проектов
[xml]$cov = Get-Content -Raw coverage/Cobertura.xml               # объединённый отчёт; package = сборка
$core = $cov.coverage.packages.package | Where-Object { $_.name -eq $Assembly }  # пакет нужной сборки
if (-not $core) { throw "В отчёте покрытия нет сборки $Assembly" }  # тесты ядра не запускались
$pct = [math]::Round([double]$core.'line-rate' * 100, 1)          # доля покрытых строк → проценты
Get-Content coverage/SummaryGithub.md | Add-Content $env:GITHUB_STEP_SUMMARY  # таблица покрытия на странице прогона
"**${Assembly}: $pct % строк** (порог $Threshold %)" | Add-Content $env:GITHUB_STEP_SUMMARY  # итоговая строка
if ($pct -lt $Threshold) { throw "Покрытие $Assembly $pct % ниже порога $Threshold %" }  # красный шаг — PR не влить
