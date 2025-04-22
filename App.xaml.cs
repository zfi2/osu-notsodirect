using System.Configuration;
using System.Data;
using System.Windows;
using osu_notsodirect_overlay.Services;
using osu_notsodirect_overlay.Services.Download;
using osu_notsodirect_overlay.Views;

namespace osu_notsodirect_overlay;

public partial class App : Application
{
    public IBeatmapService BeatmapService { get; private set; } = null!;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        BeatmapService = new BeatmapService();
        
        var mainWindow = new OverlayWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
        
        Exit += App_Exit;
    }
    
    private void App_Exit(object sender, ExitEventArgs e)
    {
        BeatmapService?.Dispose();
    }
}

