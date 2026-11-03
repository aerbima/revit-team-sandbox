## TASK-TS-01 · Генератор марок (Core)

**Зависит от:** —  **Требования ТЗ:** ТР-У.01…ТР-У.05
**Файлы ТЗ:** [`Мини-ТЗ.md`](../docs/Мини-ТЗ.md), разделы 1 и «Формат марки по умолчанию»
**Исполнитель:** dev1 · **План:** 4 ч · **Сценарии протокола:** 3–7, 9, 12–13, 15–16
**Результат:** реализация контракта `MarkFormat` и `MarkGenerator`, на которую опираются TS-02 и TS-03.

### Что создаётся

```
src/Sandbox.Core/Marks/MarkFormat.cs        — заглушка уже в репозитории, заменить реализацией
src/Sandbox.Core/Marks/MarkGenerator.cs     — то же
tests/Sandbox.Core.Tests/Marks/MarkFormatTests.cs
tests/Sandbox.Core.Tests/Marks/MarkGeneratorTests.cs
CHANGELOG.md                                — строка в разделе «Не выпущено»
```

### Контракт

Сигнатуры уже лежат в репозитории (тимлид влил их до старта). Менять их нельзя: TS-02 пишет код против них параллельно с вами.

```csharp
public sealed class MarkFormat
{
    public MarkFormat(string prefix, char separator, int digits); // проверки ТР-У.02, ТР-У.03
    public static MarkFormat Default { get; }                    // «П», «-», 3
    public string Prefix { get; }                                // «П»
    public char Separator { get; }                               // '-'
    public int Digits { get; }                                   // 3
}

public static class MarkGenerator
{
    public static string Generate(MarkFormat format, string levelCode, int number);     // «П-01-003»
    public static bool TryParse(MarkFormat format, string mark, out string levelCode, out int number); // обратная операция
}
```

### Пример теста

```csharp
[Theory]                                                    // один тест — несколько наборов данных
[InlineData("01", 3, "П-01-003")]                           // уровень 01, номер 3
[InlineData("B1", 120, "П-B1-120")]                         // буквенно-цифровой код уровня
[Trait("Task", "TS-01")]                                    // привязка к задаче — её ищет проверка trace в CI
[Trait("TR", "У.01")]                                       // привязка к требованию мини-ТЗ
public void Generate_BuildsMark(string level, int number, string expected)
{
    string mark = MarkGenerator.Generate(MarkFormat.Default, level, number); // действие
    Assert.Equal(expected, mark);                           // точное сравнение строки
}
```

### Промт

```
#Мини-ТЗ.md #MarkFormat.cs #MarkGenerator.cs

Реализуй MarkFormat и MarkGenerator по ТР-У.01…ТР-У.05 мини-ТЗ. Сигнатуры не меняй.
Сними [ExcludeFromCodeCoverage] с обоих классов. Напиши xUnit-тесты: на каждый ТР —
хотя бы один тест с [Trait("Task","TS-01")] и [Trait("TR","У.xx")], включая ошибочные входы.
Проект netstandard2.0: без record и без API новее .NET Standard 2.0.
```

### Готово, когда

- [ ] `Generate` строит марку по ТР-У.01, проверки ТР-У.02…У.04 бросают исключения с верным именем параметра
- [ ] `TryParse` разбирает марку без учёта регистра префикса и с лишними нулями, на мусоре возвращает `false` (ТР-У.05)
- [ ] Атрибут `[ExcludeFromCodeCoverage]` снят, покрытие `Sandbox.Core` в CI не ниже 70 %
- [ ] Строка в `CHANGELOG.md` добавлена
- [ ] CI зелёный по всем пяти версиям

### Подводные камни

- «Заглавная буква» — проверяйте `char.IsUpper` и `char.IsLetter`, а не диапазон `А–Я`: буква «Ё» в него не входит.
- `int.Parse` с культурой по умолчанию на чужой машине может вести себя иначе — используйте `CultureInfo.InvariantCulture`.
- Номер 1000 при 3 знаках — ошибка, а не марка `П-01-1000`.
