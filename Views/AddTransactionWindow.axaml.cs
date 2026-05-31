// ═══════════════════════════════════════════════════
// COMMIT 3 — 27. 5.
// git add Views/AddTransactionWindow.axaml Views/AddTransactionWindow.axaml.cs
// git commit -m "feat: AddTransactionWindow + validace vstupů"
// ═══════════════════════════════════════════════════

using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FinanceManager.Models;
using FinanceManager.Services;

namespace FinanceManager.Views;

public partial class AddTransactionWindow : Window
{
    private readonly DataService _data = App.DataService;
    private readonly CategoryService _categories = App.CategoryService;

    public AddTransactionWindow()
    {
        InitializeComponent();
        CategoryBox.ItemsSource = _categories.Categories.OrderBy(c => c).ToList();
        CategoryBox.SelectedIndex = 0;
        DatePicker.SelectedDate = DateTime.Today;
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        ErrorText.IsVisible = false;

        // Validation
        if (!decimal.TryParse(AmountBox.Text?.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ErrorText.Text = "Zadejte platnou kladnou částku.";
            ErrorText.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
        {
            ErrorText.Text = "Popis nesmí být prázdný.";
            ErrorText.IsVisible = true;
            return;
        }

        var type = IncomeRadio.IsChecked == true ? TransactionType.Income : TransactionType.Expense;
        var category = CategoryBox.SelectedItem as string ?? "Ostatní";
        var date = DatePicker.SelectedDate?.DateTime ?? DateTime.Today;

        _data.AddTransaction(new Transaction
        {
            Type = type,
            Amount = amount,
            Description = DescriptionBox.Text.Trim(),
            Category = category,
            Date = date
        });

        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close();
}
