// ═══════════════════════════════════════════════════
// COMMIT 5 — 30. 5.
// git add Converters/Converters.cs
// git commit -m "feat: Converters – typ→barva, částka→text"
// ═══════════════════════════════════════════════════

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FinanceManager.Models;

namespace FinanceManager.Converters;

/// <summary>Converts a TransactionType to a display string (Příjem / Výdaj).</summary>
public class TransactionTypeConverter : IValueConverter
{
    public static readonly TransactionTypeConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TransactionType t ? (t == TransactionType.Income ? "Příjem" : "Výdaj") : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

/// <summary>TransactionType → green/red brush.</summary>
public class TransactionTypeColorConverter : IValueConverter
{
    public static readonly TransactionTypeColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType t)
            return t == TransactionType.Income
                ? new SolidColorBrush(Color.Parse("#16A34A"))
                : new SolidColorBrush(Color.Parse("#DC2626"));
        return Brushes.Black;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

/// <summary>Decimal balance → green/red/gray brush.</summary>
public class BalanceColorConverter : IValueConverter
{
    public static readonly BalanceColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d > 0
                ? new SolidColorBrush(Color.Parse("#16A34A"))
                : d < 0
                    ? new SolidColorBrush(Color.Parse("#DC2626"))
                    : Brushes.Gray;
        return Brushes.Black;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

/// <summary>Formats amount with sign prefix and Kč suffix.</summary>
public class AmountFormatter : IValueConverter
{
    public static readonly AmountFormatter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal d ? $"{d:N2} Kč" : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
