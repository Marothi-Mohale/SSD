using Microsoft.Extensions.Logging;
using SSD.Mobile.ViewModels;

namespace SSD.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<Views.MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
