# Learnings & Gotchas — ChefsTest6 audit (2026-04-27)

Session: **chefs-correctness-and-arch-pass**
Scope: (b) MVVM/DI/nav hygiene + (c) interaction-correctness pass. Visual styling left at Material defaults — the test is a blind-PRD control, so design must not be "improved." Source for inspiration was *not* drawn from `src/ChefsApp/` (stale reference) or any forbidden Chefs source per `SPEC.md`.

What "verified" means below: live-walked under the `uno-app` MCP unless flagged "static-only."

---

## Defects found and fixed

| ID  | Where                                                          | Defect                                                                                                                                                          | Fix                                                                                                       | Verified           |
| --- | -------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- | ------------------ |
| D1  | `LoginViewModel.cs:53`                                         | `ForgotAsync` was `=> await Task.CompletedTask;` — clicking "Forgot password?" did literally nothing.                                                           | Sets `ErrorMessage` to a "reset link sent (demo build)" message so the click has feedback.                | static + build OK  |
| D2  | `RegisterPage.xaml:22-23`                                      | Form fields rendered at 155px wide on a 1008px screen. `MaxWidth="480" HorizontalAlignment="Center"` only caps width — content-sized children never expand it.  | Added `Width="480"` so the form is a fixed centered column. LoginPage worked only by accident — it has wrapping TextBlocks that pulled width.                 | static + build OK  |
| D5  | `MainPage.xaml`, `RecipeDetailPage.xaml`                       | Tab buttons (bottom nav) and chip tabs (Ingredients/Steps/Reviews/Nutrition) had identical visual state for selected vs. unselected. `IsXxxTab` flags existed in VMs but weren't bound to anything visual. | Added `BoolToBrushConverter`. Bound `Foreground`/`Background` to `IsXxxTab` flags via theme-resource keys (`PrimaryBrush`/`OnPrimaryBrush` etc.). Selected chip is now filled.   | static + build OK  |
| D6  | `RecipeDetailViewModel.cs`, `RecipeDetailPage.xaml`            | `ShareAsync` was a `Task.CompletedTask` no-op. Top-right Share icon did nothing.                                                                                | Removed the dead command. Replaced with a `Button.Flyout` that says "Sharing isn't available in this build." — pure XAML, no VM state.                                | static + build OK  |
| D7  | 10 ViewModels (RecipeDetail, Cooking, CookbookDetail, CreateCookbook, EditCookbook, Settings, Notifications, Map, OtherProfile, Filters) | I initially flagged the `ClearBackStack`-on-every-Back pattern as an anti-pattern and switched to `NavigateBackAsync(this)`. **Reverted after re-reading `results/test-6.log` + `test-6.md`** — the prior session had already discovered that `NavigateBackAsync(this)` does NOT unwind from over-Main routed pages on Uno Skia desktop with Toolkit 6.5.x and chose `ClearBackStack` as a deliberate workaround (documented as Gap 8 in `results/test-6.md`). | **Reverted.** Source restored to `NavigateViewModelAsync<MainViewModel>(this, qualifier: Qualifiers.ClearBackStack)`. The "right" fix is upstream in Uno Toolkit — not in app code. | reverted; build OK |
| D11 | `RecipeDetailPage.xaml.cs`, `.xaml`                            | Like/Dislike used codebehind `Click` handlers (`OnLikeClicked`/`OnDislikeClicked`) that forwarded to VM commands. Pure boilerplate, violated MVVM.              | Added `x:Name="RecipeDetailRoot"` on the page root Grid. Bound buttons via `Command="{Binding DataContext.ToggleLikeCommand, ElementName=RecipeDetailRoot}" CommandParameter="{Binding}"`. Codebehind reduced to constructor only. | static + build OK  |
| D14 | `MapPage.xaml:60`                                              | `<WrapGrid Orientation="Horizontal" />` — `WrapGrid.Orientation` is **not implemented in Uno** (build emitted `Uno0001` warning). Panel defaulted to vertical layout. | Replaced `ItemsControl` + `WrapGrid` with `muxc:ItemsRepeater` + `muxc:UniformGridLayout`, matching the pattern already used in FavoritesView/ProfileView/SearchView. | warning gone in build |
| D8/D9/D10 | `MainViewModel.cs`, `LoginViewModel.cs`, `RecipeDetailViewModel.cs` | Several `[RelayCommand] async Task X() => await Task.CompletedTask;` shells. Fakes async-ness for sync-only logic.                                              | Converted `OpenCategoryAsync` and `ToggleFavoriteAsync` on `MainViewModel` to sync `void` (command name unchanged). Updated `IAsyncRelayCommand<T>` → `IRelayCommand<T>` re-exports on `HomeTabViewModel`/`SearchTabViewModel`/`FavoritesTabViewModel`/`ProfileTabViewModel`. | static + build OK  |

## Open observations (left intentionally)

| #   | Where                              | Observation                                                                                                                                                                                              |
| --- | ---------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| O1  | every `*.xaml` page-root binding    | Page-level bindings use `{Binding}` instead of `{x:Bind}`. Inside `DataTemplate`s, `x:Bind` is used correctly. Switching root-level requires adding `x:DataType` to each `<Page>` and converting ~50+ expressions. Per global CLAUDE.md, `x:Bind` is preferred — flagged as a known divergence; mechanical migration deferred.                                                                                  |
| O2  | `MainPage.xaml`                     | Bottom tab bar is 4 raw `Button`s in a `Grid`. Per project CLAUDE.md, `Uno.Toolkit.UI.TabBar` is preferred. Works correctly today; refactor is a chunkier change. Deferred.                                                                                                  |
| O3  | `App.xaml.cs:55-60`                  | `MainWindow.ExtendsContentIntoTitleBar = false;` is set explicitly with a comment about Skia desktop title-bar drag eating clicks on `NavigationBar` back chevrons. Confirmed working — the Register back chevron clicked through correctly during the live walk.                                                                                  |
| O4  | `MapPage` — fake "map"               | The map is an `ItemsRepeater` of avatar circles, no real maps integration. Out of scope per blind-PRD test; just noting.                                                                                  |
| O5  | dependency advisories                | `dotnet build` emits 3 NU1903 high-severity advisories (`System.Security.Cryptography.Xml` 10.0.2, `Tmds.DBus.Protocol` 0.21.2). Transitive — not introduced by app code. Not in scope for the chefs-app correctness pass. |
| O6  | `src/ChefsApp/`                      | Stale folder per user — *not* a forbidden source, but ignored anyway. Don't read for inspiration.                                                                                                         |

## Gotchas to remember

### Uno / WinUI XAML

1. **`MaxWidth + HorizontalAlignment="Center"` does not center-and-stretch a form**. With Center alignment, the parent shrinks to content's natural width, and `MaxWidth` only caps. If your form's children are content-sized (`TextBox`, `PasswordBox`), the form will collapse to a tiny width. Use `Width="480"` + `HorizontalAlignment="Center"` for fixed centered forms, or a 3-column Grid (`* | content | *`) for responsive ones.

2. **`WrapGrid.Orientation` is not implemented in Uno**. Uno0001 build warning. Use `muxc:ItemsRepeater` + `muxc:UniformGridLayout` (cross-platform) instead. The codebase already uses this pattern in 3 other places — copy it. `WrapGrid` itself works, but `Orientation` does nothing, so the panel falls back to vertical.

3. **Image `UniformToFill` reports natural source size in the visual tree, even when clipped**. E.g., `<Image Stretch="UniformToFill" />` inside a 220-tall container will show `bounds = ...,1888` (the source image height) in `uno_app_visualtree_snapshot`. Visually fine — the parent clips. Don't panic at the bounds.

4. **`{x:Bind}` inside DataTemplates needs `x:DataType`** (already used correctly throughout). Page-level `{x:Bind}` requires `x:DataType` on the `<Page>` itself, which the project hasn't adopted yet. That's why all root bindings use `{Binding}` despite the global preference for `x:Bind`.

5. **Binding to a parent VM from inside an `ItemsControl`/`ItemsRepeater` `DataTemplate`** — the item's DataContext is the item, not the page VM. Pattern: name the page root (`x:Name="..."`) and use `Command="{Binding DataContext.SomeCommand, ElementName=PageRoot}" CommandParameter="{Binding}"`. Avoids codebehind `Click` shims that just forward to commands.

### Uno.Extensions Navigation

6. **On Uno Skia desktop, `NavigateBackAsync(this)` from a routed VM doesn't unwind to Main.** This is the load-bearing gotcha for ChefsTest6's whole back-nav pattern. The working workaround on this stack is explicit `NavigateViewModelAsync<MainViewModel>(qualifier: Qualifiers.ClearBackStack)` from every BackAsync/CancelAsync/CloseAsync, plus a `<utu:NavigationBar.MainCommand>` override on every routed page (so the Toolkit's broken auto-chevron is bypassed). Documented as Gap 8 in `results/test-6.md`. The "correct" upstream fix is in Toolkit/Uno.Extensions; the workaround is correct for the app code.

7. **Read `results/test-N.log` + `test-N.md` BEFORE "fixing" anti-patterns.** I initially flagged `ClearBackStack`-on-every-Back as an anti-pattern and changed it back to `NavigateBackAsync(this)`. Build was clean. But the prior session had documented the same change as a deliberate workaround for the Skia gotcha above, and my "fix" would have re-broken back navigation. Reverted in the same session. Lesson: when prior code looks unidiomatic, the prior log is the first place to look.

8. **For genuine terminals on any stack**, `ClearBackStack` is correct: post-login, post-signup, post-onboarding-finish, post-cooking-done, logout. The workaround case above is the additional reason it shows up in BackAsync/CancelAsync on Skia desktop.

### `uno-app` MCP

9. **The MCP only attaches once per session**. If you `taskkill` the app and `dotnet run` again in the same session, `uno_app_get_runtime_info` may stop attaching. Workaround: limit kills, or accept that you only get one live-walk before edits and trust `dotnet build` after. (Documented this session — first run attached fine; second/third didn't.)

10. **`uno_app_close` returns an error on Skia desktop**. Use `taskkill //F //IM ChefsTest6.exe` instead. The MCP-side close is unreliable cross-platform.

11. **`dotnet run -f net10.0-desktop` writes to OneDrive paths**. Path with spaces; remember to quote in scripts. The hot-reload `obj/uno.reload.cookie.g.cs` lives under each TFM's `obj/` — not a problem, just noisy in glob output.

12. **`uno_app_get_screenshot` can degrade to 0-byte returns mid-session** while `uno_app_visualtree_snapshot` and `uno_app_get_element_datacontext` keep working. Observed **once** during the post-fix verification walk — after a Login → Main navigation, every subsequent screenshot returned `Image file is empty (0 bytes)` for ~6 attempts across multiple page transitions. Closing and `uno_app_start`-ing again immediately recovered the screenshot subsystem on the same MCP connection. Distinct from gotcha #9 (MCP-side reattach failure): here the MCP itself is healthy, only the screenshot pipeline degrades. **Status:** observed once. Watch for repeats; if it happens again, file an upstream issue with the navigation step that triggered it.

### CommunityToolkit.Mvvm

12. **`[RelayCommand]` strips `Async` from method name**. `OpenCategoryAsync` and `OpenCategory` both produce `OpenCategoryCommand`. Renaming to remove `Async` (when converting to sync) doesn't break XAML bindings — but the **command type** changes from `IAsyncRelayCommand<T>` to `IRelayCommand<T>`, breaking re-export properties on parent VMs. Update them at the same time.

## Verification status

| Phase                              | Status      | Notes                                                                                                                                                                                                                                                                  |
| ---------------------------------- | ----------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Pre-fix runtime walk               | done        | Onboarding (3 slides + Skip + Back), Login (Forgot is dead, Register link), Register (back-chevron), Login + Sign In, Main/Home, RecipeDetail, Cooking (steps 1→3), CookingDone. Screenshots in `docs/audit-screenshots/`.                                                |
| Pre-fix static read of all VMs+XAML | done        | Every ViewModel and major page XAML read once. Notes in this file.                                                                                                                                                                                                       |
| Build after fixes                  | clean       | 0 errors, 3 warnings (transitive deps), `Uno0001 WrapGrid.Orientation` warning eliminated.                                                                                                                                                                                |
| Post-fix runtime walk               | **DONE** (after MCP reconnect) | After the user reconnected the `uno-app` MCP, walked: FP #12 visually (Home tab purple, others gray; Ingredients chip filled, others outlined); FP #14 (Register form 480px confirmed via visual-tree bounds); FP #15 (Niki Samantha Like count 3→4 after click); FP #16 (Map page renders `<ItemsRepeater>` with 6 chef avatars); FP #17 (Forgot click → "Reset link sent..." red text); FP #18 (Share click → "Sharing isn't available..." flyout); FP #19 (DataContext shows `OpenCategoryCommand` and `ToggleFavoriteCommand` as `RelayCommand` not `AsyncRelayCommand`); FP #20 (back from Recipe Detail and Map both return to Home tab cleanly). Screenshots in `docs/audit-screenshots/post-fix/`. |

## Pages audited

Walked live: Onboarding, Login, Register (back), Main/Home, RecipeDetail (Ingredients + Steps tabs), Cooking, CookingDone.

Static-only (read VM + XAML, no live walk in this session): Search, Favorites, Profile, Settings, Notifications, Map, OtherProfile, CookbookDetail, CreateCookbook, EditCookbook, Filters.
