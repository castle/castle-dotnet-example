#!/usr/bin/env bash
# Point the example apps at a specific Castle .NET SDK source.
#
#   set-sdk-version.sh main      -> build the SDK's main branch from source and consume it locally
#   set-sdk-version.sh <branch>  -> same, for any castle-dotnet branch (e.g. a release branch)
#   set-sdk-version.sh 3.0.0     -> pin the released Castle.Sdk 3.0.0 package from NuGet
#
# For a branch ref this clones castle-dotnet, packs it into ./.sdk-feed, registers
# that folder as a NuGet source (nuget.config) and rewrites the Castle.Sdk
# PackageReference versions in both sample projects. net48 only builds on Windows,
# so off Windows the SDK is packed for net8.0 only (enough for the ASP.NET Core sample).
set -euo pipefail

target="${1:?usage: set-sdk-version.sh <main|branch|X.Y.Z>}"
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

csprojs=(
  "src/CastleDemo/CastleDemo.csproj"
  "src/CastleDemo.Framework/CastleDemo.Framework.csproj"
)

set_reference_version() {
  local version="$1"
  for proj in "${csprojs[@]}"; do
    perl -0pi -e 's{(<PackageReference\s+Include="Castle\.Sdk"\s+Version=")[^"]*(")}{${1}'"$version"'${2}}g' "$proj"
  done
}

# Released semantic version: pin it from NuGet and drop any local feed.
if [[ "$target" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  rm -f nuget.config
  rm -rf .sdk-feed
  set_reference_version "$target"
  echo "Pinned Castle.Sdk $target from NuGet."
  exit 0
fi

# Branch ref: clone, pack from source and consume the package locally.
branch="$target"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

git clone --depth 1 --branch "$branch" https://github.com/castle/castle-dotnet.git "$work"

version="$(perl -ne 'print $1 if /<Version>([^<]+)<\/Version>/' "$work/src/Castle.Sdk/Castle.Sdk.csproj")"
: "${version:?could not read <Version> from the SDK csproj}"

feed="$repo_root/.sdk-feed"
rm -rf "$feed"
mkdir -p "$feed"

pack_args=(-c Release -o "$feed")
case "${RUNNER_OS:-$(uname -s)}" in
  Windows | *NT* | MINGW* | MSYS*) ;; # Windows: pack every target framework, including net48
  *) pack_args+=(-p:TargetFrameworks=net8.0) ;;
esac
dotnet pack "$work/src/Castle.Sdk/Castle.Sdk.csproj" "${pack_args[@]}"

cat > nuget.config <<'XML'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="local-sdk" value=".sdk-feed" />
  </packageSources>
</configuration>
XML

set_reference_version "$version"
echo "Built Castle.Sdk $version from '$branch' and wired the local .sdk-feed source."
