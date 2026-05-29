// ═══════════════════════════════════════════════════
// COMMIT 3 — 28. 5.
// git add Views/ManageCategoriesWindow.axaml Views/ManageCategoriesWindow.axaml.cs
// git commit -m "feat: ManageCategoriesWindow – přidávání a mazání kategorií"
// ═══════════════════════════════════════════════════

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FinanceManager.Services;

namespace FinanceManager.Views;

public partial class ManageCategoriesWindow : Window
{
    private readonly CategoryService _categories = App.CategoryService;

    public ManageCategoriesWindow()
    {
        InitializeComponent();
        Refresh();
    }

    private void Refresh()
    {
        CategoryList.ItemsSource = null;
        CategoryList.ItemsSource = _categories.Categories;
    }

    private void OnAddCategory(object? sender, RoutedEventArgs e)
    {
        var name = NewCategoryBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            ShowStatus("Název kategorie nesmí být prázdný.", isError: true);
            return;
        }

        if (_categories.AddCategory(name))
        {
            NewCategoryBox.Text = string.Empty;
            ShowStatus($"Kategorie „{name}" přidána.", isError: false);
            Refresh();
        }
        else
        {
            ShowStatus($"Kategorie „{name}" již existuje.", isError: true);
        }
    }

    private void OnRemoveCategory(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string name)
        {
            _categories.RemoveCategory(name);
            Refresh();
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusText.Text       = message;
        StatusText.Foreground = isError
            ? new SolidColorBrush(Avalonia.Media.Color.Parse("#DC2626"))
            : new SolidColorBrush(Avalonia.Media.Color.Parse("#16A34A"));
        StatusText.IsVisible  = true;
    }
}
