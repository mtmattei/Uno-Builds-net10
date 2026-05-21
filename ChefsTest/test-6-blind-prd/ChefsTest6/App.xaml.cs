using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Data;
using Uno.Resizetizer;
using ChefsTest6.Presentation.Pages;
using ChefsTest6.ViewModels;

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
                    services.AddSingleton<IRecipeService, RecipeService>();
                    services.AddSingleton<ICookbookService, CookbookService>();
                    services.AddSingleton<IUserService, UserService>();
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IAppSettingsService, AppSettingsService>();
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
            new ViewMap<MainPage, MainViewModel>(),
            new ViewMap<HomePage, HomeViewModel>(),
            new ViewMap<SearchPage, SearchViewModel>(),
            new DataViewMap<SearchPage, SearchViewModel, CategoryData>(),
            new DataViewMap<SearchPage, SearchViewModel, string>(),
            new ViewMap<FavoritesPage, FavoritesViewModel>(),
            new DataViewMap<FiltersPage, FiltersViewModel, FiltersContext>(),
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
            new ViewMap<NearMeMapPage, NearMeMapViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new RouteMap("Onboarding", View: views.FindByViewModel<OnboardingViewModel>(), IsDefault: true),
                    new RouteMap("Login", View: views.FindByViewModel<LoginViewModel>()),
                    new RouteMap("Register", View: views.FindByViewModel<RegisterViewModel>()),
                    new RouteMap("Main", View: views.FindByViewModel<MainViewModel>(),
                        Nested:
                        [
                            new RouteMap("Home", View: views.FindByViewModel<HomeViewModel>(), IsDefault: true),
                            new RouteMap("Search", View: views.FindByViewModel<SearchViewModel>()),
                            new RouteMap("Favorites", View: views.FindByViewModel<FavoritesViewModel>()),
                        ]),
                    new RouteMap("Filters", View: views.FindByViewModel<FiltersViewModel>()),
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
                ]
            )
        );
    }
}
