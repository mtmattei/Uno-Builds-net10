using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;
using ChefsTest7.Presentation.Pages;

namespace ChefsTest7;

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
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.TryAddSingleton<IChefsDataService, ChefsDataService>();
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<OnboardingPage, OnboardingViewModel>(),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<RegisterPage, RegisterViewModel>(),
            new ViewMap<MainShellPage, MainShellViewModel>(),
            new ViewMap<HomePage, HomeViewModel>(),
            new ViewMap<SearchPage, SearchViewModel>(),
            new ViewMap<FiltersPage, FiltersViewModel>(),
            new DataViewMap<RecipeDetailPage, RecipeDetailViewModel, Recipe>(),
            new DataViewMap<LiveCookingPage, LiveCookingViewModel, Recipe>(),
            new ViewMap<LiveCookingFinishPage, LiveCookingFinishViewModel>(),
            new ViewMap<FavoritesAllPage, FavoritesAllViewModel>(),
            new ViewMap<FavoritesCookbooksPage, FavoritesCookbooksViewModel>(),
            new DataViewMap<CookbookDetailPage, CookbookDetailViewModel, Cookbook>(),
            new ViewMap<CreateCookbookPage, CreateCookbookViewModel>(),
            new DataViewMap<UpdateCookbookPage, UpdateCookbookViewModel, Cookbook>(),
            new ViewMap<ProfilePage, ProfileViewModel>(),
            new DataViewMap<OtherProfilePage, OtherProfileViewModel, User>(),
            new ViewMap<SettingsPage, SettingsViewModel>(),
            new ViewMap<NotificationsPage, NotificationsViewModel>(),
            new ViewMap<NearMeMapPage, NearMeMapViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new RouteMap("Onboarding", View: views.FindByViewModel<OnboardingViewModel>(), IsDefault: true),
                    new RouteMap("Login", View: views.FindByViewModel<LoginViewModel>()),
                    new RouteMap("Register", View: views.FindByViewModel<RegisterViewModel>()),
                    new RouteMap("Main", View: views.FindByViewModel<MainShellViewModel>(),
                        Nested:
                        [
                            new RouteMap("Home", View: views.FindByViewModel<HomeViewModel>(), IsDefault: true),
                            new RouteMap("Search", View: views.FindByViewModel<SearchViewModel>()),
                            new RouteMap("Favorites", View: views.FindByViewModel<FavoritesAllViewModel>()),
                        ]),
                    new RouteMap("Filters", View: views.FindByViewModel<FiltersViewModel>()),
                    new RouteMap("RecipeDetail", View: views.FindByViewModel<RecipeDetailViewModel>()),
                    new RouteMap("LiveCooking", View: views.FindByViewModel<LiveCookingViewModel>()),
                    new RouteMap("LiveCookingFinish", View: views.FindByViewModel<LiveCookingFinishViewModel>()),
                    new RouteMap("FavoritesAll", View: views.FindByViewModel<FavoritesAllViewModel>()),
                    new RouteMap("FavoritesCookbooks", View: views.FindByViewModel<FavoritesCookbooksViewModel>()),
                    new RouteMap("CookbookDetail", View: views.FindByViewModel<CookbookDetailViewModel>()),
                    new RouteMap("CreateCookbook", View: views.FindByViewModel<CreateCookbookViewModel>()),
                    new RouteMap("UpdateCookbook", View: views.FindByViewModel<UpdateCookbookViewModel>()),
                    new RouteMap("Profile", View: views.FindByViewModel<ProfileViewModel>()),
                    new RouteMap("OtherProfile", View: views.FindByViewModel<OtherProfileViewModel>()),
                    new RouteMap("Settings", View: views.FindByViewModel<SettingsViewModel>()),
                    new RouteMap("Notifications", View: views.FindByViewModel<NotificationsViewModel>()),
                    new RouteMap("NearMeMap", View: views.FindByViewModel<NearMeMapViewModel>()),
                ]
            )
        );
    }
}
