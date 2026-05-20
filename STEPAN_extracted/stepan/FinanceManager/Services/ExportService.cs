// ═══════════════════════════════════════════════════
// COMMIT 4 — 28. 5.
// git add Services/ExportService.cs
// git commit -m "feat: ExportService – přehled do TXT"
// ═══════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FinanceManager.Models;

namespace FinanceManager.Services;

public class ExportService
{
    private readonly DataService _dataService;

    public ExportService(DataService dataService) => _dataService = dataService;

    /// <summary>Exports a human-readable TXT overview to <paramref name="path"/>.</summary>
    public void ExportOverview(string path, IEnumerable<Transaction>? transactions = null)
    {
        var list = (transactions ?? _dataService.Transactions).ToList();
        var sb = new StringBuilder();

        sb.AppendLine("=================================================");
        sb.AppendLine("         SPRÁVCE OSOBNÍCH FINANCÍ – PŘEHLED      ");
        sb.AppendLine($"         Vytvořeno: {DateTime.Now:dd.MM.yyyy HH:mm}");
        sb.AppendLine("=================================================");
        sb.AppendLine();

        // Summary
        var income   = _dataService.TotalIncome(list);
        var expenses = _dataService.TotalExpenses(list);
        var balance  = income - expenses;

        sb.AppendLine("--- SOUHRN ---");
        sb.AppendLine($"Celkové příjmy:  {income,12:N2} Kč");
        sb.AppendLine($"Celkové výdaje:  {expenses,12:N2} Kč");
        sb.AppendLine($"Zůstatek:        {balance,12:N2} Kč  ({(balance >= 0 ? "V PLUSU ✓" : "V MÍNUSU ✗")})");
        sb.AppendLine();

        // By month
        var byMonth = list.GroupBy(t => (t.Date.Year, t.Date.Month))
                          .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month);

        foreach (var monthGroup in byMonth)
        {
            var monthName = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1)
                                .ToString("MMMM yyyy", new System.Globalization.CultureInfo("cs-CZ"));
            sb.AppendLine($"--- {monthName.ToUpper()} ---");

            foreach (var t in monthGroup.OrderByDescending(x => x.Date))
            {
                var sign = t.Type == TransactionType.Income ? "+" : "-";
                sb.AppendLine($"  [{t.Date:dd.MM}] {t.Description,-30} {sign}{t.Amount,10:N2} Kč  [{t.Category}]");
            }

            var mIncome   = monthGroup.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var mExpenses = monthGroup.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            sb.AppendLine($"  Příjmy: {mIncome:N2} Kč | Výdaje: {mExpenses:N2} Kč | Bilance: {mIncome - mExpenses:N2} Kč");
            sb.AppendLine();
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>Returns the default AppData export path.</summary>
    public string DefaultExportPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FinanceManager");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"prehled_{DateTime.Now:yyyyMMdd_HHmm}.txt");
    }
}
