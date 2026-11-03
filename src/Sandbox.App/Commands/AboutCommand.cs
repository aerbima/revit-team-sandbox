using Autodesk.Revit.Attributes;                                     // Transaction, TransactionMode
using Autodesk.Revit.DB;                                             // ElementSet
using Autodesk.Revit.UI;                                             // IExternalCommand, ExternalCommandData, Result
using Sandbox.Core;                                                  // PluginInfo
using Sandbox.UI;                                                    // InfoWindow

namespace Sandbox.App.Commands;                                      // команды ленты

/// <summary>Кнопка «О плагине»: показывает версию плагина и Revit.</summary>
[Transaction(TransactionMode.ReadOnly)]                              // документ не меняется
public sealed class AboutCommand : IExternalCommand
{
    /// <inheritdoc />
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        string revit = commandData.Application.Application.VersionNumber; // «2025»
        var window = new InfoWindow(PluginInfo.Title, PluginInfo.Describe(revit)); // окно из сборки UI
        window.ShowDialog();                                         // модально, до нажатия OK
        return Result.Succeeded;                                     // команда выполнена
    }
}
