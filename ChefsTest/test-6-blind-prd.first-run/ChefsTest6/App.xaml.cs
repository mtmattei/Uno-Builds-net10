using System.Diagnostics.CodeAnalysis;
using ChefsTest6.Services;
using Uno.Resizetizer;

namespace ChefsTest6;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
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
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .UseLocalization()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IChefService, ChefService>();
                    services.AddSingleton<IAppThemeService, AppThemeService>();
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        // Render the OS title bar above our content so y<~32 clicks land on
        // the OS chrome, not on app affordances. Without this, Skia desktop
        // extends content into the title bar and treats the top strip as drag
        // region, eating clicks on NavigationBar back chevrons.
        try { MainWindow.ExtendsContentIntoTitleBar = false; }
        catch { /* not supported on this target */ }

        Host = await builder.NavigateAsync<Shell>();

        // Hook the active root for theme changes once Shell content is set.
        if (MainWindow.Content is FrameworkElement root)
        {
            var theme = Host?.Services.GetRequiredService<IAppThemeService>();
            theme?.RegisterRoot(root);
        }
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<OnboardingPage, OnboardingViewModel>(),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<RegisterPage, RegisterViewModel>(),
            new ViewMap<MainPage, MainViewModel>(),
            new ViewMap<FiltersPage, FiltersViewModel>(),
            new DataViewMap<RecipeDetailPage, RecipeDetailViewModel, Recipe>(),
            new DataViewMap<CookingPage, CookingViewModel, Recipe>(),
            new DataViewMap<CookingDonePage, CookingDoneViewModel, Recipe>(),
            new DataViewMap<CookbookDetailPage, CookbookDetailViewModel, Cookbook>(),
            new ViewMap<CreateCookbookPage, CreateCookbookViewModel>(),
            new DataViewMap<EditCookbookPage, EditCookbookViewModel, Cookbook>(),
            new DataViewMap<OtherProfilePage, OtherProfileViewModel, User>(),
            new ViewMap<SettingsPage, SettingsViewModel>(),
            new ViewMap<NotificationsPage, NotificationsViewModel>(),
            new ViewMap<MapPage, MapViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("Onboarding", View: views.FindByViewModel<OnboardingViewModel>(), IsDefault: true),
                    new ("Login", View: views.FindByViewModel<LoginViewModel>()),
                    new ("Register", View: views.FindByViewModel<RegisterViewModel>()),
                    new ("Main", View: views.FindByViewModel<MainViewModel>()),
                    new ("Filters", View: views.FindByViewModel<FiltersViewModel>()),
                    new ("RecipeDetail", View: views.FindByViewModel<RecipeDetailViewModel>()),
                    new ("Cooking", View: views.FindByViewModel<CookingViewModel>()),
                    new ("CookingDone", View: views.FindByViewModel<CookingDoneViewModel>()),
                    new ("CookbookDetail", View: views.FindByViewModel<CookbookDetailViewModel>()),
                    new ("CreateCookbook", View: views.FindByViewModel<CreateCookbookViewModel>()),
                    new ("EditCookbook", View: views.FindByViewModel<EditCookbookViewModel>()),
                    new ("OtherProfile", View: views.FindByViewModel<OtherProfileViewModel>()),
                    new ("Settings", View: views.FindByViewModel<SettingsViewModel>()),
                    new ("Notifications", View: views.FindByViewModel<NotificationsViewModel>()),
                    new ("Map", View: views.FindByViewModel<MapViewModel>()),
                ]
            )
        );
    }
}
