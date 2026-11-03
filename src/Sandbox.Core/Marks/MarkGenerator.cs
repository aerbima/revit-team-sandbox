using System.Diagnostics.CodeAnalysis;                               // ExcludeFromCodeCoverage

namespace Sandbox.Core.Marks;                                        // маркировка помещений

/// <summary>
/// Построение и разбор марок «П-01-003» (ТР-У.01, ТР-У.04, ТР-У.05).
/// КОНТРАКТ TASK-TS-01: от него зависят TS-02 и TS-03, сигнатуры менять нельзя.
/// </summary>
[ExcludeFromCodeCoverage]                                            // заглушка контракта; исполнитель TS-01 убирает атрибут
public static class MarkGenerator
{
    /// <summary>Строит марку: префикс, код уровня и номер через разделитель.</summary>
    /// <param name="format">Формат марки.</param>
    /// <param name="levelCode">Код уровня: 1–4 буквы или цифры.</param>
    /// <param name="number">Номер помещения на уровне, от 1.</param>
    public static string Generate(MarkFormat format, string levelCode, int number) =>
        throw new System.NotImplementedException("TASK-TS-01");      // реализует TS-01

    /// <summary>Разбирает марку обратно; false — строка не является маркой этого формата.</summary>
    /// <param name="format">Формат, по которому разбирать.</param>
    /// <param name="mark">Строка марки.</param>
    /// <param name="levelCode">Код уровня при успехе; пустая строка при неудаче.</param>
    /// <param name="number">Номер при успехе; 0 при неудаче.</param>
    public static bool TryParse(MarkFormat format, string mark, out string levelCode, out int number) =>
        throw new System.NotImplementedException("TASK-TS-01");      // реализует TS-01
}
