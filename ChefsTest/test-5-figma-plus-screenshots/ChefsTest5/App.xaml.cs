using System.Diagnostics.CodeAnalysis;
using ChefsTest5.Services;
using Uno.Resizetizer;
using Uno.Toolkit.UI;

namespace ChefsTest5;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    public static T GetService<T>() where T : notnull
    {
        if (AppHost is null) throw new InvalidOperationException("Host not initialized.");
        return AppHost.Services.GetRequiredService<T>();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    logBuilder
                        .SetMinimumLevel(context.HostingEnvironment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>())
                .UseLocalization()
                .UseThemeSwitching()
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IChefsDataService, ChefsDataService>();

                    services.AddTransient<HomeViewModel>();
                    services.AddTransient<SearchViewModel>();
                    services.AddTransient<FavoritesViewModel>();
                })
                .UseNavigation(RegisterRoutes));

        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
        AppHost = Host;
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<OnboardingPage, OnboardingViewModel>(),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<RegisterPage, RegisterViewModel>(),
            new ViewMap<MainShellPage, MainShellViewModel>(),
            new DataViewMap<RecipeDetailPage, RecipeDetailViewModel, RecipeData>(),
            new DataViewMap<LiveCookingPage, LiveCookingViewModel, RecipeData>(),
            new DataViewMap<LiveCookingFinishPage, LiveCookingFinishViewModel, RecipeData>(),
            new DataViewMap<CookbookDetailPage, CookbookDetailViewModel, CookbookData>(),
            new ViewMap<CreateCookbookPage, CreateCookbookViewModel>(),
            new DataViewMap<UpdateCookbookPage, UpdateCookbookViewModel, CookbookData>(),
            new ViewMap<ProfilePage, ProfileViewModel>(),
            new DataViewMap<OtherProfilePage, OtherProfileViewModel, UserData>(),
            new ViewMap<SettingsPage, SettingsViewModel>(),
            new ViewMap<NotificationsPage, NotificationsViewModel>(),
            new ViewMap<NearMeMapPage, NearMeMapViewModel>(),
            new ViewMap<FiltersPage, FiltersViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new RouteMap("Onboarding", View: views.FindByViewModel<OnboardingViewModel>(), IsDefault: true),
                    new RouteMap("Login", View: views.FindByViewModel<LoginViewModel>()),
                    new RouteMap("Register", View: views.FindByViewModel<RegisterViewModel>()),
                    new RouteMap("Main", View: views.FindByViewModel<MainShellViewModel>()),
                    new RouteMap("RecipeDetail", View: views.FindByViewModel<RecipeDetailViewModel>()),
                    new RouteMap("LiveCooking", View: views.FindByViewModel<LiveCookingViewModel>()),
                    new RouteMap("LiveCookingFinish", View: views.FindByViewModel<LiveCookingFinishViewModel>()),
                    new RouteMap("CookbookDetail", View: views.FindByViewModel<CookbookDetailViewModel>()),
                    new RouteMap("CreateCookbook", View: views.FindByViewModel<CreateCookbookViewModel>()),
                    new RouteMap("UpdateCookbook", View: views.FindByViewModel<UpdateCookbookViewModel>()),
                    new RouteMap("Profile", View: views.FindByViewModel<ProfileViewModel>()),
                    new RouteMap("OtherProfile", View: views.FindByViewModel<OtherProfileViewModel>()),
                    new RouteMap("Settings", View: views.FindByViewModel<SettingsViewModel>()),
                    new RouteMap("Notifications", View: views.FindByViewModel<NotificationsViewModel>()),
                    new RouteMap("NearMeMap", View: views.FindByViewModel<NearMeMapViewModel>()),
                    new RouteMap("Filters", View: views.FindByViewModel<FiltersViewModel>()),
                ]
            )
        );
    }
}
