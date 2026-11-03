using System;                                                        // ArgumentException
using Sandbox.Core;                                                  // PluginInfo
using Xunit;                                                         // Fact, Theory, Trait, Assert

namespace Sandbox.Core.Tests;                                        // тесты ядра

/// <summary>Образец оформления тестов: каждый тест помечен задачей и требованием.</summary>
public class PluginInfoTests
{
    [Fact]                                                           // обычный тест
    [Trait("Task", "BASE")]                                          // задача: исходный каркас
    [Trait("TR", "У.00")]                                            // требование учебного мини-ТЗ
    public void Describe_ContainsTitleVersionAndRevit()
    {
        string text = PluginInfo.Describe("2025");                   // действие
        Assert.Equal("Sandbox 0.1.0 · Revit 2025", text);            // точный ожидаемый текст
    }

    [Theory]                                                         // один тест — несколько входов
    [InlineData("")]                                                 // пустая строка
    [InlineData("   ")]                                              // одни пробелы
    [Trait("Task", "BASE")]
    [Trait("TR", "У.00")]
    public void Describe_RejectsEmptyVersion(string version)
    {
        Assert.Throws<ArgumentException>(() => PluginInfo.Describe(version)); // ошибка, а не пустая версия в окне
    }
}
