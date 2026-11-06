using System;                                                        // ArgumentException, ArgumentOutOfRangeException

namespace Sandbox.Core.Marks;                                        // маркировка помещений (учебное мини-ТЗ, раздел 1)

/// <summary>
/// Формат марки помещения: префикс, разделитель, число знаков номера (ТР-У.01…У.03).
/// КОНТРАКТ TASK-TS-01: сигнатуры менять нельзя.
/// </summary>
public sealed class MarkFormat
{
    /// <summary>Наименьшее допустимое число знаков номера (ТР-У.03).</summary>
    internal const int MinDigits = 1;                                // «от 1»

    /// <summary>Наибольшее допустимое число знаков номера (ТР-У.03).</summary>
    internal const int MaxDigits = 5;                                // «до 5»: 99 999 помещается в int

    /// <summary>Наибольшая длина префикса (ТР-У.02).</summary>
    internal const int MaxPrefixLength = 3;                          // «от 1 до 3 заглавных букв»

    /// <summary>Создаёт формат и проверяет значения (ТР-У.02, ТР-У.03).</summary>
    /// <param name="prefix">Префикс: 1–3 заглавные буквы.</param>
    /// <param name="separator">Разделитель частей марки.</param>
    /// <param name="digits">Число знаков номера: 1–5.</param>
    public MarkFormat(string prefix, char separator, int digits)
    {
        if (!IsValidPrefix(prefix))                                  // ТР-У.02: длина и состав префикса
        {
            throw new ArgumentException(                             // тип исключения задан мини-ТЗ
                "Префикс — от 1 до 3 заглавных букв кириллицы или латиницы.", // понятное сообщение
                nameof(prefix));                                     // имя параметра «prefix» проверяют тесты
        }

        if (char.IsLetterOrDigit(separator) || char.IsWhiteSpace(separator)) // ТР-У.03: буква, цифра или пробел
        {
            throw new ArgumentException(                             // разделитель недопустим
                "Разделитель не может быть буквой, цифрой или пробелом.", // иначе марку не разобрать однозначно
                nameof(separator));                                  // имя параметра «separator»
        }

        if (digits < MinDigits || digits > MaxDigits)                // ТР-У.03: 1…5 знаков
        {
            throw new ArgumentOutOfRangeException(                   // вне диапазона — свой тип исключения
                nameof(digits),                                      // имя параметра «digits»
                digits,                                              // фактическое значение попадёт в сообщение
                "Число знаков номера — от 1 до 5.");                 // понятное сообщение
        }

        Prefix = prefix;                                             // проверки пройдены — сохраняем
        Separator = separator;                                       // разделитель
        Digits = digits;                                             // число знаков
        MaxNumber = Pow10(digits) - 1;                               // 3 знака → 999 (ТР-У.03: 10^знаков − 1)
    }

    /// <summary>Формат по умолчанию: «П», «-», 3 знака (мини-ТЗ, последний раздел).</summary>
    public static MarkFormat Default => new MarkFormat("П", '-', 3); // новый объект при каждом обращении

    /// <summary>Префикс марки, например «П».</summary>
    public string Prefix { get; }                                    // значение задаёт конструктор

    /// <summary>Разделитель частей, например «-».</summary>
    public char Separator { get; }                                   // значение задаёт конструктор

    /// <summary>Число знаков номера с ведущими нулями.</summary>
    public int Digits { get; }                                       // значение задаёт конструктор

    /// <summary>Наибольший номер для этого формата: 10^Digits − 1.</summary>
    internal int MaxNumber { get; }                                  // нужен генератору и разбору; не часть контракта

    /// <summary>Проверка префикса по ТР-У.02.</summary>
    private static bool IsValidPrefix(string? prefix)
    {
        if (prefix is null || prefix.Length == 0 || prefix.Length > MaxPrefixLength) // null, пусто или длиннее 3
        {
            return false;                                            // длина не подходит
        }

        foreach (char c in prefix)                                   // каждый символ префикса
        {
            if (!char.IsLetter(c) || !char.IsUpper(c) || !IsLatinOrCyrillic(c)) // заглавная буква нужного алфавита
            {
                return false;                                        // цифра, строчная, греческая и т. п.
            }
        }

        return true;                                                 // все символы подходят
    }

    /// <summary>Латиница A–Z или блок кириллицы Юникода (в него входит «Ё», в отличие от диапазона «А–Я»).</summary>
    private static bool IsLatinOrCyrillic(char c) =>
        (c >= 'A' && c <= 'Z')                                       // латиница
        || (c >= 'Ѐ' && c <= 'ӿ');                         // кириллица: Ё = U+0401, Ї, Ў и др.

    /// <summary>10 в степени n для n = 1…5 без double и Math.Pow.</summary>
    private static int Pow10(int n)
    {
        int result = 1;                                              // 10^0
        for (int i = 0; i < n; i++)                                  // n раз
        {
            result *= 10;                                            // умножаем на 10
        }

        return result;                                               // 10^n
    }
}
