#!/usr/bin/env bash
# fetch-report.sh
#
# Runs on the machine where YOU use Codex (your laptop or a coding box), from
# inside your CODE repo. It pulls the latest Hermes report, shows you what is in
# it so you can read it first, and copies it into ./.hermes-report/ where Codex
# can read it. It never starts Codex. You do that yourself, which is the gate.
#
# Usage:
#   fetch-report.sh                 newest report
#   fetch-report.sh --list          list the 15 newest reports and stop
#   fetch-report.sh NAME.md         a specific report
#   fetch-report.sh --full          print the whole report instead of the first 60 lines
#
# Config (environment variables):
#   REPORTS_REPO_URL   git URL of the reports repo (needed for the first clone)
#   REPORTS_REPO_DIR   where the clone lives (default: ~/hermes-reports)

set -Eeuo pipefail

REPORTS_REPO_DIR="${REPORTS_REPO_DIR:-$HOME/hermes-reports}"
INTO=".hermes-report"
MODE="newest"
FULL=0
NAME=""

while [ $# -gt 0 ]; do
  case "$1" in
    --list) MODE="list" ;;
    --full) FULL=1 ;;
    -h | --help) sed -n '2,17p' "$0"; exit 0 ;;
    -*) echo "unknown option: $1" >&2; exit 2 ;;
    *) NAME="$1" ;;
  esac
  shift
done

if [ ! -d "$REPORTS_REPO_DIR/.git" ]; then
  : "${REPORTS_REPO_URL:?first run: set REPORTS_REPO_URL to clone the reports repo}"
  git clone --quiet "$REPORTS_REPO_URL" "$REPORTS_REPO_DIR"
else
  git -C "$REPORTS_REPO_DIR" pull --ff-only --quiet
fi

REPORTS="$REPORTS_REPO_DIR/reports"
[ -d "$REPORTS" ] || { echo "no reports/ folder in the repo yet" >&2; exit 1; }

# Report names start with a timestamp, so sorting by name sorts by time.
list_reports() {
  (cd "$REPORTS" && find . -type f -not -name '.*' -not -path '*/.*' -print0 | sort -z) |
    tr '\0' '\n' | sed 's|^\./||'
}

if [ "$MODE" = "list" ]; then
  list_reports | tail -n 15
  exit 0
fi

if [ -z "$NAME" ]; then
  NAME="$(list_reports | tail -n 1)"
  [ -n "$NAME" ] || { echo "no reports found" >&2; exit 1; }
fi

# Re-check the name here too. Do not trust what came out of the repo.
if ! [[ "$NAME" =~ ^[A-Za-z0-9][A-Za-z0-9._/-]*$ ]] || [[ "$NAME" == *..* ]]; then
  echo "refusing unsafe report name" >&2
  exit 1
fi
SRC="$REPORTS/$NAME"
if [ ! -f "$SRC" ] || [ -L "$SRC" ]; then
  echo "not a regular file: $NAME" >&2
  exit 1
fi

commit="$(git -C "$REPORTS_REPO_DIR" log -1 --format='%h %cd' --date=short -- "reports/$NAME" || true)"
echo "Report:  $NAME"
echo "Size:    $(wc -c <"$SRC") bytes"
echo "SHA-256: $(sha256sum "$SRC" | cut -d' ' -f1)"
echo "Commit:  ${commit:-unknown}"
echo

# Heuristic skim aid. It catches lazy attacks and sloppy reports. It does NOT
# make a report safe, so you still read the thing.
flags='ignore (all |any |the )?(previous|prior|above)|disregard|curl[^|]*\|[^|]*(sh|bash)|wget[^|]*\|[^|]*(sh|bash)|base64 -d|rm -rf|chmod 777|authorized_keys|\.ssh/|api[_-]?key|BEGIN [A-Z ]*PRIVATE KEY|you (must|should) (now )?(run|execute|install)'
flagfile="$(mktemp)"
trap 'rm -f "$flagfile"' EXIT
if grep -niE -e "$flags" -- "$SRC" >"$flagfile" 2>/dev/null; then
  echo "!! Lines worth a careful look:"
  sed 's/^/   /' "$flagfile"
  echo
fi

if [ "$FULL" -eq 1 ]; then
  cat -- "$SRC"
else
  head -n 60 -- "$SRC"
  total="$(wc -l <"$SRC")"
  [ "$total" -gt 60 ] && echo "... ($total lines total, rerun with --full to see everything)"
fi
echo

mkdir -p "$INTO"
cp -- "$SRC" "$INTO/$(basename "$NAME")"
# Keep the copy out of your code repo's commits.
if exclude="$(git rev-parse --git-path info/exclude 2>/dev/null)"; then
  mkdir -p "$(dirname "$exclude")"
  grep -qxF "$INTO/" "$exclude" 2>/dev/null || echo "$INTO/" >>"$exclude"
fi

echo "Copied to $INTO/$(basename "$NAME")"
echo "When you have read it, start Codex yourself and tell it what to do, for example:"
echo "  codex \"Read $INTO/$(basename "$NAME"). Do task 1 only, on a new branch from hermes-dev. Do not push.\""
