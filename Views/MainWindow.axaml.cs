// ═══════════════════════════════════════════════════
// COMMIT 1 — 22. 5.  (nejdřív pushni MainWindow.axaml)
// git add Views/MainWindow.axaml
// git commit -m "feat: MainWindow – základní layout a topbar"
//
// COMMIT 4 — 29. 5.  (po přidání logiky seznamu + zůstatku)
// git add Views/MainWindow.axaml.cs
// git commit -m "feat: MainWindow – seznam transakcí a zůstatek"
//
// COMMIT 6 — 31. 5.  (po přidání filtrů)
// git add Views/MainWindow.axaml.cs
// git commit -m "feat: filtrování podle kategorie a měny v MainWindow"
// ═══════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FinanceManager.Models;
using FinanceManager.Services;

namespace FinanceManager.Views;

public partial class MainWindow : Window
{
    private readonly DataService _data = App.DataService;
    private readonly CategoryService _categories = App.CategoryService;
    private readonly ExportService _export = App.ExportService;

    private string? _selectedCategory;
    private (int Year, int Month)? _selectedMonth;

    public MainWindow()
    {
        InitializeComponent();
        RefreshAll();
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        RefreshFilters();
        RefreshList();
        RefreshSummary();
    }

    private void RefreshFilters()
    {
        // Category filter
        var cats = new List<string> { "— všechny kategorie —" };
        cats.AddRange(_categories.Categories.OrderBy(c => c));
        CategoryFilter.ItemsSource = cats;
        CategoryFilter.SelectedIndex = 0;

        // Month filter
        var months = new List<string> { "— všechny měsíce —" };
        var monthKeys = _data.Transactions
            .Select(t => (t.Date.Year, t.Date.Month))
            .Distinct()
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.Month)
            .Select(m => new DateTime(m.Year, m.Month, 1)
                             .ToString("MMMM yyyy", new System.Globalization.CultureInfo("cs-CZ")))
            .ToList();
        months.AddRange(monthKeys);
        MonthFilter.ItemsSource = months;
        MonthFilter.SelectedIndex = 0;
    }

    private IEnumerable<Transaction> GetFiltered()
    {
        var source = _data.Transactions.AsEnumerable();
        if (_selectedCategory != null)
            source = source.Where(t => t.Category == _selectedCategory);
        if (_selectedMonth.HasValue)
            source = source.Where(t => t.Date.Year == _selectedMonth.Value.Year
                                    && t.Date.Month == _selectedMonth.Value.Month);
        return source.OrderByDescending(t => t.Date);
    }

    private void RefreshList()
    {
        TransactionList.ItemsSource = null;
        TransactionList.ItemsSource = GetFiltered().ToList();
    }

    private void RefreshSummary()
    {
        var filtered = GetFiltered().ToList();
        var income   = _data.TotalIncome(filtered);
        var expenses = _data.TotalExpenses(filtered);
        var balance  = income - expenses;

        IncomeText.Text  = $"{income:N2} Kč";
        ExpenseText.Text = $"{expenses:N2} Kč";
        BalanceText.Text = $"{balance:N2} Kč";

        if (balance > 0)
        {
            BalanceText.Foreground = new SolidColorBrush(Color.Parse("#16A34A"));
            BalanceLabel.Text = "✅ V plusu";
            BalanceLabel.Foreground = new SolidColorBrush(Color.Parse("#16A34A"));
        }
        else if (balance < 0)
        {
            BalanceText.Foreground = new SolidColorBrush(Color.Parse("#DC2626"));
            BalanceLabel.Text = "❌ V mínusu";
            BalanceLabel.Foreground = new SolidColorBrush(Color.Parse("#DC2626"));
        }
        else
        {
            BalanceText.Foreground = new SolidColorBrush(Color.Parse("#64748B"));
            BalanceLabel.Text = "⚖️ Vyrovnáno";
            BalanceLabel.Foreground = new SolidColorBrush(Color.Parse("#64748B"));
        }
    }

    // ── Toolbar handlers ─────────────────────────────────────────────────────

    private async void OnAddTransaction(object? sender, RoutedEventArgs e)
    {
        var win = new AddTransactionWindow();
        await win.ShowDialog(this);
        RefreshAll();
    }

    private async void OnOpenStatistics(object? sender, RoutedEventArgs e)
    {
        var win = new StatisticsWindow();
        await win.ShowDialog(this);
    }

    private async void OnManageCategories(object? sender, RoutedEventArgs e)
    {
        var win = new ManageCategoriesWindow();
        await win.ShowDialog(this);
        RefreshAll(); // categories may have changed
    }

    private async void OnExport(object? sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Uložit přehled",
            DefaultExtension = "txt",
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Text files", Extensions = { "txt" } }
            },
            InitialFileName = $"prehled_{DateTime.Now:yyyyMMdd}.txt"
        };

        var path = await dialog.ShowAsync(this);
        if (path != null)
        {
            _export.ExportOverview(path, GetFiltered());
            await MessageBox.Show(this, "Přehled byl exportován.", "Export dokončen");
        }
    }

    private async void OnImport(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Načíst data z TXT",
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Text files", Extensions = { "txt" } }
            },
            AllowMultiple = false
        };

        var files = await dialog.ShowAsync(this);
        if (files?.Length > 0)
        {
            try
            {
                var (imported, skipped) = _data.ImportFromFile(files[0]);
                await MessageBox.Show(this,
                    $"Importováno: {imported} záznamů\nPřeskočeno (duplicity/chyby): {skipped}",
                    "Import dokončen");
                RefreshAll();
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Chyba při importu:\n{ex.Message}", "Chyba");
            }
        }
    }

    // ── Filter handlers ───────────────────────────────────────────────────────

    private void OnCategoryFilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        var sel = CategoryFilter.SelectedItem as string;
        _selectedCategory = (sel == null || sel.StartsWith("—")) ? null : sel;
        RefreshList();
        RefreshSummary();
    }

    private void OnClearFilter(object? sender, RoutedEventArgs e)
    {
        _selectedCategory = null;
        CategoryFilter.SelectedIndex = 0;
        RefreshList();
        RefreshSummary();
    }

    private void OnMonthFilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        var sel = MonthFilter.SelectedItem as string;
        if (sel == null || sel.StartsWith("—"))
        {
            _selectedMonth = null;
        }
        else
        {
            // Parse "MMMM yyyy"
            if (DateTime.TryParseExact(sel, "MMMM yyyy",
                    new System.Globalization.CultureInfo("cs-CZ"),
                    System.Globalization.DateTimeStyles.None, out var dt))
                _selectedMonth = (dt.Year, dt.Month);
            else
                _selectedMonth = null;
        }
        RefreshList();
        RefreshSummary();
    }

    private void OnClearMonthFilter(object? sender, RoutedEventArgs e)
    {
        _selectedMonth = null;
        MonthFilter.SelectedIndex = 0;
        RefreshList();
        RefreshSummary();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    private void OnDeleteTransaction(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            _data.RemoveTransaction(id);
            RefreshAll();
        }
    }
}
