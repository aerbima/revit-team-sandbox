using System;                                                        // ArgumentException, ArgumentOutOfRangeException
using Sandbox.Core.Marks;                                            // MarkFormat
using Xunit;                                                         // Fact, Theory, Trait, Assert

namespace Sandbox.Core.Tests.Marks;                                  // тесты маркировки

/// <summary>Формат марки: значения по умолчанию и проверки конструктора (ТР-У.02, ТР-У.03).</summary>
public class MarkFormatTests
{
    [Fact]
    [Trait("Task", "TS-01")]                                         // задача
    [Trait("TR", "У.01")]                                            // формат по умолчанию из мини-ТЗ
    public void Default_IsPrefixDashThreeDigits()
    {
        MarkFormat format = MarkFormat.Default;                      // действие

        Assert.Equal("П", format.Prefix);                            // кириллическая «П»
        Assert.Equal('-', format.Separator);                         // дефис
        Assert.Equal(3, format.Digits);                              // три знака
    }

    [Theory]
    [InlineData("П")]                                                // одна кириллическая буква
    [InlineData("AB")]                                               // латиница
    [InlineData("ЁЖЩ")]                                              // «Ё» — вне диапазона «А–Я», но допустима
    [InlineData("ПA")]                                               // смесь кириллицы и латиницы
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.02")]                                            // префикс: 1–3 заглавные буквы
    public void Ctor_AcceptsValidPrefix(string prefix)
    {
        var format = new MarkFormat(prefix, '-', 3);                 // действие: исключения быть не должно

        Assert.Equal(prefix, format.Prefix);                         // префикс сохранён как есть
    }

    [Theory]
    [InlineData(null)]                                               // нет префикса
    [InlineData("")]                                                 // пусто
    [InlineData("ПОМЕ")]                                             // 4 буквы — длиннее 3
    [InlineData("п")]                                                // строчная
    [InlineData("Пп")]                                               // одна из букв строчная
    [InlineData("П1")]                                               // цифра
    [InlineData("П ")]                                               // пробел
    [InlineData("Ω")]                                                // заглавная, но греческая
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.02")]
    public void Ctor_RejectsInvalidPrefix(string? prefix)
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() => new MarkFormat(prefix!, '-', 3)); // точный тип по ТР-У.02

        Assert.Equal("prefix", ex.ParamName);                        // имя параметра — часть требования
    }

    [Theory]
    [InlineData(1)]                                                  // нижняя граница
    [InlineData(5)]                                                  // верхняя граница
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.03")]                                            // число знаков 1–5
    public void Ctor_AcceptsDigitsInRange(int digits)
    {
        var format = new MarkFormat("П", '-', digits);               // действие

        Assert.Equal(digits, format.Digits);                         // значение сохранено
    }

    [Theory]
    [InlineData(0)]                                                  // меньше 1
    [InlineData(6)]                                                  // больше 5
    [InlineData(-1)]                                                 // отрицательное
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.03")]
    public void Ctor_RejectsDigitsOutOfRange(int digits)
    {
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => new MarkFormat("П", '-', digits)); // тип по ТР-У.03

        Assert.Equal("digits", ex.ParamName);                        // какой параметр неверен
    }

    [Theory]
    [InlineData('-')]                                                // дефис
    [InlineData('/')]                                                // косая черта
    [InlineData('.')]                                                // точка
    [InlineData('_')]                                                // подчёркивание
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.03")]                                            // разделитель — не буква, не цифра, не пробел
    public void Ctor_AcceptsSeparator(char separator)
    {
        var format = new MarkFormat("П", separator, 3);              // действие

        Assert.Equal(separator, format.Separator);                   // значение сохранено
    }

    [Theory]
    [InlineData('A')]                                                // латинская буква
    [InlineData('п')]                                                // кириллическая буква
    [InlineData('0')]                                                // цифра
    [InlineData(' ')]                                                // пробел
    [InlineData('\t')]                                               // табуляция — тоже пробельный символ
    [Trait("Task", "TS-01")]
    [Trait("TR", "У.03")]
    public void Ctor_RejectsInvalidSeparator(char separator)
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() => new MarkFormat("П", separator, 3)); // тип по ТР-У.03

        Assert.Equal("separator", ex.ParamName);                     // какой параметр неверен
    }
}
