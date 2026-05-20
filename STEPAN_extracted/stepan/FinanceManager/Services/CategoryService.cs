// ═══════════════════════════════════════════════════
// COMMIT 2 — 22. 5.
// git add Services/CategoryService.cs
// git commit -m "feat: CategoryService – výchozí kategorie + AppData"
// ═══════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FinanceManager.Services;

public class CategoryService
{
    private readonly string _filePath;
    private readonly List<string> _categories;

    public IReadOnlyList<string> Categories => _categories.AsReadOnly();

    public CategoryService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "FinanceManager");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "categories.txt");

        _categories = LoadCategories();
    }

    private List<string> LoadCategories()
    {
        if (!File.Exists(_filePath))
        {
            // Seed default categories
            var defaults = new List<string>
            {
                "Jídlo", "Bydlení", "Doprava", "Zdraví", "Zábava",
                "Oblečení", "Vzdělání", "Spoření", "Mzda", "Ostatní"
            };
            File.WriteAllLines(_filePath, defaults);
            return defaults;
        }
        return File.ReadAllLines(_filePath)
                   .Where(l => !string.IsNullOrWhiteSpace(l))
                   .Distinct()
                   .ToList();
    }

    public bool AddCategory(string name)
    {
        name = name.Trim();
        if (string.IsNullOrEmpty(name) || _categories.Contains(name, StringComparer.OrdinalIgnoreCase))
            return false;
        _categories.Add(name);
        Save();
        return true;
    }

    public bool RemoveCategory(string name)
    {
        var removed = _categories.Remove(name);
        if (removed) Save();
        return removed;
    }

    private void Save() => File.WriteAllLines(_filePath, _categories);
}
