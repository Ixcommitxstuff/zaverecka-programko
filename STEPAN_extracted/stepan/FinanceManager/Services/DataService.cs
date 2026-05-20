// ═══════════════════════════════════════════════════
// COMMIT 3 — 26. 5.
// git add Services/DataService.cs
// git commit -m "feat: DataService – CRUD operace s transakcemi"
//
// COMMIT 5 — 30. 5. (po přidání ImportFromFile funkce)
// git add Services/DataService.cs
// git commit -m "feat: ImportFromFile – načtení transakcí z TXT"
// ═══════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FinanceManager.Models;

namespace FinanceManager.Services;

public class DataService
{
    private readonly string _filePath;
    private readonly CategoryService _categoryService;
    private readonly List<Transaction> _transactions = new();

    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    public DataService(CategoryService categoryService)
    {
        _categoryService = categoryService;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "FinanceManager");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "transactions.txt");

        // Create empty file if missing
        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, string.Empty);

        Load();
    }

    // ── CRUD ──────────────────────────────────────────────────────────────────

    public void AddTransaction(Transaction t)
    {
        _transactions.Add(t);
        Save();
    }

    public void RemoveTransaction(Guid id)
    {
        _transactions.RemoveAll(t => t.Id == id);
        Save();
    }

    // ── Persistence ──────────────────────────────────────────────────────────

    private void Load()
    {
        _transactions.Clear();
        foreach (var line in File.ReadAllLines(_filePath))
        {
            var t = Transaction.Deserialize(line);
            if (t != null) _transactions.Add(t);
        }
    }

    private void Save()
    {
        File.WriteAllLines(_filePath, _transactions.Select(t => t.Serialize()));
    }

    /// <summary>Imports transactions from an external .txt file.</summary>
    public (int imported, int skipped) ImportFromFile(string path)
    {
        int imported = 0, skipped = 0;
        var existingIds = new HashSet<Guid>(_transactions.Select(t => t.Id));

        foreach (var line in File.ReadAllLines(path))
        {
            var t = Transaction.Deserialize(line);
            if (t == null) { skipped++; continue; }
            if (existingIds.Contains(t.Id)) { skipped++; continue; }
            _transactions.Add(t);
            existingIds.Add(t.Id);
            imported++;
        }
        if (imported > 0) Save();
        return (imported, skipped);
    }

    // ── Statistics helpers ───────────────────────────────────────────────────

    public decimal TotalIncome(IEnumerable<Transaction>? source = null) =>
        (source ?? _transactions).Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);

    public decimal TotalExpenses(IEnumerable<Transaction>? source = null) =>
        (source ?? _transactions).Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

    public decimal Balance(IEnumerable<Transaction>? source = null)
    {
        var list = (source ?? _transactions).ToList();
        return TotalIncome(list) - TotalExpenses(list);
    }

    public IEnumerable<IGrouping<string, Transaction>> ByCategory(TransactionType type) =>
        _transactions.Where(t => t.Type == type).GroupBy(t => t.Category);

    public IEnumerable<IGrouping<(int Year, int Month), Transaction>> ByMonth() =>
        _transactions.GroupBy(t => (t.Date.Year, t.Date.Month))
                     .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month);
}
