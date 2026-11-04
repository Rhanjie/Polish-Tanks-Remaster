#!/usr/bin/env bash
# Prints the game version from Player Settings (ProjectSettings.asset -> bundleVersion).
set -euo pipefail
version=$(grep -m1 -E '^\s*bundleVersion:' ProjectSettings/ProjectSettings.asset | awk '{print $2}' | tr -d '\r')
if [[ ! "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "::error file=ProjectSettings/ProjectSettings.asset::bundleVersion '$version' is not MAJOR.MINOR.PATCH (set it in Player Settings)" >&2
  exit 1
fi
echo "$version"
