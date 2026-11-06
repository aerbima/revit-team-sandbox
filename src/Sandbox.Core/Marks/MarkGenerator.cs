using System;                                                        // ArgumentException, StringComparison
using System.Globalization;                                          // CultureInfo, NumberStyles

namespace Sandbox.Core.Marks;                                        // маркировка помещений

/// <summary>
/// Построение и разбор марок «П-01-003» (ТР-У.01, ТР-У.04, ТР-У.05).
/// КОНТРАКТ TASK-TS-01: от него зависят TS-02 и TS-03, сигнатуры менять нельзя.
/// </summary>
public static class MarkGenerator
{
    /// <summary>Наибольшая длина кода уровня (ТР-У.04).</summary>
    internal const int MaxLevelCodeLength = 4;                       // «от 1 до 4 букв или цифр»

    /// <summary>Строит марку: префикс, код уровня и номер через разделитель.</summary>
    /// <param name="format">Формат марки.</param>
    /// <param name="levelCode">Код уровня: 1–4 буквы или цифры.</param>
    /// <param name="number">Номер помещения на уровне, от 1.</param>
    public static string Generate(MarkFormat format, string levelCode, int number)
    {
        if (format is null)                                          // формат обязателен: ошибка вызывающего кода
        {
            throw new ArgumentNullException(nameof(format));         // стандартное исключение для null
        }

        if (!IsValidLevelCode(levelCode))                            // ТР-У.04
        {
            throw new ArgumentException(                             // тип исключения задан мини-ТЗ
                "Код уровня — от 1 до 4 букв или цифр.",             // понятное сообщение
                nameof(levelCode));                                  // имя параметра «levelCode» проверяют тесты
        }

        if (number < 1 || number > format.MaxNumber)                 // ТР-У.03: 1…10^знаков − 1
        {
            throw new ArgumentOutOfRangeException(                   // 1000 при 3 знаках — ошибка, а не «П-01-1000»
                nameof(number),                                      // имя параметра «number»
                number,                                              // фактическое значение в сообщении
                $"Номер — от 1 до {format.MaxNumber.ToString(CultureInfo.InvariantCulture)}."); // граница из формата
        }

        string digits = number.ToString(                             // номер с ведущими нулями
            "D" + format.Digits.ToString(CultureInfo.InvariantCulture), // «D3» → 3 → «003»
            CultureInfo.InvariantCulture);                           // не зависит от культуры машины

        return format.Prefix                                         // «П»
            + format.Separator                                       // «-»
            + levelCode                                              // «01»
            + format.Separator                                       // «-»
            + digits;                                                // «003» → «П-01-003»
    }

    /// <summary>Разбирает марку обратно; false — строка не является маркой этого формата.</summary>
    /// <param name="format">Формат, по которому разбирать.</param>
    /// <param name="mark">Строка марки.</param>
    /// <param name="levelCode">Код уровня при успехе; пустая строка при неудаче.</param>
    /// <param name="number">Номер при успехе; 0 при неудаче.</param>
    public static bool TryParse(MarkFormat format, string mark, out string levelCode, out int number)
    {
        if (format is null)                                          // формат — ошибка программиста, не «мусорная» строка
        {
            throw new ArgumentNullException(nameof(format));         // ТР-У.05 про строку марки, не про формат
        }

        levelCode = string.Empty;                                    // значения при неудаче (см. документацию)
        number = 0;                                                  // 0 не бывает настоящим номером

        if (mark is null)                                            // null — тоже «не марка»
        {
            return false;                                            // ТР-У.05: false, а не исключение
        }

        string[] parts = mark.Split(format.Separator);               // «П-01-003» → «П», «01», «003»
        if (parts.Length != 3)                                       // частей должно быть ровно три
        {
            return false;                                            // лишний или недостающий разделитель
        }

        if (!string.Equals(parts[0], format.Prefix, StringComparison.OrdinalIgnoreCase)) // «п» = «П», «ё» = «Ё»
        {
            return false;                                            // чужой префикс
        }

        if (!IsValidLevelCode(parts[1]))                             // код уровня по ТР-У.04
        {
            return false;                                            // пусто, длиннее 4 или недопустимые символы
        }

        if (!TryParseNumber(parts[2], format, out int parsed))       // номер с возможными лишними нулями
        {
            return false;                                            // не число или вне диапазона
        }

        levelCode = parts[1];                                        // успех: код уровня как в строке
        number = parsed;                                             // успех: номер
        return true;                                                 // строка — марка этого формата
    }

    /// <summary>Проверка кода уровня по ТР-У.04.</summary>
    private static bool IsValidLevelCode(string? levelCode)
    {
        if (levelCode is null || levelCode.Length == 0 || levelCode.Length > MaxLevelCodeLength) // null, пусто, > 4
        {
            return false;                                            // длина не подходит
        }

        foreach (char c in levelCode)                                // каждый символ кода
        {
            if (!char.IsLetterOrDigit(c))                            // пробел, дефис, точка и т. п.
            {
                return false;                                        // недопустимый символ
            }
        }

        return true;                                                 // код корректен
    }

    /// <summary>
    /// Номер марки: только цифры 0–9, не короче <see cref="MarkFormat.Digits"/> знаков
    /// (лишние ведущие нули допустимы), значение 1…10^знаков − 1.
    /// </summary>
    private static bool TryParseNumber(string text, MarkFormat format, out int value)
    {
        value = 0;                                                   // значение при неудаче

        if (text.Length < format.Digits)                             // «3» вместо «003» — не марка формата
        {
            return false;                                            // недостающие нули не допускаем
        }

        foreach (char c in text)                                     // каждый символ номера
        {
            if (c < '0' || c > '9')                                  // только ASCII-цифры: «٣» и «+» не номер
            {
                return false;                                        // мусор в номере
            }
        }

        string significant = text.TrimStart('0');                    // «0003» → «3»; длинная цепочка нулей не переполнит int
        if (significant.Length == 0 || significant.Length > format.Digits) // «000» = 0 или «1000» при 3 знаках
        {
            return false;                                            // вне диапазона 1…10^знаков − 1
        }

        value = int.Parse(significant, NumberStyles.None, CultureInfo.InvariantCulture); // не длиннее 5 цифр — переполнения нет
        return true;                                                 // номер корректен
    }
}
