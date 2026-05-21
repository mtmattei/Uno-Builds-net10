#!/bin/bash
# Wave 1 — 10 desktop-only projects, SDK bump only
set -e
repo="/c/temp/Uno-Builds-net10"
cd "$repo"

# name|root (root = where global.json lives, dotnet runs from here)
projects=(
  "Caffe|Caffe"
  "Nexus|Nexus"
  "Olea|Olea"
  "Orbital|Orbital"
  "Riviera|Riviera/RivieraHome"
  "WinampClassic|WinampClassic"
  "MCP-blog|MCP-blog"
  "listhold|listhold"
  "fitnesstracker|fitnesstracker"
  "Vitalis|Vitalis"
)

log="$repo/UPGRADE-LOG.md"
if [ ! -f "$log" ]; then
  printf '# Upgrade log\n\n| Wave | Project | From | To | Result | Commit |\n|---|---|---|---|---|---|\n' > "$log"
fi

target_sdk="6.5.33"

for entry in "${projects[@]}"; do
  name="${entry%%|*}"
  root="${entry#*|}"
  gj="$repo/$root/global.json"

  echo ""
  echo "============================================"
  echo "Wave 1 :: $name  (root: $root)"
  echo "============================================"

  if [ ! -f "$gj" ]; then echo "SKIP: no global.json at $gj"; continue; fi

  current=$(grep -oE '"Uno\.Sdk"\s*:\s*"[^"]+"' "$gj" | sed -E 's/.*"([^"]+)"$/\1/')
  echo "  current Uno.Sdk: $current"

  if [ "$current" = "$target_sdk" ]; then
    echo "  already at $target_sdk — skipping"
    printf '| 1 | %s | %s | %s | skipped | - |\n' "$name" "$current" "$target_sdk" >> "$log"
    continue
  fi

  # Update global.json (preserve formatting, use perl for in-place edit)
  perl -i -pe 's/"Uno\.Sdk"\s*:\s*"[^"]+"/"Uno.Sdk": "'"$target_sdk"'"/' "$gj"

  # Verify the edit
  newver=$(grep -oE '"Uno\.Sdk"\s*:\s*"[^"]+"' "$gj" | sed -E 's/.*"([^"]+)"$/\1/')
  if [ "$newver" != "$target_sdk" ]; then echo "  ERROR: edit didn't take ($newver)"; exit 1; fi
  echo "  edited global.json -> $target_sdk"

  pushd "$repo/$root" > /dev/null
  echo "  dotnet restore..."
  if ! dotnet restore > /tmp/restore.log 2>&1; then
    echo "  RESTORE FAILED"
    tail -25 /tmp/restore.log
    popd > /dev/null
    exit 1
  fi

  echo "  dotnet build -f net10.0-desktop --no-restore..."
  if ! dotnet build -f net10.0-desktop --no-restore --nologo > /tmp/build.log 2>&1; then
    echo "  BUILD FAILED"
    tail -40 /tmp/build.log
    popd > /dev/null
    exit 1
  fi
  echo "  build OK"
  popd > /dev/null

  # Commit
  cd "$repo"
  git add "$root/global.json"
  msg="chore($name): bump to Uno.Sdk $target_sdk

Was $current. Wave 1 — desktop-only SDK bump, no TFM change.
desktop build: ok"
  git commit -m "$msg" > /dev/null
  sha=$(git rev-parse --short HEAD)
  echo "  committed $sha"
  printf '| 1 | %s | %s | %s | ok | %s |\n' "$name" "$current" "$target_sdk" "$sha" >> "$log"
done

echo ""
echo "=== Wave 1 done ==="
git log --oneline -12
