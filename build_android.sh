#!/usr/bin/env bash
set -euo pipefail

: "${UNITY_EXECUTABLE:?Set UNITY_EXECUTABLE to the Unity 2020.3.18f1 editor executable}"
PROJECT_PATH="${PROJECT_PATH:-$(cd "$(dirname "$0")" && pwd)}"
BUILD_PATH="${BUILD_PATH:-Builds/Jujutsu_Endless_Upgraded.apk}"

mkdir -p "$(dirname "$PROJECT_PATH/$BUILD_PATH")"
exec "$UNITY_EXECUTABLE" \
  -batchmode -nographics -quit \
  -projectPath "$PROJECT_PATH" \
  -buildTarget Android \
  -executeMethod Reconstructed.CloudBuild.BuildAndroid.PerformBuild \
  -logFile -
