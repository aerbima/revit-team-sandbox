## TASK-TS-02 · Поиск дублей марок (Core)

**Зависит от:** TS-01 (контракт уже в репозитории)  **Требования ТЗ:** ТР-У.06, ТР-У.07
**Файлы ТЗ:** [`Мини-ТЗ.md`](../docs/Мини-ТЗ.md), раздел 2
**Исполнитель:** dev2 · **План:** 3 ч · **Сценарии протокола:** 10, 16
**Результат:** поиск дублей, которым пользуется окно-итог TS-03.

### Порядок работы — два PR

1. **PR 1 «TASK-TS-02: контракт».** Только сигнатуры ниже с заглушками `throw new NotImplementedException("TASK-TS-02")` и атрибутом `[ExcludeFromCodeCoverage]`. В описании PR — строка «Тесты: не применимо — контракт». После слияния TS-03 может начинать.
2. **PR 2 «TASK-TS-02: реализация».** Код и тесты. Тесты вызывают `MarkGenerator.TryParse` из TS-01: пока TS-01 не влит, они красные. Это ожидаемо — PR 2 вливается после TS-01, через `git rebase origin/dev`.

### Что создаётся

```
src/Sandbox.Core/Marks/DuplicateReport.cs
src/Sandbox.Core/Marks/DuplicateMarkFinder.cs
tests/Sandbox.Core.Tests/Marks/DuplicateMarkFinderTests.cs
CHANGELOG.md                                — строка в разделе «Не выпущено»
```

### Контракт

```csharp
public sealed class DuplicateReport
{
    public DuplicateReport(IReadOnlyList<IReadOnlyList<string>> groups, IReadOnlyList<string> unrecognized); // создаёт результат
    public IReadOnlyList<IReadOnlyList<string>> Groups { get; }  // группы дублей, строки в порядке появления (ТР-У.06)
    public IReadOnlyList<string> Unrecognized { get; }           // строки, которые не разобрались (ТР-У.07)
    public bool HasDuplicates { get; }                           // Groups.Count > 0 — для окна-итога TS-03
}

public static class DuplicateMarkFinder
{
    public static DuplicateReport Find(MarkFormat format, IEnumerable<string> marks); // пустой вход → пустой отчёт
}
```

### Пример теста

```csharp
[Fact]
[Trait("Task", "TS-02")]                                    // задача
[Trait("TR", "У.06")]                                       // требование
public void Find_GroupsMarksEqualAfterParse()
{
    var marks = new[] { "П-01-003", "П-01-004", "п-01-0003" }; // первая и третья — одна и та же марка
    DuplicateReport report = DuplicateMarkFinder.Find(MarkFormat.Default, marks); // действие
    Assert.Single(report.Groups);                           // ровно одна группа
    Assert.Equal(new[] { "П-01-003", "п-01-0003" }, report.Groups[0]); // исходные строки, порядок сохранён
}
```

### Промт

```
#Мини-ТЗ.md #MarkGenerator.cs #DuplicateMarkFinder.cs

Реализуй DuplicateMarkFinder.Find по ТР-У.06 и ТР-У.07. Сравнение марок — только через
MarkGenerator.TryParse, своих разборов строки не пиши. Сними [ExcludeFromCodeCoverage].
Тесты xUnit с [Trait("Task","TS-02")] и [Trait("TR","У.06")] / [Trait("TR","У.07")].
```

### Готово, когда

- [ ] PR 1 с контрактом влит до начала TS-03
- [ ] Дубли определяются после разбора, порядок строк в группе — как во входе (ТР-У.06)
- [ ] Нераспознанные строки — в отдельном списке, пустой вход не роняет метод (ТР-У.07)
- [ ] PR 2 перебазирован на `dev` после слияния TS-01, CI зелёный
- [ ] Строка в `CHANGELOG.md` добавлена, конфликт с TS-01 разрешён

### Подводные камни

- Ключ группы — пара «код уровня + номер», а не исходная строка.
- `GroupBy` в LINQ сохраняет порядок первого появления — этим можно пользоваться.
