#!/usr/bin/env bash

# This script requires a running local SpacetimeDB instance.

set -ueo pipefail

STDB_PATH="$1"
SDK_PATH="$(dirname "$0")/.."
SDK_PATH="$(realpath "$SDK_PATH")"

cargo run --manifest-path "$STDB_PATH/crates/cli/Cargo.toml" -- generate -y -l csharp -o "$SDK_PATH/examples~/regression-tests/client/module_bindings" --project-path "$SDK_PATH/examples~/regression-tests/server"
cargo run --manifest-path "$STDB_PATH/crates/cli/Cargo.toml" -- publish -c -y -p "$SDK_PATH/examples~/regression-tests/server" btree-repro
cd "$SDK_PATH/examples~/regression-tests/client" && dotnet run -c Debug