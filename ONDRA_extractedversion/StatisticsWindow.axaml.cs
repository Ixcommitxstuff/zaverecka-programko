// ═══════════════════════════════════════════════════
// COMMIT 1 — 24. 5.  (nejdřív pushni StatisticsWindow.axaml)
// git add Views/StatisticsWindow.axaml
// git commit -m "feat: StatisticsWindow – základní layout a záložky"
//
// COMMIT 2 — 27. 5.  (přehled + průměry)
// git add Views/StatisticsWindow.axaml.cs
// git commit -m "feat: StatisticsWindow – přehled příjmů a výdajů"
//
// COMMIT 4 — 29. 5.  (sloupcový graf)
// git add Views/StatisticsWindow.axaml.cs
// git commit -m "feat: sloupcový graf příjmy vs výdaje"
//
// COMMIT 5 — 31. 5.  (kategorie)
// git add Views/StatisticsWindow.axaml.cs
// git commit -m "feat: výdaje podle kategorie v StatisticsWindow"
// ═══════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using FinanceManager.Models;
using FinanceManager.Services;

namespace FinanceManager.Views;

public partial class StatisticsWindow : Window
{
    private readonly DataService _data = App.DataService;

    public StatisticsWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => BuildStatistics();
    }

    private void BuildStatistics()
    {
        var transactions = _data.Transactions.ToList();

        // ── Summary ──
        var income   = _data.TotalIncome(transactions);
        var expenses = _data.TotalExpenses(transactions);
        var balance  = income - expenses;

        TotalIncomeText.Text   = $"{income:N2} Kč";
        TotalExpensesText.Text = $"{expenses:N2} Kč";
        BalanceText.Text       = $"{balance:N2} Kč";

        if (balance > 0)
        {
            BalanceText.Foreground      = new SolidColorBrush(Color.Parse("#16A34A"));
            BalanceStatusText.Text      = "✅ V plusu";
            BalanceStatusText.Foreground = new SolidColorBrush(Color.Parse("#16A34A"));
        }
        else if (balance < 0)
        {
            BalanceText.Foreground      = new SolidColorBrush(Color.Parse("#DC2626"));
            BalanceStatusText.Text      = "❌ V mínusu";
            BalanceStatusText.Foreground = new SolidColorBrush(Color.Parse("#DC2626"));
        }
        else
        {
            BalanceText.Foreground      = new SolidColorBrush(Color.Parse("#64748B"));
            BalanceStatusText.Text      = "⚖️ Vyrovnáno";
            BalanceStatusText.Foreground = new SolidColorBrush(Color.Parse("#64748B"));
        }

        // ── Monthly table ──
        var monthRows = transactions
            .GroupBy(t => (t.Date.Year, t.Date.Month))
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Select(g =>
            {
                var mInc = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var mExp = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                var mBal = mInc - mExp;
                return new MonthRow
                {
                    MonthLabel  = new DateTime(g.Key.Year, g.Key.Month, 1)
                                      .ToString("MMMM yyyy", new System.Globalization.CultureInfo("cs-CZ")),
                    IncomeText  = $"{mInc:N2} Kč",
                    ExpenseText = $"{mExp:N2} Kč",
                    BalanceText = $"{mBal:N2} Kč",
                    BalanceColor = mBal >= 0
                        ? new SolidColorBrush(Color.Parse("#16A34A"))
                        : new SolidColorBrush(Color.Parse("#DC2626"))
                };
            })
            .ToList();

        MonthlyList.ItemsSource = monthRows;

        // ── Bar chart (Tab 2) ──
        DrawBarChart(monthRows);

        // ── Category breakdown (Tab 3) ──
        BuildCategoryLists(transactions);
    }

    private void DrawBarChart(List<MonthRow> rows)
    {
        BarChart.Children.Clear();
        if (rows.Count == 0) return;

        // We need raw amounts — reconstruct from DataService grouped by month
        var months = _data.Transactions
            .GroupBy(t => (t.Date.Year, t.Date.Month))
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .ToList();

        if (months.Count == 0) return;

        const double canvasH   = 360;
        const double topPad    = 20;
        const double bottomPad = 40;
        const double leftPad   = 60;
        const double rightPad  = 20;

        double maxVal = months.Max(g =>
            Math.Max(g.Where(t => t.Type == TransactionType.Income).Sum(t => (double)t.Amount),
                     g.Where(t => t.Type == TransactionType.Expense).Sum(t => (double)t.Amount)));
        if (maxVal == 0) maxVal = 1;

        double chartW = Math.Max(600, months.Count * 80);
        BarChart.Width = chartW + leftPad + rightPad;

        double groupW  = (chartW - leftPad - rightPad) / months.Count;
        double barW    = Math.Min(groupW * 0.38, 28);
        double chartH  = canvasH - topPad - bottomPad;

        var incomeBrush  = new SolidColorBrush(Color.Parse("#4F8EF7"));
        var expenseBrush = new SolidColorBrush(Color.Parse("#F87171"));
        var gridBrush    = new SolidColorBrush(Color.Parse("#E2E8F0"));
        var textBrush    = new SolidColorBrush(Color.Parse("#475569"));

        // Grid lines (5 levels)
        for (int i = 0; i <= 5; i++)
        {
            double y = topPad + chartH - (chartH * i / 5);
            BarChart.Children.Add(new Line
            {
                StartPoint = new Point(leftPad, y),
                EndPoint   = new Point(chartW + leftPad, y),
                Stroke     = gridBrush,
                StrokeThickness = 1
            });
            BarChart.Children.Add(new TextBlock
            {
                Text       = $"{maxVal * i / 5:N0}",
                FontSize   = 10,
                Foreground = textBrush,
                [Canvas.LeftProperty]   = 2.0,
                [Canvas.TopProperty]    = y - 8
            });
        }

        // Bars
        for (int i = 0; i < months.Count; i++)
        {
            var g       = months[i];
            double inc  = (double)g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            double exp  = (double)g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            double cx   = leftPad + groupW * i + groupW / 2;

            // Income bar
            double incH = chartH * (inc / maxVal);
            BarChart.Children.Add(new Rectangle
            {
                Width  = barW, Height = Math.Max(incH, 1),
                Fill   = incomeBrush, RadiusX = 3, RadiusY = 3,
                [Canvas.LeftProperty]   = cx - barW - 1,
                [Canvas.TopProperty]    = topPad + chartH - incH
            });

            // Expense bar
            double expH = chartH * (exp / maxVal);
            BarChart.Children.Add(new Rectangle
            {
                Width  = barW, Height = Math.Max(expH, 1),
                Fill   = expenseBrush, RadiusX = 3, RadiusY = 3,
                [Canvas.LeftProperty]   = cx + 1,
                [Canvas.TopProperty]    = topPad + chartH - expH
            });

            // Month label
            var label = new DateTime(g.Key.Year, g.Key.Month, 1)
                            .ToString("MMM yy", new System.Globalization.CultureInfo("cs-CZ"));
            BarChart.Children.Add(new TextBlock
            {
                Text       = label,
                FontSize   = 10,
                Foreground = textBrush,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                Width      = groupW,
                [Canvas.LeftProperty] = leftPad + groupW * i,
                [Canvas.TopProperty]  = topPad + chartH + 6
            });
        }
    }

    private void BuildCategoryLists(List<Transaction> transactions)
    {
        decimal maxExp = _data.ByCategory(TransactionType.Expense).Max(g => g.Sum(t => t.Amount));
        if (maxExp == 0) maxExp = 1;

        ExpenseCategoryList.ItemsSource = _data.ByCategory(TransactionType.Expense)
            .OrderByDescending(g => g.Sum(t => t.Amount))
            .Select(g => new CategoryRow
            {
                Category   = g.Key,
                AmountText = $"{g.Sum(t => t.Amount):N2} Kč",
                Percent    = (double)(g.Sum(t => t.Amount) / maxExp * 100)
            }).ToList();

        decimal maxInc = _data.ByCategory(TransactionType.Income).Any()
            ? _data.ByCategory(TransactionType.Income).Max(g => g.Sum(t => t.Amount)) : 1;

        IncomeCategoryList.ItemsSource = _data.ByCategory(TransactionType.Income)
            .OrderByDescending(g => g.Sum(t => t.Amount))
            .Select(g => new CategoryRow
            {
                Category   = g.Key,
                AmountText = $"{g.Sum(t => t.Amount):N2} Kč",
                Percent    = (double)(g.Sum(t => t.Amount) / maxInc * 100)
            }).ToList();
    }

    // ── View models ──────────────────────────────────────────────────────────

    private class MonthRow
    {
        public string MonthLabel  { get; set; } = string.Empty;
        public string IncomeText  { get; set; } = string.Empty;
        public string ExpenseText { get; set; } = string.Empty;
        public string BalanceText { get; set; } = string.Empty;
        public IBrush BalanceColor { get; set; } = Brushes.Black;
    }

    private class CategoryRow
    {
        public string Category   { get; set; } = string.Empty;
        public string AmountText { get; set; } = string.Empty;
        public double Percent    { get; set; }
    }
}
