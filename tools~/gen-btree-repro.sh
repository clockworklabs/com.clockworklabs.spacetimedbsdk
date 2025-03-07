#!/usr/bin/env bash

set -ueo pipefail

STDB_PATH="$1"
SDK_PATH="$(dirname "$0")/.."
SDK_PATH="$(realpath "$SDK_PATH")"

cargo run --manifest-path "$STDB_PATH/crates/cli/Cargo.toml" -- generate -y -l csharp -o "$SDK_PATH/examples~/btree-repro/client/module_bindings" --project-path "$SDK_PATH/examples~/btree-repro/server"
