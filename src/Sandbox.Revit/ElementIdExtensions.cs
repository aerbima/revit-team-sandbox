using Autodesk.Revit.DB;                                             // ElementId

namespace Sandbox.Revit;                                             // слой Revit: только здесь допустимы #if

/// <summary>Различия Revit API между версиями собраны в одном месте.</summary>
public static class ElementIdExtensions
{
    /// <summary>Числовое значение ElementId одинаково для 2021–2025.</summary>
    /// <param name="id">Идентификатор элемента.</param>
    public static long GetValue(this ElementId id)
    {
#if REVIT2024 || REVIT2025
        return id.Value;                                             // с Revit 2024 ElementId 64-битный, свойство Value
#else
        return id.IntegerValue;                                      // Revit 2021–2023: 32-битный IntegerValue
#endif
    }
}
