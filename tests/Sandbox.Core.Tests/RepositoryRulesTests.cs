using System;                                                        // AppContext, StringComparison
using System.IO;                                                     // Directory, File, Path
using System.Linq;                                                   // Where, Select
using Xunit;                                                         // Fact, Trait, Assert

namespace Sandbox.Core.Tests;                                        // тесты ядра

/// <summary>Правила репозитория, которые дешевле проверить тестом, чем глазами на ревью.</summary>
public class RepositoryRulesTests
{
    [Fact]
    [Trait("Task", "BASE")]
    [Trait("TR", "У.12")]                                            // #if только в Sandbox.Revit
    public void ConditionalDirectives_OnlyInRevitProject()
    {
        string src = Path.Combine(FindRoot(), "src");                // каталог исходников
        string[] offenders = Directory                               // все .cs вне Sandbox.Revit с директивой #if
            .GetFiles(src, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)) // без сгенерированных файлов
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}Sandbox.Revit{Path.DirectorySeparatorChar}", StringComparison.Ordinal)) // слой Revit можно
            .Where(f => File.ReadLines(f).Any(l => l.TrimStart().StartsWith("#if", StringComparison.Ordinal))) // есть #if
            .Select(f => Path.GetRelativePath(src, f))               // короткий путь для сообщения
            .ToArray();
        Assert.True(offenders.Length == 0, "Директивы #if вне Sandbox.Revit: " + string.Join(", ", offenders)); // список нарушителей в отчёте
    }

    private static string FindRoot()                                 // корень репозитория — там, где лежит Sandbox.sln
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);       // каталог сборки тестов
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Sandbox.sln"))) // поднимаемся вверх
        {
            dir = dir.Parent;                                        // на уровень выше
        }

        return dir?.FullName ?? throw new InvalidOperationException("Не найден Sandbox.sln выше " + AppContext.BaseDirectory); // понятная ошибка
    }
}
