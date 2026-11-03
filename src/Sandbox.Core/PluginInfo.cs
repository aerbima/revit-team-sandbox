using System;                                                        // ArgumentException

namespace Sandbox.Core;                                              // ядро: без Revit API и WPF

/// <summary>Сведения о плагине для окна «О плагине».</summary>
public static class PluginInfo
{
    /// <summary>Имя вкладки ленты и заголовок окон.</summary>
    public const string Title = "Sandbox";                           // учебное имя, без брендов заказчика

    /// <summary>Версия плагина; меняется только в PR с обновлением версии.</summary>
    public const string Version = "0.1.0";                           // семантическая версия

    /// <summary>Текст окна «О плагине» для указанной версии Revit.</summary>
    /// <param name="revitVersion">Номер версии Revit, например «2025».</param>
    public static string Describe(string revitVersion)
    {
        if (string.IsNullOrWhiteSpace(revitVersion))                 // версия обязательна
        {
            throw new ArgumentException("Не указана версия Revit.", nameof(revitVersion)); // понятная ошибка вызывающему
        }

        return $"{Title} {Version} · Revit {revitVersion.Trim()}";   // «Sandbox 0.1.0 · Revit 2025»
    }
}
