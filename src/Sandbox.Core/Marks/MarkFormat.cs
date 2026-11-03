using System.Diagnostics.CodeAnalysis;                               // ExcludeFromCodeCoverage

namespace Sandbox.Core.Marks;                                        // маркировка помещений (учебное мини-ТЗ, раздел 1)

/// <summary>
/// Формат марки помещения: префикс, разделитель, число знаков номера (ТР-У.01…У.03).
/// КОНТРАКТ TASK-TS-01: сигнатуры менять нельзя, реализацию пишет исполнитель TS-01.
/// </summary>
[ExcludeFromCodeCoverage]                                            // заглушка контракта; исполнитель TS-01 убирает атрибут
public sealed class MarkFormat
{
    /// <summary>Создаёт формат и проверяет значения (ТР-У.02, ТР-У.03).</summary>
    /// <param name="prefix">Префикс: 1–3 заглавные буквы.</param>
    /// <param name="separator">Разделитель частей марки.</param>
    /// <param name="digits">Число знаков номера: 1–5.</param>
    public MarkFormat(string prefix, char separator, int digits)
    {
        throw new System.NotImplementedException("TASK-TS-01");      // заглушка: сборка проходит, вызов — явная ошибка
    }

    /// <summary>Формат по умолчанию: «П», «-», 3 знака (мини-ТЗ, последний раздел).</summary>
    public static MarkFormat Default => new MarkFormat("П", '-', 3); // новый объект при каждом обращении

    /// <summary>Префикс марки, например «П».</summary>
    public string Prefix { get; } = string.Empty;                    // значение задаёт конструктор

    /// <summary>Разделитель частей, например «-».</summary>
    public char Separator { get; }                                   // значение задаёт конструктор

    /// <summary>Число знаков номера с ведущими нулями.</summary>
    public int Digits { get; }                                       // значение задаёт конструктор
}
