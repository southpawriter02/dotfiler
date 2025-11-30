using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dotman.Gui.Services;
using Dotman.Gui.ViewModels;
using Dotman.Gui.Views;

namespace Dotman.Gui;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Initialize services
            var dotfileService = new DotfileService();

            // Create main window with view model
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(dotfileService)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
