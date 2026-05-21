# test-2 fixes — Figma MCP only

> Project folder: `test-2-figma-mcp/ChefsTest2/`
> Run grade: **C** (58/100). 🔁 re-run still required for failure mode #1 (skill discipline).
> Pass-bar status: 1 ✅ (after 7 FPs), 2 ⚠ (17/20 walked), 3 ❌ (Chefs-screenshots/ forbidden).
>
> **Read `COMMON-FIXES.md` first.** Items below are unique to this run.

## Methodology fixes (the load-bearing ones for this run)

### 1. Skill discipline failure mode #1 — re-run with skills called *in-flow*

**Symptom:** All 9 SKILL-USE log lines were appended retroactively after the user explicitly asked whether skills had been invoked. Implementation drove from rules-init + base reasoning. Same shape as the original test-1 / test-6 pre-discipline runs.

**Fix:** Re-run the methodology with each `Skill` tool call issued *before* writing code in that domain. The audit gate at workflow step 10.5 (per parent SPEC) cross-checks:
- count of actual `Skill` tool calls in transcript
- count of `SKILL-USE` lines in `test-2.log`
- skill-usage table `Invocations` column sum

All three must agree. The retroactive logging produced equal counts but hid the in-flow gap.

### 2. Figma MCP rate-limit on a View seat — methodology cost

**Symptom:** ~5 calls into Figma MCP (`get_variable_defs` + 4× `get_design_context` + 1× `get_screenshot`), the API returned `"You've reached the Figma MCP tool call limit for your View seat on the Organization plan."` 26 of 30 frames in the design file went un-retrieved.

**Fix:** Either (a) upgrade to a higher Figma seat tier and re-run, or (b) downgrade methodology to "Figma MCP for tokens, screenshots/visual-skill DESIGN.md for the 26 unfetched frames" — the cleanest tokens (chef-pink `#E8455C` light / `#FFA3B7` dark, secondary cream `#EAE3D6` / olive `#494737`, surface inverse `#2D2D2D`, Roboto type scale) all came from Figma; the layouts that Material defaults under-served are the ones that needed the screenshots.

For the public stunt: **don't ship "Figma MCP only" as a measured methodology unless you have an Editor seat or higher.**

## Anti-patterns shipping live (4 noted)

### 3. Tab-swap clears back stack

**Symptom:** `BottomNavBar` uses `_navigator.NavigateRouteAsync('-/Route')` on each tab tap. The `-/` qualifier clears the back stack, so a deep drill from inside Home (Home → RecipeDetail → LiveCooking) gets erased when the user taps the Search tab and back.

**Fix:** Switch to region-based navigation. Wrap Main page tab content in a parent with `Region.Navigator="Visibility"`:
```xml
<Page x:Class="ChefsTest2.Presentation.MainPage" ...
      uen:Region.Attached="True">
  <Grid uen:Region.Navigator="Visibility">
    <ContentControl uen:Region.Name="Home"      x:Load="True"/>
    <ContentControl uen:Region.Name="Search"    x:Load="False"/>
    <ContentControl uen:Region.Name="Favorites" x:Load="False"/>
  </Grid>
  <utu:TabBar Style="{StaticResource BottomTabBarStyle}">
    <utu:TabBarItem uen:Region.Name="Home"      Content="Home" .../>
    <utu:TabBarItem uen:Region.Name="Search"    Content="Search" .../>
    <utu:TabBarItem uen:Region.Name="Favorites" Content="Favorites" .../>
  </utu:TabBar>
</Page>
```

⚠ See COMMON-FIXES Tier-1 #6 for the Skia desktop trap. Smoke-test on Skia desktop *before* declaring this fix done; if regions render empty, fall back to flat routing under Shell (test-7's pivot in FP #3/#4).

### 4. NearMeMap SVG markers transparent on Skia

**Symptom:** `Maps/location_pin.svg` and `Maps/location_circle.svg` referenced via `<Image Source="ms-appx:///Assets/Maps/location_pin.svg"/>` render transparent. FP #6 substituted a placeholder; SVGs not re-fixed within budget.

**Fix:** Either (a) re-export those specific SVGs with proper viewBox (test-7 found that some `reference/assets/` SVGs have malformed viewBox attrs and render blank even with `Svg` enabled), or (b) replace with `PathIcon` using inline data:
```xml
<PathIcon Data="M12,2 C8.13,2 5,5.13 5,9 C5,14.25 12,22 12,22 C12,22 19,14.25 19,9 C19,5.13 15.87,2 12,2 Z M12,11.5 ..."
          Width="48" Height="48"
          Foreground="{ThemeResource PrimaryBrush}"/>
```

Or (c) ship `Mapsui.Uno.WinUI` for a real map with native pins — see COMMON-FIXES Tier 2.E.

### 5. Persistence in-memory only (no JSON write-back)

**Symptom:** `IDataService.ToggleFavoriteRecipe` / `ToggleSavedCookbook` mutate in-memory `HashSet`s; `JsonDataService` doesn't write back. Settings Save / Apply navigate back without persisting.

**Fix:** Acceptable as-is per API-CONTRACT.md ("free choice of mechanism"). If durability matters, write to `ApplicationData.Current.LocalFolder` instead of `ms-appx:///` (which is read-only). Pattern:
```csharp
var folder = ApplicationData.Current.LocalFolder;
var file = await folder.CreateFileAsync("favorites.json", CreationCollisionOption.ReplaceExisting);
await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(favorites));
```

### 6. Live Cooking surrogate (image hero + ProgressBar, no MediaPlayer)

**Symptom:** Hero `Image` + play-icon overlay + `ProgressBar` scrubber. UrlVideo string never wired to a real player.

**Fix:** Replace with real `MediaPlayerElement` — see COMMON-FIXES Tier 2.F.

## Visual fidelity gaps from the rate-limit

### 7. Tablet variants not implemented

**Symptom:** Single phone-style layout at all viewports. Figma rate-limit blocked tablet variant retrieval.

**Fix:** This is the structural cost of the View-seat methodology. Either upgrade and re-pull tablet frames, or apply visual-state fixes by inspection from the screenshots set (which would defeat the "Figma MCP only" methodology label).

### 8. NavigationBar surface inversion not wired into utu:NavigationBar styles

**Symptom:** `#2D2D2D` light / `#E3E5E8` dark are in `ColorPaletteOverride.xaml` (correctly pulled from Figma `Dark/Surface/SurfaceInverseColor`), but the `utu:NavigationBar` default style doesn't reference `SurfaceInverseBrush` — pages with `ChefsNavigationBarStyle` show default Material chrome instead of the inverted near-black.

**Fix:** Define a lightweight style override:
```xml
<Style x:Key="ChefsNavigationBarStyle" TargetType="utu:NavigationBar"
       BasedOn="{StaticResource MaterialNavigationBarStyle}">
  <Setter Property="Background" Value="{ThemeResource SurfaceInverseBrush}"/>
  <Setter Property="Foreground" Value="{ThemeResource OnSurfaceInverseBrush}"/>
</Style>
```

Apply this style on Home, Search, Favorites, Recipe, LiveCooking, Profile, Cookbooks pages.

## Visual polish

### 9. Profile description vertical 1-char-per-line wrap (FIXED in FP #8/#9)

Already fixed. Mentioned for completeness — was a TextWrapping issue resolved by setting `TextWrapping="Wrap"` on the description `TextBlock` and giving its `Grid.Column` an explicit `MinWidth` so it doesn't collapse to 1ch.

### 10. Settings + Profile pre-fill momentary empty (async LoadAsync timing)

**Symptom:** Form fields render empty for the first few hundred ms while `LoadAsync` hydrates the current user. Visible during screenshot capture.

**Fix:** Add a loading state branch:
```xml
<Grid>
  <ProgressRing IsActive="{x:Bind ViewModel.IsLoading, Mode=OneWay}"
                Visibility="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource BoolToVis}}"/>
  <StackPanel Visibility="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource InverseBoolToVis}}">
    <!-- form fields -->
  </StackPanel>
</Grid>
```

## What's done / acceptable as-is

- All 5 builds green (after 7 FPs) ✅
- 20/20 routes registered, 17/20 walked, 3 reachable-but-not-screenshot-verified ✅
- Chef-pink palette correctly mapped across both themes from the 4 Figma frames retrieved ✅
- Roboto type scale + UNO Semantic typography keys ✅
- 4-tab Recipe Detail with Pivot ✅
- Card-tap navigation working (Click handlers wired on all recipe / cookbook / contributor cards) ✅
- 5/5 empty states implemented ✅

## Priority order for next iteration

1. **Skill discipline re-run (#1)** — required to close the 🔁 flag. Until this lands, the methodology label "Figma MCP only" is unreliable.
2. **Tab-swap back stack (#3)** — functional fix; otherwise back-from-deep-drill loses context on tab-switch.
3. **Live Cooking media player (#6)** — visible fidelity gap; bundled CookingVideo.mp4 makes it easy.
4. **NearMeMap SVG transparency (#4)** — switch to PathIcon or Mapsui.
5. **Tablet variants (#7)** — only if Figma seat is upgraded; otherwise structurally blocked.
