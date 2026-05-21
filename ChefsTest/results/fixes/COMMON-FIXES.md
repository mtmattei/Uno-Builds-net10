# Common fixes — applies to all 7 ChefsFT projects

> Synthesized from `master-comp-matrix.md` §3 (Pre-bake checklist), §17 (Tier A/B/C/D gotchas), §6 (UnoFeatures dropouts), and §7 (Toolkit composite substitutions). Each project also has its own per-test fix doc (`test-N-fixes.md`) for issues unique to that run.
>
> Read order: do every Tier-1 fix before opening any per-project doc — these are the ones that bit ≥3 runs and are pure preventive work.

## Tier 1 — Universal Tier-A gotchas (every run that touched the path was bitten)

### 1. Add `Svg;` to `<UnoFeatures>` in the .csproj

**Symptom:** `<Image Source="ms-appx:///Assets/Images/empty_box_light.svg" />` and similar SVG `Image.Source` URIs render blank with no error. Brand wordmark, empty-state illustrations, splash pictogram all fail silently.

**Root cause:** `Svg` is *not* implied by `SkiaRenderer`. The `Image` control accepts `.svg` URIs but the renderer needs the explicit feature flag plus, in some setups, an explicit `Uno.WinUI.Svg` `<PackageReference>`.

**Fix:**
```xml
<PropertyGroup>
  <UnoFeatures>
    Material;
    Hosting;
    Toolkit;
    Logging;
    Configuration;
    HttpKiota;
    Serialization;
    Localization;
    Navigation;
    MediaElement;
    Skia;
    SkiaRenderer;
    ThemeService;
    Authentication;
    Svg;  <!-- ADD THIS -->
  </UnoFeatures>
</PropertyGroup>
```

If after rebuild the runtime still warns `Microsoft.UI.Xaml.Media.Imaging.SvgImageSource: To use SVG on this platform, make sure to install the Uno.WinUI.Svg package` (test-5 hit this), add the explicit reference:
```xml
<PackageReference Include="Uno.WinUI.Svg" Version="6.x.*" />
```

**Tier-A caveat (test-7 finding):** even with `Svg` enabled, **SVGs with malformed viewBox attributes silently render blank.** `chefsappsignature_*.svg` and `success_*.svg` from `reference/assets/` exhibit this in every run that exercised them. **Pre-render those specific files to PNG at build time** (or use a styled `TextBlock` fallback for the wordmark, an emoji-on-circle fallback for the success illustration — test-7's iter-3 #15/#16/#17 patterns).

### 2. Custom `JsonConverter<TimeSpan>` handling all 3 fixture shapes

**Symptom:** Empty Trending / Recently Added carousels on Home; Cookbook Detail page renders 0 recipes; no exception in console.

**Root cause:** Fixture data is two-encoded:
- `Recipes.json` ships `"PrepTime": { "ticks": 9600000000 }` — object form
- `Cookbooks.json` ships `"AverageTime": "00:10:00"` — ISO string form
- Some sub-records ship bare numbers

The default `System.Text.Json` `TimeSpan` converter accepts only the ISO string. Object-form deserialize throws → silent caught somewhere up the call stack → empty `ObservableCollection`.

**Fix:** Register a flexible converter on the `JsonSerializerOptions` used by your data service.

```csharp
public sealed class FlexibleTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return TimeSpan.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
            case JsonTokenType.Number:
                return TimeSpan.FromTicks(reader.GetInt64());
            case JsonTokenType.StartObject:
                long ticks = 0;
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName &&
                        reader.GetString()!.Equals("ticks", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Read();
                        ticks = reader.GetInt64();
                    }
                }
                return TimeSpan.FromTicks(ticks);
            default:
                throw new JsonException($"Unexpected token {reader.TokenType} for TimeSpan");
        }
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("c"));
}
```

Hit by test-1, test-6, test-7 (test-7 #5 was a root-cause fix for the empty Trending carousels).

### 3. Lenient `JsonSerializerOptions`

**Symptom:** Fixture parses fail on trailing commas or `// comment` lines; case-mismatch on property names.

**Fix:**
```csharp
private static readonly JsonSerializerOptions JsonOpts = new()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    Converters = { new FlexibleTimeSpanConverter() }
};
```

### 4. Mark every record with an `Id` as `partial`

**Symptom:** Build error `KE0001` — even on MVVM projects (no `MVUX` in `<UnoFeatures>`). Cascades into ~50 `CS0246` "BindableMainModel type not found" noise that masks the real cause.

**Root cause:** The MVUX `IKeyEquatable` source-generator runs whenever Hosting + Navigation `UnoFeatures` are enabled, regardless of the presentation pattern. Records with an `Id` property get extended; the generator requires `partial`.

**Fix:** Add `partial` to every DTO record that has an `Id`:
```csharp
public partial record RecipeData(int Id, string Title, ...);
public partial record CookbookData(int Id, string Name, ...);
public partial record UserData(int Id, string Name, ...);
public partial record ReviewData(int Id, ...);
public partial record CategoryData(int Id, ...);
```

Records *without* `Id` (Ingredient, Step, Nutrition, LoginRequest, Notification, OnboardingSlide) **don't** need `partial`.

Bit test-6 with 5 errors + ~50 downstream cascades on first build.

### 5. Card-tap requires a focusable root inside `ItemsRepeater` / `ListView` / `GridView` `ItemTemplate`

**Symptom:** Recipe / cookbook / contributor cards visually highlight on hover, focus ring appears on click, but tap does *not* navigate. `Command="{Binding OpenRecipeCommand}"` on a `Border` or `Grid` looks correct in XAML but fires nothing at runtime.

**Root cause:** `Border` and `Grid` aren't focusable controls. Inside an `ItemsRepeater` or `ListView`'s `ItemTemplate`, the container intercepts the click as item-selection focus *only* — the inner `Command` binding never fires.

**Fix (option A — Button wrap):**
```xml
<DataTemplate x:DataType="m:RecipeData">
  <Button Style="{StaticResource RecipeCardButtonStyle}"
          Command="{Binding DataContext.OpenRecipeCommand, ElementName=PageRoot}"
          CommandParameter="{x:Bind}">
    <!-- card content -->
  </Button>
</DataTemplate>
```

Note the `ElementName=PageRoot` — `DataTemplate.DataContext` is the *item*, not the page VM. Walk to the page root by name.

**Fix (option B — `IsItemClickEnabled` on `GridView`):**
```xml
<GridView ItemsSource="{Binding Recipes}"
          IsItemClickEnabled="True"
          SelectionMode="None"
          ItemClick="OnRecipeClick">
  ...
</GridView>
```

**Fix (option C — canonical upstream pattern, prefer this if using Uno.Extensions.Navigation):**
```xml
<Button uen:Navigation.Request="RecipeDetail"
        uen:Navigation.Data="{x:Bind}"
        ...>
```

Bit test-1, test-4, test-5, test-7 across grids on Home, Search, Favorites All, Favorites Cookbooks, Cookbook Detail, Profile, OtherProfile.

### 6. Region-based nav with `IsDefault: true` + `Region.Navigator="Visibility"` doesn't auto-instantiate child regions on Skia desktop

**Symptom:** `MainPage` renders empty — Home/Search/Favorites tab content is just the TabBar with blank space below. Routes register fine, no exception.

**Root cause:** New Tier-A gotcha surfaced by test-7 (FP #3/#4) and confirmed by test-4 (Blocker #2). The Toolkit composite + region pattern that **upstream uses pervasively** has a runtime trap: the parent region with `IsDefault: true` + `Region.Navigator="Visibility"` does not trigger child-region instantiation on Skia desktop.

**Fix (chosen by test-7):** flat routing under Shell.

```csharp
// In App.xaml.cs RegisterRoutes
routeBuilder.Register(
    new RouteMap("", View: views.FindByViewModel<ShellModel>(),
        Nested:
        [
            new ("Onboarding", IsDefault: true, View: views.FindByViewModel<OnboardingModel>()),
            new ("Login", View: views.FindByViewModel<LoginModel>()),
            new ("Home", View: views.FindByViewModel<HomeModel>()),
            new ("Search", View: views.FindByViewModel<SearchModel>()),
            new ("FavoritesAll", View: views.FindByViewModel<FavoritesAllRecipesModel>()),
            new ("FavoritesCookbooks", View: views.FindByViewModel<FavoritesMyCookbooksModel>()),
            // ... all remaining 16 pages as flat siblings ...
        ]));
```

**Fix (alternative — match upstream):** if you must keep the Toolkit `utu:TabBar` + region pattern, smoke-test on Skia desktop *before* declaring the run done. If empty regions appear, revert to flat routing.

### 7. `dotnet run` exit-code 0 within seconds is NOT a crash

**Symptom:** `dotnet run -f net10.0-desktop` returns exit 0 (or `127` via Bash background) within 2-5s; stdout is empty. Looks like a silent crash; isn't.

**Root cause:** A healthy Skia desktop window detaches from the console; Uno's `ILogger` doesn't route to `Console.Out` by default; `tasklist` from Bash is unreliable on Windows.

**Fix:** Verify launch via PowerShell:
```powershell
Get-Process -Name <YourAppName> -ErrorAction SilentlyContinue
```

If a row returns, the process is alive. If empty, look at `Get-EventLog -LogName Application -Source "Application Error" -Newest 5`.

Bit every run that tried to verify launch from Bash.

### 8. `Uno0001 ItemsWrapGrid not implemented` — compile-time warning, runtime degrades to single row

**Symptom:** `<ListView><ItemsWrapGrid Orientation="Horizontal"/></ListView>` builds clean with a warning, then renders as a single horizontal row instead of a multi-column grid on Skia desktop / WASM / iOS native.

**Fix:** Replace with `ItemsRepeater` + `UniformGridLayout`.
```xml
<ScrollViewer>
  <muxc:ItemsRepeater ItemsSource="{x:Bind ViewModel.Recipes}">
    <muxc:ItemsRepeater.Layout>
      <muxc:UniformGridLayout MinItemWidth="160" MinItemHeight="200"
                              MinRowSpacing="12" MinColumnSpacing="12" />
    </muxc:ItemsRepeater.Layout>
    <muxc:ItemsRepeater.ItemTemplate>
      <DataTemplate x:DataType="m:RecipeData">
        <!-- card content -->
      </DataTemplate>
    </muxc:ItemsRepeater.ItemTemplate>
  </muxc:ItemsRepeater>
</ScrollViewer>
```

Bit test-3, test-4, test-6.

### 9. Pre-grep `Assets/` for actual filenames before referencing them in XAML

**Symptom:** `<Image Source="ms-appx:///Assets/Maps/map.jpg" />` renders blank (file doesn't exist; only `.svg` ships). No diagnostic.

**Root cause:** Image refs that don't resolve don't error.

**Fix:** Before authoring XAML, run:
```bash
ls reference/assets/Icons/
ls reference/assets/Maps/
ls reference/assets/Welcome/
```

Specifically:
- `Welcome/` ships `first_splash_screen.png`, `second_splash_screen.png`, `third_splash_screen.png` — *not* `welcome_1.jpg` etc. (test-2 FP #5).
- `Maps/` ships only `location_pin.svg` + `location_circle.svg` — no `map.jpg` background. Use `Mapsui.Uno.WinUI` for a real tile or accept a colored rectangle placeholder.
- `Icons/` ships only branding + `close.svg` — no per-ingredient PNGs (`avocado.png`, `egg.png`, etc.). The 4 ingredient-icon 404s in §13 are an **input gap**, not implementation loss.

### 10. Use `NavigateRouteAsync(this, "Route", data: payload)`, not `NavigateDataAsync`, for routed-with-payload nav

**Symptom:** "I want to go to Route X with a typed payload Y." The intuitive `_navigator.NavigateDataAsync(this, payload)` doesn't take a `route:` parameter; it picks the route from the registered `DataViewMap<TPage, TModel, TData>`.

**Fix:**
```csharp
await _navigator.NavigateRouteAsync(this, "RecipeDetail", data: recipe);
```

For pure data-driven nav (no specific route name needed), use `NavigateDataAsync` — it'll pick the route via the registered `DataViewMap`.

### 11. `DataViewMap<Page, Model, TData>` for pages whose VM ctor takes a payload

**Symptom:** Tap on a card always navigates to RecipeDetail showing "Avocado Toast" regardless of which card was tapped.

**Root cause:** Plain `ViewMap<Page, Model>` silently drops `data:` arguments at navigation time. Without a registered `DataViewMap`, the navigator can't marshal the payload.

**Fix:** Register pages whose VMs need payloads as `DataViewMap`:
```csharp
viewBuilder.Register(
    new DataViewMap<RecipeDetailPage, RecipeDetailModel, RecipeData>(),
    new DataViewMap<CookbookDetailPage, CookbookDetailModel, CookbookData>(),
    new DataViewMap<ProfilePage, ProfileModel, UserData>(),
    new DataViewMap<UpdateCookbookPage, UpdateCookbookModel, CookbookData>(),
    new ViewMap<HomePage, HomeModel>(),  // no payload
    new ViewMap<SearchPage, SearchModel>(),
    // ...
);
```

Bit test-1.

### 12. Verify `mcp__uno-app__*` MCP at session start

**Symptom:** `uno_app_*` calls return `InputValidationError` even though tool *names* appear in the deferred-tools list.

**Root cause:** Tool *schemas* are lazy. The harness silently drops failed servers. test-3 found the workaround.

**Fix:** Issue this once at session start, before any `uno_app_*` call:
```
ToolSearch select:mcp__uno-app__uno_app_get_runtime_info,mcp__uno-app__uno_app_get_screenshot,mcp__uno-app__uno_app_visualtree_snapshot,mcp__uno-app__uno_app_pointer_click,mcp__uno-app__uno_app_element_peer_default_action
```

**If tools still don't surface or disconnect mid-session** (test-1 re-run, test-5, test-6 re-run): **restart the Claude Code session.** Don't fall back to PowerShell GDI+ capture — test-5 confirmed it isn't equivalent to MCP tree-aware capture.

## Tier 2 — Framework features every run dropped vs upstream

These aren't strictly *fixes* on a passing build — they're feature gaps the experiment confirmed every methodology produces. Address per-project as needed; the public stunt should call them out as known dropouts unless the per-project doc says otherwise.

### A. MVUX → MVVM substitution

Every run shipped MVVM (CommunityToolkit.Mvvm `[ObservableProperty]` + `[RelayCommand]`) instead of upstream's MVUX (`IFeed<T>` / `IState<T>` / `IListState<T>`).

**Why every run did this:** operator default + absent MVUX-only inputs. Both `[ObservableProperty]` and `IFeed<T>` are well documented; MVVM is the easier first reach.

**Fix (if MVUX is required):** scaffold with `dotnet new unoapp -presentation mvux`. Then expect Tier B gotchas — see `master-comp-matrix.md` §17.B (only test-6 hit them on a non-MVUX scaffold because the source-gen runs whenever Hosting/Navigation features are enabled).

### B. `Authentication` → demo bypass

Every run downgraded auth to `_navigator.NavigateBackAsync` or "match by email." Upstream uses `UseAuthentication()` with a custom auth handler simulating `ProcessCredentials`.

**Fix:** add `UseAuthentication()` in the host builder + register a custom `IAuthenticationProvider`:
```csharp
.UseAuthentication(b => b.AddCustom(c => c.Login(async (sp, dispatcher, credentials, ct) =>
{
    // simulate token issuance from email/password
    return new Dictionary<string, string> { ["Token"] = "demo-token" };
})))
```

### C. `Localization` (4 locales) → none

No run shipped `Strings/{en,es,fr,pt-BR}/Resources.resw`. Test-7 retained `UseLocalization` from the scaffold but didn't ship resources.

**Fix:** add `UseLocalization()` + create the 4 locale folders + run `dotnet build` to generate resource keys; reference via `{Binding [SomeKey], Source={StaticResource Resources}}` or `x:Uid="key"`.

### D. Toolkit composites every run substituted with primitives

Hit by every run. See `master-comp-matrix.md` §7 for the verified upstream usage. Fix at the per-project level — tracked in each `test-N-fixes.md`. Highlights:

- **`utu:AutoLayout`** (30+ uses on RecipeDetailsPage upstream → 0 uses across all tests). Replace `Grid` + `StackPanel` with `<utu:AutoLayout Orientation="Vertical" Spacing="12" Justify="Stretch">` for declarative spacing.
- **`uer:FeedView`** (6+ uses upstream → 0 uses). Replace the `ItemsControl + HasItems bool + empty-state branch` triad with `<uer:FeedView Source="{Binding RecipesFeed}" NoneTemplate="{StaticResource EmptyTemplate}">`.
- **`utu:TabBar TopTabBarStyle`** for in-page tabs (RecipeDetails 4-tab → all tests used deprecated `Pivot` or hand-rolled Buttons). Replace with `<utu:TabBar Style="{StaticResource TopTabBarStyle}">` + 4 `<utu:TabBarItem uen:Region.Name="..." />`.
- **`uen:Region.Attached` + `Region.Navigator="Visibility"`** (MainPage tab content + RecipeDetails tab content → 0 uses). See Tier-1 fix #6 above for the Skia-desktop trap; if you can land it, prefer this over flat routing for upstream-canonical structure.
- **`uen:Navigation.Request` markup ext** for card-tap (0 uses across all tests; runtime-failure mode in test-1 + test-4). Prefer over `Command="{Binding ...}"` — declarative + doesn't break under ListViewItem container interception.

### E. Real `Mapsui.Uno.WinUI` (Map page) + `LiveChartsCore.SkiaSharpView.Uno.WinUI` (nutrition donut)

Every run shipped placeholder rectangles + stacked `Ellipse` strokes.

**Fix (Map):**
```xml
<PackageReference Include="Mapsui.Uno.WinUI" Version="4.x.*" />
```

```xml
<mapsui:MapControl x:Name="MapView" />
```

```csharp
MapView.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
foreach (var pin in viewModel.Pins)
    MapView.Map.Layers.Add(CreatePinLayer(pin.Lat, pin.Lon, pin.Avatar));
```

**Fix (Nutrition donut):**
```xml
<PackageReference Include="LiveChartsCore.SkiaSharpView.Uno.WinUI" Version="2.x.*" />
```

```xml
<lvc:PieChart Series="{x:Bind ViewModel.NutritionSeries}" />
```

### F. `MediaPlayerElement` on Live Cooking

3 of 7 runs shipped a real `MediaPlayerElement` (test-6 only). The other 4 used a play-icon overlay surrogate.

**Fix:**
```xml
<MediaPlayerElement AreTransportControlsEnabled="True"
                    AutoPlay="True"
                    Source="{x:Bind ViewModel.CurrentStep.UrlVideo}">
  <MediaPlayerElement.TransportControls>
    <MediaTransportControls IsCompact="True" />
  </MediaPlayerElement.TransportControls>
</MediaPlayerElement>
```

`Source` accepts both `ms-appx://` URIs and `http(s)://` URIs. The `CookingVideo.mp4` fixture is bundled in `reference/assets/Videos/`.

## Tier 3 — Operational hygiene

### Pre-bake at scaffold time (saves 5–15 min per run)

Run this checklist before opening Claude Code:

1. ☐ `dotnet new unoapp -preset recommended -presentation mvvm -markup xaml -theme material -platforms desktop,wasm,android,ios,windows -renderer skia -tfm net10.0-*`
2. ☐ Edit `.csproj`: add `Svg;` + (if needed) `<PackageReference Include="Uno.WinUI.Svg" />`
3. ☐ Add `Mapsui.Uno.WinUI` + `LiveChartsCore.SkiaSharpView.Uno.WinUI` references *if visual fidelity on Map / Recipe-Detail nutrition matters*
4. ☐ Copy `reference/assets/` into `<App>/Assets/` (94 files)
5. ☐ Copy `reference/data/*.json` into `<App>/Assets/Data/`
6. ☐ Delete scaffold placeholders (`MainPage`, `SecondPage`, `Entity.cs`) before authoring routes
7. ☐ Add `FlexibleTimeSpanConverter` + lenient `JsonSerializerOptions` (Tier-1 #2/#3)
8. ☐ Mark all `Id`-bearing records as `partial` (Tier-1 #4)
9. ☐ Apply the starter kit so `uno-app` MCP server is present at session start (Tier-1 #12)
10. ☐ Open Claude Code, invoke `mcp__uno__uno_platform_agent_rules_init` + `mcp__uno__uno_platform_usage_rules_init` once — both ALWAYS, before any code (per-test SPEC §"Skill-usage discipline")

### Skill discipline (after rules-init)

Before authoring code in each domain, invoke the matching `Skill` tool — *not* a `SKILL-USE` log line, an actual `Skill` tool call:

| Domain | Skill |
|---|---|
| Page authoring (XAML, x:Bind) | `winui-xaml` |
| Theming, MD3 tokens, color palette | `uno-material` |
| Toolkit primitives (TabBar, AutoLayout, SafeArea) | `uno-toolkit` |
| Routes, INavigator, ViewMap registration | `uno-navigation` |
| DI, Hosting, HTTP, config | `uno-extensions-services` |
| Typography, modular scale, responsive | `userinterface-wiki-uno` |
| Cross-cutting MVVM scaffold | `uno-platform-agent` |
| Walk-through screenshot capture | `uno-app-ui-testing` |
| Visual-tree assertions | `uno-app-test-assertions` |

After each invocation, add a `SKILL-USE: <name>` line to `test-N.log` referencing what the skill returned.

**Audit gate at workflow step 10.5:** count actual `Skill` tool calls in the transcript — must equal `SKILL-USE` line count + skill-usage table `Invocations` column. If they don't match, you've hit failure mode #2 (test-7's gap). The fix is to stop authoring code from internalized rules-init patterns and start invoking the skills.
