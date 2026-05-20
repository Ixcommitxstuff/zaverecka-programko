// ═══════════════════════════════════════════════════
// COMMIT 1 — 20. 5.
// git add Models/Transaction.cs
// git commit -m "feat: Transaction model + serializace do TXT"
// ═══════════════════════════════════════════════════

using System;

namespace FinanceManager.Models;

public enum TransactionType { Income, Expense }

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Ostatní";
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>Serializes to a single pipe-delimited line for .txt storage.</summary>
    public string Serialize() =>
        $"{Id}|{Type}|{Amount}|{Description}|{Category}|{Date:yyyy-MM-dd}";

    /// <summary>Parses a line produced by Serialize().</summary>
    public static Transaction? Deserialize(string line)
    {
        var parts = line.Split('|');
        if (parts.Length != 6) return null;
        if (!Guid.TryParse(parts[0], out var id)) return null;
        if (!Enum.TryParse<TransactionType>(parts[1], out var type)) return null;
        if (!decimal.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var amount)) return null;
        if (!DateTime.TryParse(parts[5], out var date)) return null;

        return new Transaction
        {
            Id = id,
            Type = type,
            Amount = amount,
            Description = parts[3],
            Category = parts[4],
            Date = date
        };
    }
}
