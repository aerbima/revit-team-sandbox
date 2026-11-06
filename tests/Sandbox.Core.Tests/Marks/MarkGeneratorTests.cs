using System;                                                        // ArgumentException, ArgumentOutOfRangeException
using System.Globalization;                                          // CultureInfo
using Sandbox.Core.Marks;                                            // MarkFormat, MarkGenerator
using Xunit;                                                         // Fact, Theory, Trait, Assert

namespace Sandbox.Core.Tests.Marks;                                  // тесты маркировки

/// <summary>Построение и разбор марок (ТР-У.01, ТР-У.03…ТР-У.05).</summary>
public class MarkGeneratorTests
{
    // ---------- ТР-У.01: построение марки ----------

    [Theory]                                                         // один тест — несколько наборов данных
    [InlineData("01", 3, "П-01-003")]                                // пример из мини-ТЗ
    [InlineData("B1", 120, "П-B1-120")]                              // буквенно-цифровой код уровня
    [InlineData("1", 1, "П-1-001")]                                  // минимальные код и номер
    [InlineData("ABCD", 999, "П-ABCD-999")]                          // максимальные код и номер при 3 знаках
    [Trait("Task", "TS-01")]                                         // привязка к задаче — её ищет проверка trace в CI
    [Trait("TR", "У.01")]                                            // привязка к требованию мини-ТЗ
    public void Generate_BuildsMark(string level, int number, string expected)
    {
        string mark = MarkGenerator.Generate(MarkFormat.Default, level, number); // действие

        Assert.Equal(expected, mark);                                // точное сравнение строки
    }

    [Theory]
    [InlineData("Q", '/', 1, "02", 7, "Q/02/7")]                     // 1 знак — без ведущих нулей
    [InlineData("AB", '.', 5, "3", 42, "AB.3.00042")]                // 5 знаков
    [InlineData("ЁЖ", '_', 2, "Л1", 99, "ЁЖ_Л1_99")]                 // кириллица в префиксе и коде уровня
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.01")]
    public void Generate_UsesFormatSettings(string prefix, char separator, int digits, string level, int number, string expected)
    {
        var format = new MarkFormat(prefix, separator, digits);      // нестандартный формат

        string mark = MarkGenerator.Generate(format, level, number); // действие

        Assert.Equal(expected, mark);                                // префикс, разделитель и число знаков из формата
    }

    [Fact]
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.01")]
    public void Generate_DoesNotDependOnCurrentCulture()
    {
        CultureInfo saved = CultureInfo.CurrentCulture;              // запоминаем культуру потока
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");   // культура с другими правилами чисел
            string mark = MarkGenerator.Generate(MarkFormat.Default, "01", 3); // действие

            Assert.Equal("П-01-003", mark);                          // те же ASCII-цифры
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;                      // не влияем на другие тесты
        }
    }

    [Fact]
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.01")]
    public void Generate_RejectsNullFormat()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => MarkGenerator.Generate(null!, "01", 1)); // без формата марку не построить

        Assert.Equal("format", ex.ParamName);                        // какой параметр неверен
    }

    // ---------- ТР-У.03: диапазон номера ----------

    [Theory]
    [InlineData(3, 0)]                                               // номер 0
    [InlineData(3, -5)]                                              // отрицательный
    [InlineData(3, 1000)]                                            // 1000 при 3 знаках — ошибка, а не «П-01-1000»
    [InlineData(1, 10)]                                              // 10 при 1 знаке
    [InlineData(5, 100000)]                                          // 100 000 при 5 знаках
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.03")]
    public void Generate_RejectsNumberOutOfRange(int digits, int number)
    {
        var format = new MarkFormat("П", '-', digits);               // формат с нужным числом знаков

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => MarkGenerator.Generate(format, "01", number)); // тип по ТР-У.03

        Assert.Equal("number", ex.ParamName);                        // какой параметр неверен
    }

    [Theory]
    [InlineData(1, 9, "П-01-9")]                                     // верхняя граница при 1 знаке
    [InlineData(5, 99999, "П-01-99999")]                             // верхняя граница при 5 знаках
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.03")]
    public void Generate_AcceptsUpperBoundNumber(int digits, int number, string expected)
    {
        var format = new MarkFormat("П", '-', digits);               // формат с нужным числом знаков

        string mark = MarkGenerator.Generate(format, "01", number);  // действие

        Assert.Equal(expected, mark);                                // граница включительно: 10^знаков − 1
    }

    // ---------- ТР-У.04: код уровня ----------

    [Theory]
    [InlineData(null)]                                               // нет кода
    [InlineData("")]                                                 // пусто
    [InlineData("ABCDE")]                                            // 5 символов — длиннее 4
    [InlineData("0-1")]                                              // дефис
    [InlineData("0 1")]                                              // пробел
    [InlineData("01.")]                                              // точка
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.04")]
    public void Generate_RejectsInvalidLevelCode(string? level)
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() => MarkGenerator.Generate(MarkFormat.Default, level!, 1)); // точный тип по ТР-У.04

        Assert.Equal("levelCode", ex.ParamName);                     // имя параметра — часть требования
    }

    // ---------- ТР-У.05: разбор марки ----------

    [Theory]
    [InlineData("П-01-003", "01", 3)]                                // ровно как строит Generate
    [InlineData("п-01-003", "01", 3)]                                // строчный префикс
    [InlineData("п-01-0003", "01", 3)]                               // пример из мини-ТЗ: лишний ноль
    [InlineData("П-B1-00120", "B1", 120)]                            // несколько лишних нулей
    [InlineData("П-ABCD-999", "ABCD", 999)]                          // код из 4 символов, максимальный номер
    [InlineData("П-01-000000000000000000001", "01", 1)]              // длинная цепочка нулей не переполняет int
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.05")]
    public void TryParse_ParsesValidMark(string mark, string expectedLevel, int expectedNumber)
    {
        bool ok = MarkGenerator.TryParse(MarkFormat.Default, mark, out string level, out int number); // действие

        Assert.True(ok);                                             // строка распознана
        Assert.Equal(expectedLevel, level);                          // код уровня
        Assert.Equal(expectedNumber, number);                        // номер без лишних нулей
    }

    [Fact]
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.05")]
    public void TryParse_PrefixCaseInsensitive_ForYo()
    {
        var format = new MarkFormat("Ё", '-', 3);                    // префикс с «Ё»

        bool ok = MarkGenerator.TryParse(format, "ё-01-003", out string level, out int number); // строчная «ё»

        Assert.True(ok);                                             // регистр префикса не важен
        Assert.Equal("01", level);                                   // код уровня
        Assert.Equal(3, number);                                     // номер
    }

    [Theory]
    [InlineData(null)]                                               // нет строки
    [InlineData("")]                                                 // пусто
    [InlineData("мусор")]                                            // произвольный текст
    [InlineData("П-01")]                                             // нет номера
    [InlineData("П-01-003-4")]                                       // лишняя часть
    [InlineData("Р-01-003")]                                         // чужой префикс
    [InlineData("P-01-003")]                                         // латинская P вместо кириллической П
    [InlineData("П/01/003")]                                         // чужой разделитель
    [InlineData("П--003")]                                           // пустой код уровня
    [InlineData("П-ABCDE-003")]                                      // код уровня длиннее 4
    [InlineData("П-01-")]                                            // пустой номер
    [InlineData("П-01-3")]                                           // меньше знаков, чем требует формат
    [InlineData("П-01-000")]                                         // номер 0
    [InlineData("П-01-1000")]                                        // 1000 при 3 знаках
    [InlineData("П-01-00a")]                                         // буква в номере
    [InlineData("П-01-+03")]                                         // знак в номере
    [InlineData("П-01- 03")]                                         // пробел в номере
    [InlineData(" П-01-003")]                                        // пробел перед маркой
    [InlineData("П-01-003 ")]                                        // пробел после марки
    [InlineData("П-01-٠٠٣")]                                         // арабско-индийские цифры
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.05")]
    public void TryParse_ReturnsFalseOnGarbage(string? mark)
    {
        bool ok = MarkGenerator.TryParse(MarkFormat.Default, mark!, out string level, out int number); // без исключения

        Assert.False(ok);                                            // строка не марка
        Assert.Equal(string.Empty, level);                           // значение при неудаче — пустая строка
        Assert.Equal(0, number);                                     // значение при неудаче — 0
    }

    [Theory]
    [InlineData("01", 1)]                                            // минимальный номер
    [InlineData("B1", 120)]                                          // буквенно-цифровой код
    [InlineData("Л2", 999)]                                          // кириллический код, максимум
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.05")]
    public void TryParse_IsInverseOfGenerate(string level, int number)
    {
        string mark = MarkGenerator.Generate(MarkFormat.Default, level, number); // строим

        bool ok = MarkGenerator.TryParse(MarkFormat.Default, mark, out string parsedLevel, out int parsedNumber); // разбираем

        Assert.True(ok);                                             // марка распознана
        Assert.Equal(level, parsedLevel);                            // тот же код уровня
        Assert.Equal(number, parsedNumber);                          // тот же номер
    }

    [Fact]
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.05")]
    public void TryParse_RejectsNullFormat()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => MarkGenerator.TryParse(null!, "П-01-003", out _, out _)); // формат — ошибка программиста

        Assert.Equal("format", ex.ParamName);                        // какой параметр неверен
    }
}
