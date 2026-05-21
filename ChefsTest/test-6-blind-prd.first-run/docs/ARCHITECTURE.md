# Architecture

Backfilled 2026-04-27 from observed code. The CLAUDE.md description was correct in spirit; this records what's actually in the tree.

## Stack

- Uno Platform Single Project (`Uno.Sdk`), all 5 TFMs (`net10.0-{android,ios,windows,browserwasm,desktop}`)
- Skia renderer, Material Toolkit theme
- DI: `Microsoft.Extensions.DependencyInjection` via `Uno.Extensions.Hosting`
- Navigation: `Uno.Extensions.Navigation` (region-based)
- MVVM: `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`)

## Project layout (actual, supersedes CLAUDE.md "src/" claim)

```
ChefsTest6/
├── App.xaml(.cs)              # Host builder, navigation routes, DI registration
├── ChefsTest6.csproj
├── GlobalUsings.cs
├── Models/                    # Plain records — Recipe, Ingredient, Cookbook, etc.
├── Platforms/{Android,Desktop,iOS,WebAssembly}/
├── Presentation/              # Pages, ViewModels, UserControls, Converters — colocated
├── Services/                  # IChefService + ChefService (in-memory fixture-backed)
├── Strings/en/                # Localization
├── Styles/ColorPaletteOverride.xaml
└── Assets/                    # Recipe images, profile images, splash assets
```

CLAUDE.md says ViewModels live in `src/ViewModels/`. Actually they live in `Presentation/` next to their pages (`HomeView.xaml` + `HomeTabViewModel.cs`, etc.). The `src/` directory at the repo root contains a stale `ChefsApp/` reference that should be ignored.

## Navigation graph

Registered in `App.xaml.cs` `RegisterRoutes`. All routes are nested under the Shell.

```
ShellViewModel (boot, navigates to default)
└── Onboarding (default) ──▶ Login ──▶ Register
                              │
                              ▼
                            Main (tabs: Home / Search / Favorites / Profile)
                              │
                              ├─▶ RecipeDetail ──▶ Cooking ──▶ CookingDone
                              ├─▶ CookbookDetail ──▶ EditCookbook
                              ├─▶ CreateCookbook
                              ├─▶ Filters (back to Main/Search after apply)
                              ├─▶ Notifications
                              ├─▶ Settings ──▶ (logout) Login
                              ├─▶ Map ──▶ OtherProfile
                              └─▶ OtherProfile
```

`ViewMap` for static screens, `DataViewMap<Page, VM, T>` where the screen needs a payload (Recipe, Cookbook, User).

## Tab pattern

`MainPage` is a single `<Page>` with 4 child views (`HomeView`/`SearchView`/`FavoritesView`/`ProfileView`) toggled by `Visibility` driven from `MainViewModel.IsXxxTab` flags. `NavigationCacheMode="Required"` keeps the page alive across re-navigation. Each tab VM (`HomeTabViewModel` etc.) is owned by `MainViewModel` and re-exposes `MainViewModel`'s commands so child views never need a direct dep on the parent VM type.

## Service layer

- `IChefService` — sole data access. In-memory, populated from `reference/data/*.json` via `FixtureLoader`. Owns: recipes, categories, cookbooks, favorites, notifications, current user.
- `IAppThemeService` — wraps Material Toolkit theme switching.

Both registered as singletons in `App.xaml.cs`.

## Back navigation contract (post-2026-04-27 fix)

- `BackAsync` / `CancelAsync` / `CloseAsync` → `INavigator.NavigateBackAsync(this)` (genuine pop).
- `Qualifiers.ClearBackStack` → reserved for terminals: post-login, post-signup, post-onboarding, post-cooking-done, logout.

If you add a new screen, default to `NavigateBackAsync`. Reach for `ClearBackStack` only when the user genuinely shouldn't be able to navigate back to the prior state.

## Selected-state pattern for tab chips

Tab/chip "selected" visual is driven by `IsXxxTab` bool flags on the VM, mapped to theme brushes via `BoolToBrushConverter` (Converters.cs). Converter parameter format: `"TrueBrushKey|FalseBrushKey"`, looked up at convert time so theme changes propagate.

## Open architectural items (not blockers)

- Page-level bindings use `{Binding}` instead of `{x:Bind}`. DataTemplates correctly use `x:Bind` with `x:DataType`. Migrating root bindings would mean adding `x:DataType` to every `<Page>` and converting ~50+ expressions.
- Bottom tab bar is hand-rolled (Buttons in a Grid). `Uno.Toolkit.UI.TabBar` would be more idiomatic.
- `MapPage` is a fake map (avatar grid). No real maps integration.
