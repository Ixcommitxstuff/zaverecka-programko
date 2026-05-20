// ═══════════════════════════════════════════════════
// COMMIT 6 — 1. 6. (odevzdání)
// git add App.axaml App.axaml.cs Program.cs FinanceManager.csproj .gitignore README.md
// git commit -m "docs: základ projektu, konfigurace a README"
// ═══════════════════════════════════════════════════

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FinanceManager.Services;
using FinanceManager.Views;

namespace FinanceManager;

public partial class App : Application
{
    public static DataService DataService { get; private set; } = null!;
    public static CategoryService CategoryService { get; private set; } = null!;
    public static ExportService ExportService { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CategoryService = new CategoryService();
        DataService = new DataService(CategoryService);
        ExportService = new ExportService(DataService);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
