using System.Diagnostics.CodeAnalysis;
using ChefsTest4.Presentation.Pages;
using ChefsTest4.Services;
using ChefsTest4.ViewModels;
using Uno.Resizetizer;

namespace ChefsTest4;

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
                        .SetMinimumLevel(context.HostingEnvironment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder.EmbeddedSource<App>().Section<AppConfig>()
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
                    services.AddSingleton<IChefsDataService, ChefsDataService>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegisterViewModel>();
                    services.AddTransient<OnboardingViewModel>();
                    services.AddTransient<MainShellViewModel>();
                    services.AddTransient<HomeViewModel>();
                    services.AddTransient<SearchViewModel>();
                    services.AddTransient<FiltersViewModel>();
                    services.AddTransient<RecipeDetailViewModel>();
                    services.AddTransient<LiveCookingViewModel>();
                    services.AddTransient<LiveCookingFinishViewModel>();
                    services.AddTransient<FavoritesRecipesViewModel>();
                    services.AddTransient<FavoritesCookbooksViewModel>();
                    services.AddTransient<FavoritesViewModel>();
                    services.AddTransient<CookbookDetailViewModel>();
                    services.AddTransient<CreateCookbookViewModel>();
                    services.AddTransient<UpdateCookbookViewModel>();
                    services.AddTransient<ProfileViewModel>();
                    services.AddTransient<OtherProfileViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<NotificationsViewModel>();
                    services.AddTransient<NearMeViewModel>();
                })
                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Presentation.Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(Presentation.ShellViewModel)),
            new ViewMap<LoginPage, LoginViewModel>(),
            new ViewMap<RegisterPage, RegisterViewModel>(),
            new ViewMap<OnboardingPage, OnboardingViewModel>(),
            new ViewMap<MainShellPage, MainShellViewModel>(),
            new ViewMap<HomePage, HomeViewModel>(),
            new ViewMap<SearchPage, SearchViewModel>(),
            new ViewMap<FavoritesPage, FavoritesViewModel>(),
            new ViewMap<FiltersPage, FiltersViewModel>(),
            new ViewMap<RecipeDetailPage, RecipeDetailViewModel>(),
            new ViewMap<LiveCookingPage, LiveCookingViewModel>(),
            new ViewMap<LiveCookingFinishPage, LiveCookingFinishViewModel>(),
            new ViewMap<CookbookDetailPage, CookbookDetailViewModel>(),
            new ViewMap<CreateCookbookPage, CreateCookbookViewModel>(),
            new ViewMap<UpdateCookbookPage, UpdateCookbookViewModel>(),
            new ViewMap<ProfilePage, ProfileViewModel>(),
            new ViewMap<OtherProfilePage, OtherProfileViewModel>(),
            new ViewMap<SettingsPage, SettingsViewModel>(),
            new ViewMap<NotificationsPage, NotificationsViewModel>(),
            new ViewMap<NearMePage, NearMeViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<Presentation.ShellViewModel>(),
                Nested:
                [
                    new ("Login", View: views.FindByViewModel<LoginViewModel>(), IsDefault: true),
                    new ("Register", View: views.FindByViewModel<RegisterViewModel>()),
                    new ("Onboarding", View: views.FindByViewModel<OnboardingViewModel>()),
                    new ("MainShell", View: views.FindByViewModel<MainShellViewModel>(),
                        Nested:
                        [
                            new ("Home", View: views.FindByViewModel<HomeViewModel>(), IsDefault: true),
                            new ("Search", View: views.FindByViewModel<SearchViewModel>()),
                            new ("Favorites", View: views.FindByViewModel<FavoritesViewModel>()),
                        ]),
                    new ("Filters", View: views.FindByViewModel<FiltersViewModel>()),
                    new ("RecipeDetail", View: views.FindByViewModel<RecipeDetailViewModel>()),
                    new ("LiveCooking", View: views.FindByViewModel<LiveCookingViewModel>()),
                    new ("LiveCookingFinish", View: views.FindByViewModel<LiveCookingFinishViewModel>()),
                    new ("CookbookDetail", View: views.FindByViewModel<CookbookDetailViewModel>()),
                    new ("CreateCookbook", View: views.FindByViewModel<CreateCookbookViewModel>()),
                    new ("UpdateCookbook", View: views.FindByViewModel<UpdateCookbookViewModel>()),
                    new ("Profile", View: views.FindByViewModel<ProfileViewModel>()),
                    new ("OtherProfile", View: views.FindByViewModel<OtherProfileViewModel>()),
                    new ("Settings", View: views.FindByViewModel<SettingsViewModel>()),
                    new ("Notifications", View: views.FindByViewModel<NotificationsViewModel>()),
                    new ("NearMe", View: views.FindByViewModel<NearMeViewModel>()),
                ]
            )
        );
    }
}
