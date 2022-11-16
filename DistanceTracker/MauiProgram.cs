using CommunityToolkit.Maui;
using DistanceTracker.ViewModels;
using Prism.DryIoc;
using ZXing.Net.Maui.Controls;

namespace DistanceTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp
                .CreateBuilder()
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .UseMauiCommunityToolkit()
                .UseShinyFramework(
                    new DryIocContainerExtension(),
                    prism => prism.OnAppStart("NavigationPage/MainPage")
                )
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Configuration.AddJsonPlatformBundle();
            RegisterServices(builder);
            RegisterViews(builder.Services);

            MonkeyCache.SQLite.Barrel.ApplicationId = AppInfo.PackageName;

            return builder.Build();
        }


        static void RegisterServices(MauiAppBuilder builder)
        {
            var s = builder.Services;

            s.AddSingleton(MediaPicker.Default);

            s.AddDataAnnotationValidation();
            s.AddGlobalCommandExceptionHandler(new(
#if DEBUG
            ErrorAlertType.FullError
#else
                ErrorAlertType.NoLocalize
#endif
            ));
        }


        static void RegisterViews(IServiceCollection s)
        {
            s.RegisterForNavigation<MainPage, MainViewModel>();
            s.RegisterForNavigation<SettingsPage, SettingsPageViewModel>();
            s.RegisterForNavigation<RecordDistancePage, RecordDistancePageViewModel>();
            s.RegisterForNavigation<SetupEventPage, SetupEventPageViewModel>();
            s.RegisterForNavigation<AddRunnerPage, AddRunnerPageViewModel>();
            s.RegisterForNavigation<EventsListPage, EventsListPageViewModel>();
            s.RegisterForNavigation<DashboardPage, DashboardPageViewModel>();

        }
    }
}