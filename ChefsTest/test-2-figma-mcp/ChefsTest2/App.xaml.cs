using System.Diagnostics.CodeAnalysis;
using ChefsTest2.Services;
using Uno.Resizetizer;

namespace ChefsTest2;

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
                .UseHttp((context, services) => {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IDataService, JsonDataService>();
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();

#if DEBUG
        var initialRoute = Environment.GetEnvironmentVariable("CHEFSTEST2_INITIAL_ROUTE");
        if (!string.IsNullOrWhiteSpace(initialRoute) && Host?.Services is { } svc)
        {
            try
            {
                var navigator = svc.GetService(typeof(INavigator)) as INavigator;
                if (navigator is not null)
                {
                    await Task.Delay(500);
                    await navigator.NavigateRouteAsync(this, $"-/{initialRoute}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[InitialRouteOverride] {ex}");
            }
        }
#endif
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<OnboardingPage, OnboardingViewModel>(),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<RegisterPage, RegisterViewModel>(),
            new ViewMap<HomePage, HomeViewModel>(),
            new ViewMap<SearchPage, SearchViewModel>(),
            new ViewMap<FiltersPage, FiltersViewModel>(),
            new DataViewMap<RecipeDetailPage, RecipeDetailViewModel, Recipe>(),
            new DataViewMap<LiveCookingPage, LiveCookingViewModel, Recipe>(),
            new DataViewMap<LiveCookingFinishPage, LiveCookingFinishViewModel, Recipe>(),
            new ViewMap<FavoritesPage, FavoritesViewModel>(),
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
                    new("Onboarding", View: views.FindByViewModel<OnboardingViewModel>(), IsDefault: true),
                    new("Login", View: views.FindByViewModel<LoginViewModel>()),
                    new("Register", View: views.FindByViewModel<RegisterViewModel>()),
                    new("Home", View: views.FindByViewModel<HomeViewModel>()),
                    new("Search", View: views.FindByViewModel<SearchViewModel>()),
                    new("Filters", View: views.FindByViewModel<FiltersViewModel>()),
                    new("RecipeDetail", View: views.FindByViewModel<RecipeDetailViewModel>()),
                    new("LiveCooking", View: views.FindByViewModel<LiveCookingViewModel>()),
                    new("LiveCookingFinish", View: views.FindByViewModel<LiveCookingFinishViewModel>()),
                    new("Favorites", View: views.FindByViewModel<FavoritesViewModel>()),
                    new("CookbookDetail", View: views.FindByViewModel<CookbookDetailViewModel>()),
                    new("CreateCookbook", View: views.FindByViewModel<CreateCookbookViewModel>()),
                    new("UpdateCookbook", View: views.FindByViewModel<UpdateCookbookViewModel>()),
                    new("Profile", View: views.FindByViewModel<ProfileViewModel>()),
                    new("OtherProfile", View: views.FindByViewModel<OtherProfileViewModel>()),
                    new("Settings", View: views.FindByViewModel<SettingsViewModel>()),
                    new("Notifications", View: views.FindByViewModel<NotificationsViewModel>()),
                    new("NearMeMap", View: views.FindByViewModel<NearMeMapViewModel>()),
                ]
            )
        );
    }
}
