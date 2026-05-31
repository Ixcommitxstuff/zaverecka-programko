// ═══════════════════════════════════════════════════
// COMMIT 7 — 1. 6. (odevzdání)
// git add Views/MessageBox.cs
// git commit -m "fix: MessageBox + finální úpravy MainWindow"
// ═══════════════════════════════════════════════════

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace FinanceManager.Views;

/// <summary>Simple reusable message dialog.</summary>
public class MessageBox : Window
{
    private MessageBox(string message, string title)
    {
        Title  = title;
        Width  = 360;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.Parse("#F1F5F9"));

        var btn = new Button
        {
            Content             = "OK",
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding             = new Thickness(20, 8),
            Margin              = new Thickness(0, 12, 0, 0)
        };
        btn.Classes.Add("primary");
        btn.Click += (_, _) => Close();

        Content = new Border
        {
            Padding = new Thickness(24),
            Child   = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text        = message,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize    = 14,
                        Foreground  = new SolidColorBrush(Color.Parse("#1E293B"))
                    },
                    btn
                }
            }
        };
    }

    public static Task Show(Window owner, string message, string title = "Informace")
    {
        var box = new MessageBox(message, title);
        return box.ShowDialog(owner);
    }
}
