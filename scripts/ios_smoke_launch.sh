#!/usr/bin/env bash
#
# Smoke test: installs the built app on an iOS simulator, launches it and
# checks that it is still running after a while.
#
# The console output, the simulator log, crash reports, the app's own log
# and a screenshot go to the output directory (CI uploads it as an artifact).
#
# Usage: ios_smoke_launch.sh <path to .app> [output directory]
#
# Needs macOS with Xcode and jq. Covers the app start only (the login page):
# nothing is tapped or typed.
#
# Environment:
#   SMOKE_RUN_SECONDS   how long the app has to stay alive (default: 40)
#   SMOKE_DEVICE_UDID   simulator to use (default: an iPhone of the newest runtime)

set -uo pipefail

APP_PATH="${1:-}"
OUT_DIR="${2:-ios-smoke}"
RUN_SECONDS="${SMOKE_RUN_SECONDS:-40}"

summary() {
  echo "$1"

  if [ -n "${GITHUB_STEP_SUMMARY:-}" ]; then
    echo "$1" >> "$GITHUB_STEP_SUMMARY"
  fi
}

fail() {
  summary "iOS smoke test: $1"
  exit 1
}

if [ -z "$APP_PATH" ] || [ ! -d "$APP_PATH" ]; then
  fail "the .app bundle is not found ('$APP_PATH')."
fi

command -v jq > /dev/null || fail "jq is required."

mkdir -p "$OUT_DIR"
START_MARKER="$OUT_DIR/.start"
touch "$START_MARKER"

BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleIdentifier" "$APP_PATH/Info.plist")
EXECUTABLE=$(/usr/libexec/PlistBuddy -c "Print :CFBundleExecutable" "$APP_PATH/Info.plist")

if [ -z "$BUNDLE_ID" ] || [ -z "$EXECUTABLE" ]; then
  fail "can't read the bundle id or the executable name from Info.plist."
fi

echo "App: $APP_PATH ($BUNDLE_ID, executable $EXECUTABLE)"

# An iPhone of the newest installed iOS runtime.
UDID="${SMOKE_DEVICE_UDID:-}"

if [ -z "$UDID" ]; then
  UDID=$(xcrun simctl list devices available -j | jq -r '
    [ .devices | to_entries[]
      | select(.key | test("iOS"))
      | .key as $runtime
      | .value[]
      | select(.name | startswith("iPhone"))
      | { runtime: $runtime, udid: .udid } ]
    | sort_by(.runtime) | last | .udid // empty')
fi

if [ -z "$UDID" ]; then
  xcrun simctl list devices available
  fail "no available iPhone simulator."
fi

echo "Simulator: $UDID"
xcrun simctl list devices available | grep "$UDID" || true

xcrun simctl boot "$UDID" 2> /dev/null || true
xcrun simctl bootstatus "$UDID" -b || fail "the simulator did not boot."

# The bundle is installed as it was built (the build signs it ad hoc) and
# is not signed again here. The signature check is only logged.
{
  codesign --display --verbose=2 "$APP_PATH"
  codesign --verify --deep --strict --verbose=2 "$APP_PATH"
  echo "codesign verify exit code: $?"
} > "$OUT_DIR/codesign.log" 2>&1 || true

if ! xcrun simctl install "$UDID" "$APP_PATH" > "$OUT_DIR/install.log" 2>&1; then
  cat "$OUT_DIR/install.log"
  fail "the app could not be installed on the simulator."
fi

# Pid of the app process in the simulator (empty if it is not running).
app_pid() {
  xcrun simctl spawn "$UDID" launchctl list 2> /dev/null |
    awk -v id="UIKitApplication:$BUNDLE_ID" 'index($3, id) == 1 && $1 ~ /^[0-9]+$/ { print $1; exit }'
}

# --console blocks until the app exits and prints its stdout/stderr:
# an unhandled .NET exception is written there before the process aborts.
xcrun simctl launch --console --terminate-running-process "$UDID" "$BUNDLE_ID" \
  > "$OUT_DIR/console.log" 2>&1 < /dev/null &
LAUNCH_PID=$!

ELAPSED=0
IS_RUNNING=1

while [ "$ELAPSED" -lt "$RUN_SECONDS" ]; do
  sleep 2
  ELAPSED=$((ELAPSED + 2))

  # Both checks: the launcher has ended and the process is gone.
  if ! kill -0 "$LAUNCH_PID" 2> /dev/null && [ -z "$(app_pid)" ]; then
    IS_RUNNING=0
    break
  fi
done

xcrun simctl io "$UDID" screenshot "$OUT_DIR/screen.png" > /dev/null 2>&1 || true

if [ "$IS_RUNNING" -eq 1 ]; then
  xcrun simctl terminate "$UDID" "$BUNDLE_ID" > /dev/null 2>&1 || true
else
  # The crash report is written a few seconds after the crash.
  sleep 15
fi

wait "$LAUNCH_PID" 2> /dev/null

xcrun simctl spawn "$UDID" log show --last 10m --style compact \
  --predicate "process == \"$EXECUTABLE\"" > "$OUT_DIR/simulator.log" 2>&1 || true

REPORTS_DIR="$HOME/Library/Logs/DiagnosticReports"

if [ -d "$REPORTS_DIR" ]; then
  find "$REPORTS_DIR" -maxdepth 1 -type f -name "${EXECUTABLE}*" -newer "$START_MARKER" \
    -exec cp {} "$OUT_DIR/" \;
fi

DATA_DIR=$(xcrun simctl get_app_container "$UDID" "$BUNDLE_ID" data 2> /dev/null || true)

if [ -n "$DATA_DIR" ] && [ -d "$DATA_DIR" ]; then
  find "$DATA_DIR" -type f -name "app_logs.txt" -exec cp {} "$OUT_DIR/app_logs.txt" \;
fi

xcrun simctl shutdown "$UDID" > /dev/null 2>&1 || true
rm -f "$START_MARKER"

echo "----- console output (last 80 lines) -----"
tail -n 80 "$OUT_DIR/console.log" || true
echo "----- files -----"
ls -la "$OUT_DIR"

if [ "$IS_RUNNING" -eq 1 ]; then
  summary "iOS smoke test: the app started and was still running after ${ELAPSED} s."
  exit 0
fi

summary "iOS smoke test: the app ended ${ELAPSED} s after the launch (crash on start)."

if [ -n "${GITHUB_STEP_SUMMARY:-}" ]; then
  {
    echo
    echo "Console output (errors):"
    echo
    echo '```'
    grep -i -E "unhandled|exception|terminating app|fatal|abort|dyld|error" "$OUT_DIR/console.log" | head -n 60
    echo '```'
    echo
    echo "Full logs: the ios-smoke-logs artifact."
  } >> "$GITHUB_STEP_SUMMARY"
fi

exit 1
