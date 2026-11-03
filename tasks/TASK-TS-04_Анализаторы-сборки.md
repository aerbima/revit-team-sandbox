## TASK-TS-04 · Анализаторы и «предупреждение = ошибка» (защищённый файл)

**Зависит от:** —  **Требования ТЗ:** ТР-У.13
**Файлы ТЗ:** [`Мини-ТЗ.md`](../docs/Мини-ТЗ.md), раздел 4
**Исполнитель:** dev1 · **План:** 2 ч · **Сценарии протокола:** 7, 8
**Результат:** проверка качества кода при каждой сборке. Главная цель задачи — проверить, что изменение защищённого файла `Directory.Build.props` нельзя влить без одобрения тимлида.

### Что меняется

```
Directory.Build.props       — защищён CODEOWNERS
src/**/*.cs                 — только если после включения анализаторов появятся предупреждения
CHANGELOG.md
```

### Контракт

```xml
<PropertyGroup>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>                  <!-- анализаторы .NET и для netstandard2.0 / net48 -->
  <AnalysisLevel>latest-Recommended</AnalysisLevel>              <!-- рекомендуемый набор правил -->
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>        <!-- правила .editorconfig проверяются и в CI -->
</PropertyGroup>
<PropertyGroup Condition="$(Configuration.StartsWith('Release'))">
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>            <!-- в Release предупреждение = ошибка -->
</PropertyGroup>
```

### Готово, когда

- [ ] Блок добавлен в `Directory.Build.props` на место комментария «TASK-TS-04»
- [ ] Сборка Release2021 и Release2025 без предупреждений, CI зелёный
- [ ] В описании PR строка «Тесты: не применимо — изменение настроек сборки»
- [ ] PR одобрен владельцем кода (тимлидом); одобрения второго программиста недостаточно — это и проверяем
- [ ] Строка в `CHANGELOG.md` добавлена

### Подводные камни

- Предупреждение нельзя «чинить» подавлением `#pragma warning disable` без объяснения в PR.
- Если правило мешает массово, обсудите с тимлидом точечную настройку в `.editorconfig` — это тоже защищённое изменение.
