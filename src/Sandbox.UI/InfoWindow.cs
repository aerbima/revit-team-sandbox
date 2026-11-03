using System.Windows;                                                // Window, Thickness, TextWrapping
using System.Windows.Controls;                                       // StackPanel, TextBlock, Button

namespace Sandbox.UI;                                                // окна плагина

/// <summary>Простое окно с текстом и кнопкой OK. Собрано в коде, без XAML.</summary>
public sealed class InfoWindow : Window
{
    /// <summary>Создаёт окно.</summary>
    /// <param name="title">Заголовок окна.</param>
    /// <param name="text">Текст сообщения.</param>
    public InfoWindow(string title, string text)
    {
        Title = title;                                               // заголовок
        Width = 420;                                                 // ширина фиксирована
        SizeToContent = SizeToContent.Height;                        // высота — по содержимому
        ResizeMode = ResizeMode.NoResize;                            // размер не меняется
        WindowStartupLocation = WindowStartupLocation.CenterScreen;  // по центру экрана

        var ok = new Button                                          // кнопка закрытия
        {
            Content = "OK",                                          // подпись
            Width = 80,                                              // ширина кнопки
            IsDefault = true,                                        // Enter закрывает окно
            IsCancel = true,                                         // Esc закрывает окно
            HorizontalAlignment = HorizontalAlignment.Right,         // кнопка справа
            Margin = new Thickness(0, 12, 0, 0),                     // отступ от текста
        };
        ok.Click += (_, _) => Close();                               // закрыть по нажатию

        var panel = new StackPanel { Margin = new Thickness(16) };   // вертикальная компоновка с полями
        panel.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap }); // текст с переносом
        panel.Children.Add(ok);                                      // кнопка под текстом
        Content = panel;                                             // содержимое окна
    }
}
