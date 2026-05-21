# test-1 fixes — Screenshots only (re-run, 2026-04-28)

> Project folder: `test-1-screenshots/ChefsTest1/`
> Run grade: **C** (60/100). Discipline ledger clean (re-run cleared failure mode #1).
> Pass-bar status: 1 ✅, 2 ✅ code-level (3 unmeasurable due to MCP outage).
>
> **Read `COMMON-FIXES.md` first.** Items below are unique to this run or are common-fix specifics with extra context for this codebase.

## Build / scaffolding

### 1. PipsPager + OnboardingFrame namespace fix (FP #1 in re-run)

**Symptom:** First build fails with `XamlCompiler` complaining about `<muxc:PipsPager>` and `OnboardingFrame` not resolved.

**Fix:** Add the missing xmlns at page roots and the `m:` mapping for the VM-side record:
```xml
<Page x:Class="ChefsTest1.Presentation.OnboardingPage"
      xmlns:muxc="using:Microsoft.UI.Xaml.Controls"
      xmlns:m="using:ChefsTest1.ViewModels">
  <DataTemplate x:DataType="m:OnboardingFrame">
    ...
  </DataTemplate>
  <muxc:PipsPager .../>
</Page>
```

Already applied in the re-run. Mentioned for completeness.

### 2. `Uno.WinUI.Svg` runtime warning (FP #2 in re-run)

**Symptom:** Console shows `SvgImageSource: To use SVG on this platform, make sure to install the Uno.WinUI.Svg package`. Brand wordmark + empty-state SVGs render blank on Skia desktop.

**Fix:** Already added `Svg;` to `<UnoFeatures>` in re-run (FP #2). If brand wordmark *still* doesn't render after rebuild, the file likely has a viewBox issue (test-7 #15/#16 found this) — replace with a styled `TextBlock` lockup:
```xml
<StackPanel Orientation="Horizontal" Spacing="4" HorizontalAlignment="Center">
  <TextBlock Text="Uno"  FontSize="36" FontWeight="ExtraBold"
             Foreground="{ThemeResource OnSurfaceBrush}"/>
  <TextBlock Text="Chefs" FontSize="36" FontWeight="ExtraBold"
             Foreground="{ThemeResource PrimaryBrush}"/>
</StackPanel>
```

## Anti-patterns shipping live

### 3. Cards-without-tap (Border-wrapped) — known gap acknowledged

**Symptom:** Recipe / cookbook / contributor cards on Home Trending, Home Recently Added, Search results, Favorites All Recipes, Favorites My Cookbooks, Profile My Recipes, Cookbook Detail get a pink focus ring on click, but tap does *not* navigate. `OpenRecipeCommand` is wired on the parent VM but never fires from the wrapping `Border`.

**Root cause:** Tier-1 fix #5. `Border` isn't focusable; `ItemsRepeater` / `ListView` container intercepts as item-selection.

**Fix:** Wrap each card's `Border` in a `<Button Style="..."/>` that styles like a card:
```xml
<DataTemplate x:DataType="m:RecipeData">
  <Button Style="{StaticResource RecipeCardButtonStyle}"
          Command="{Binding DataContext.OpenRecipeCommand, ElementName=PageRoot}"
          CommandParameter="{x:Bind}">
    <Grid CornerRadius="12" ...>
      <!-- existing card content -->
    </Grid>
  </Button>
</DataTemplate>
```

`<Page x:Name="PageRoot">` so `ElementName=PageRoot` resolves at the template scope.

This is the load-bearing fix for test-1 — the original "5/5 first-try, 12/20 reachable post-verif" symptom traces directly back here. Apply across:
- `HomePage.xaml` Trending carousel
- `HomePage.xaml` Recently Added carousel
- `HomePage.xaml` Popular Contributors avatar row (→ `OtherProfilePage`)
- `SearchPage.xaml` results grid
- `FavoritesAllRecipesPage.xaml` grid
- `FavoritesMyCookbooksPage.xaml` grid (→ `CookbookDetailPage`)
- `OwnProfilePage.xaml` My Recipes grid
- `CookbookDetailPage.xaml` recipes grid

### 4. Heart-toggle persistence — model needs INPC

**Symptom:** Tap heart → mutates the in-memory cache, but the card visual doesn't reflect (`Recipe.IsFavorite` stays read as the original value because the model record isn't `INotifyPropertyChanged`).

**Fix (option A — re-emit the collection):**
```csharp
// in HomeViewModel after ToggleFavoriteAsync
[ObservableProperty]
private ObservableCollection<RecipeData> _trending = new();

public async Task ToggleFavoriteAsync(RecipeData recipe)
{
    await _data.ToggleFavoriteAsync(recipe.Id);
    var idx = Trending.IndexOf(recipe);
    if (idx >= 0) Trending[idx] = recipe with { IsFavorite = !recipe.IsFavorite };
}
```

**Fix (option B — `[ObservableProperty]` on a wrapper class):** wrap `RecipeData` in a `RecipeViewModel` with `[ObservableProperty] bool _isFavorite`. More code; cleaner reactivity.

### 5. Notifications grouping (Today / Yesterday / Monday headers)

**Symptom:** Reference shows notifications grouped by relative date with sticky headers. Test-1 ships a flat `ItemsControl` list.

**Fix:** Use `CollectionViewSource` with a key-selector projecting to a relative-date string:
```csharp
public string RelativeGroup => Date switch
{
    var d when d.Date == DateTime.Today => "Today",
    var d when d.Date == DateTime.Today.AddDays(-1) => "Yesterday",
    var d when (DateTime.Today - d.Date).TotalDays < 7 => d.ToString("dddd"),
    _ => Date.ToString("MMMM d")
};
```

```xml
<CollectionViewSource x:Key="GroupedNotifications"
                      Source="{x:Bind ViewModel.Notifications}"
                      IsSourceGrouped="True"
                      ItemsPath="Items"/>
<ListView ItemsSource="{StaticResource GroupedNotifications}">
  <ListView.GroupStyle>
    <GroupStyle>
      <GroupStyle.HeaderTemplate>
        <DataTemplate>
          <TextBlock Text="{Binding Key}" Style="{StaticResource TitleSmallStyle}"/>
        </DataTemplate>
      </GroupStyle.HeaderTemplate>
    </GroupStyle>
  </ListView.GroupStyle>
</CollectionViewSource>
```

## Visual fidelity

### 6. Theme dead-spots in Live Cooking overlay

**Symptom:** Live Cooking translucent overlay uses raw `#88000000` and inline `FontWeight="SemiBold"` — these don't change with theme.

**Fix:** Replace with theme-aware brushes:
```xml
<Grid Background="{ThemeResource MediaOverlayBrush}">
  <!-- elsewhere in App.xaml: Light=#88000000, Dark=#88FFFFFF -->
</Grid>
```

### 7. Tablet adaptive layout

**Symptom:** Phone TabBar at all viewports. Reference tablet shows left-rail nav + 2-pane Recipe Detail.

**Fix:** Add an `AdaptiveTrigger` at the page level on `MainPage`, `RecipeDetailPage`, `HomePage`:
```xml
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup>
    <VisualState>
      <VisualState.StateTriggers>
        <AdaptiveTrigger MinWindowWidth="720"/>
      </VisualState.StateTriggers>
      <VisualState.Setters>
        <Setter Target="BottomBar.Visibility" Value="Collapsed"/>
        <Setter Target="LeftRailNav.Visibility" Value="Visible"/>
        <Setter Target="ContentColumn.(Grid.Column)" Value="1"/>
      </VisualState.Setters>
    </VisualState>
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>
```

The cleanest is `utu:Responsive` markup ext — see common-fixes Tier 2.D.

## Stub commands (still no real persistence)

### 8. Save / Update / Sign Up commands

**Symptom:** `Settings.SaveChanges` navigates back without writing; `RegisterPage.SignUp` no-op.

**Fix:** Either (a) wire to the in-memory data service via `_data.UpdateCurrentUserAsync(user)` and accept session-only persistence, or (b) write back to `Assets/Data/Users.json` via `StorageFile.WriteAllTextAsync` (more work, also platform-specific paths). Option (a) matches every other run + matches what API-CONTRACT.md considers acceptable.

## Methodology gaps from the re-run

### 9. uno-app MCP went down mid-run

**Symptom:** No per-page screenshot pass possible. Visual-match scoring stuck at "code-level review."

**Fix path:**
1. Restart Claude Code session — the MCP often re-attaches cleanly.
2. Re-run with `ToolSearch select:mcp__uno-app__*` at session start (Tier-1 #12).
3. If still failing, capture screenshots from `dotnet run -f net10.0-desktop` via PowerShell GDI+ and pixel-diff against `Chefs-screenshots/Chef App-mobile-light/` in a separate validator session (the validator is allowed to read `Chefs-screenshots/`; the implementation session is not).

### 10. Per-ingredient icon 404s

**Symptom:** Recipes.json references `ms-appx:///Assets/Icons/avocado.png` but only branding SVGs ship in `reference/assets/Icons/`. Cards render with blank thumbnails.

**Fix:** Pure input gap — affects every run identically. Either ship synthetic 32×32 emoji-on-circle placeholders or extend the asset pack with real ingredient PNGs. Not a test-1-specific bug.

## What's done / acceptable as-is

- 5/5 builds green ✅
- 20/20 routes registered + data-bound at code level ✅
- Theme dictionaries (Light + Dark) authored with chef-pink + cream + inverted near-black ✅
- Empty states (5/5) implemented and gated by `HasItems` flags ✅
- Recipe Detail 4 tabs implemented ✅
- 8 actual `Skill` tool calls + clean DISCIPLINE-AUDIT ✅

## Priority order for next iteration

1. **Card-tap (#3)** — the load-bearing functional fix; without this the "20/20 navigable" claim is fragile.
2. **MCP recovery (#9)** — unblocks all visual measurement.
3. **Heart toggle (#4) + Notifications grouping (#5)** — visible polish.
4. **Tablet adaptive (#7)** — only if pixel-parity at ≥720px matters for the public stunt.
5. **Persistence (#8)** — only if "save changes survive a session" is in scope.

Estimated effort to clear pass-bar criterion 3 (≥75%/combo) once MCP is back: ~30 min.

---

## Walkthrough fix-pass — 2026-04-28 (uno-app MCP recovered)

Run context: `mcp__uno-app__*` came back online; drove `dotnet run -f net10.0-desktop` through all 19 routes and screenshotted each (`results/screenshots/walkthrough-2026-04-28/`). Four code edits landed mid-walkthrough to unblock pages that the post-scaffold checkpoint left unreachable. All four are minimal-diff variants of issues already flagged in `test-1-learnings.md`.

### W1. `MainPage.xaml` — inline page content into Visibility regions

**Symptom:** All three tab selections (Home / Search / Favorites) render an empty body. TabBar selection indicator updates correctly; the page surface above stays blank.

**Root cause:** `MainPage.xaml` had three sibling grids with `uen:Region.Name="Home/Search/Favorites" Visibility="Collapsed"` and **no children**. With `Region.Navigator="Visibility"` the navigator only toggles existing children; it does not inject a Page from the route registry. The default-route resolution under `Main` therefore had nothing to make visible.

**Fix (applied):**
```xml
<Page x:Class="ChefsTest1.Presentation.MainPage"
      ...
      xmlns:local="using:ChefsTest1.Presentation">
  ...
  <Grid Grid.Row="0" uen:Region.Attached="True" uen:Region.Navigator="Visibility">
    <local:HomePage      uen:Region.Name="Home"      Visibility="Collapsed" />
    <local:SearchPage    uen:Region.Name="Search"    Visibility="Collapsed" />
    <local:FavoritesPage uen:Region.Name="Favorites" Visibility="Collapsed" />
  </Grid>
```
Hot Reload picked this up live (no restart needed).

### W2. Card tap handlers — minimal-diff alternative to §3

**Symptom:** Same pattern §3 documented — recipe / cookbook / contributor cards on `HomePage`, `SearchPage`, `FavoritesPage`, `NearMePage` don't navigate on tap. The §3 recommendation (card-button restyle) was never applied to this checkpoint.

**Fix (applied):** Add `Tapped="OnXTapped"` on the existing `Border` (or `StackPanel` for the contributor avatar template) and route to the VM command from code-behind. Five XAML attributes + four code-behind handler methods, no new style or `ElementName=PageRoot` plumbing.

```xml
<!-- HomePage.xaml — both Trending and Recently Added templates -->
<Border ... Tapped="OnRecipeTapped">…</Border>
<!-- HomePage.xaml — contributor avatar template -->
<StackPanel ... Tapped="OnContributorTapped">…</StackPanel>
<!-- SearchPage.xaml, FavoritesPage.xaml — recipe template -->
<Border ... Tapped="OnRecipeTapped">…</Border>
<!-- FavoritesPage.xaml — cookbook template -->
<Border ... Tapped="OnCookbookTapped">…</Border>
<!-- NearMePage.xaml — chef card -->
<Border ... Tapped="OnChefTapped">…</Border>
```

```csharp
// e.g. HomePage.xaml.cs
private void OnRecipeTapped(object sender, TappedRoutedEventArgs e)
{
    if (sender is FrameworkElement fe && fe.DataContext is Recipe recipe)
        ViewModel?.OpenRecipeCommand.Execute(recipe);
}
private void OnContributorTapped(object sender, TappedRoutedEventArgs e)
{
    if (sender is FrameworkElement fe && fe.DataContext is User user)
        ViewModel?.OpenProfileCommand.Execute(user);
}
```

Tradeoff vs §3: this needs a code-behind file per page (which already exists here). §3 is more declarative (no code-behind) but requires a `RecipeCardButtonStyle` resource and `x:Name="PageRoot"` on every consuming `Page`. Pick by codebase preference; both ship working.

**Code-behind requires `Tapped`, not `element_peer_default_action`.** Discovered during the walkthrough: `uno_app_element_peer_default_action` on a plain `Border` is a no-op (Border has no automation Invoke). Driving these cards from the MCP needs `uno_app_pointer_click` with the bounds from `visualtree_snapshot(includeBounds=true)`. Worth adding to `uno-app-test-assertions` skill notes.

### W3. `CookbookService.SaveAsync` — also add to `_saved`

**Symptom:** Create a cookbook through `CreateCookbookPage` → `_navigator.NavigateBackAsync` returns to Favorites → My Cookbooks tab still shows "No Cookbooks Created". The cookbook *is* in `_cache`; `GetSavedAsync` filters by `_saved`, which is never updated.

**Fix (applied)** in `Services/IRecipeService.cs`:
```csharp
public async Task SaveAsync(Cookbook cookbook)
{
    await EnsureLoadedAsync();
    var existing = _cache!.FirstOrDefault(c => c.Id == cookbook.Id);
    if (existing != null) { existing.Name = cookbook.Name; existing.Recipes = cookbook.Recipes; }
    else
    {
        if (cookbook.Id == Guid.Empty) cookbook.Id = Guid.NewGuid();
        _cache!.Add(cookbook);
    }
    if (!_saved!.Contains(cookbook.Id)) _saved!.Add(cookbook.Id);   // ← added
}
```

### W4. `FavoritesViewModel` — reload cookbooks on tab activation

**Symptom:** Even after W3, the freshly created cookbook didn't appear in the My Cookbooks tab. `FavoritesViewModel.LoadAsync()` only runs once at construction (page is `NavigationCacheMode="Required"`), so the `Cookbooks` ObservableCollection is frozen at construction-time content.

**Fix (applied)** in `Presentation/FavoritesViewModel.cs`:
```csharp
SelectCookbooksCommand = new AsyncRelayCommand(async () =>
{
    SelectedTabIndex = 1;
    await ReloadCookbooksAsync();
});
…
private async Task ReloadCookbooksAsync()
{
    Cookbooks.Clear();
    var saved = await _cookbooks.GetSavedAsync();
    foreach (var c in saved) Cookbooks.Add(c);
    OnPropertyChanged(nameof(HasCookbooks));
    OnPropertyChanged(nameof(HasNoCookbooks));
}
```
Re-runs every time the user re-enters the Cookbooks tab — cheap, ~21 items max.

### Cosmetic (not fixed, captured for next pass)

- **Login username placeholder shows `"Microsoft.UI.Xaml.Controls.FontIcon"`.** Likely a `Header`/`PlaceholderText` binding got the FontIcon DataContext stringified. One-line XAML fix.
- **Search top-bar glyphs** — `&#xE787;` (calendar) and `&#xE721;` (search) where Home uses `&#xE77B;` (person) and `&#xEA8F;` (bell). Commands wired correctly; only the glyphs are swapped. Two-character fix.
- **NearMe map** — `Assets/Maps/location_pin.svg` doesn't render and the chef list shows only one entry. Cousin to learnings #2/#27. Lower priority — page is functional, just sparse.

### Walkthrough fix-pass summary

| # | File | Lines | Status |
|---|---|---|---|
| W1 | `Presentation/MainPage.xaml` | +1 (xmlns), 3 lines retyped | Resolved |
| W2 | `HomePage.xaml(.cs)`, `SearchPage.xaml(.cs)`, `FavoritesPage.xaml(.cs)`, `NearMePage.xaml(.cs)` | +6 attrs, +5 handler methods | Resolved |
| W3 | `Services/IRecipeService.cs` | +1 line in `CookbookService.SaveAsync` | Resolved |
| W4 | `Presentation/FavoritesViewModel.cs` | refactored `LoadAsync` + 1 new method | Resolved |

After all four, the walkthrough captured 19/19 routes plus 4 dark-theme variants in `results/screenshots/walkthrough-2026-04-28/`.
