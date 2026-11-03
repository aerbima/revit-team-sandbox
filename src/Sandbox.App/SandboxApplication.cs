using System.Reflection;                                             // путь к собственной сборке
using Autodesk.Revit.UI;                                             // IExternalApplication, RibbonPanel, PushButtonData
using Sandbox.App.Commands;                                          // команды плагина
using Sandbox.Core;                                                  // PluginInfo

namespace Sandbox.App;                                               // точка входа

/// <summary>Точка входа плагина: создаёт вкладку и панель на ленте Revit.</summary>
public sealed class SandboxApplication : IExternalApplication
{
    /// <summary>Имя панели на вкладке.</summary>
    public const string PanelName = "Учебная";                       // TS-03 добавит сюда кнопку «Марки помещений»

    /// <inheritdoc />
    public Result OnStartup(UIControlledApplication application)
    {
        application.CreateRibbonTab(PluginInfo.Title);               // вкладка «Sandbox»
        RibbonPanel panel = application.CreateRibbonPanel(PluginInfo.Title, PanelName); // панель на вкладке
        string assembly = Assembly.GetExecutingAssembly().Location;  // путь к Sandbox.App.dll

        var about = new PushButtonData(                              // описание кнопки
            "Sandbox.About",                                         // внутреннее имя, уникальное в панели
            "О плагине",                                             // подпись на ленте
            assembly,                                                // сборка с командой
            typeof(AboutCommand).FullName!);                         // полное имя класса команды; у типа оно всегда есть
        panel.AddItem(about);                                        // кнопка на панели

        return Result.Succeeded;                                     // плагин загружен
    }

    /// <inheritdoc />
    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded; // освобождать нечего
}
