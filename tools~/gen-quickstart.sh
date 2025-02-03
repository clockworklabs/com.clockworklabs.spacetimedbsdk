#!/bin/bash

set -ueo pipefail

STDB_PATH="$1"
SDK_PATH="$(dirname "$0")/.."

rm -rf "$SDK_PATH/examples~/quickstart/client/module_bindings/*"
cargo run --manifest-path "$STDB_PATH/crates/cli/Cargo.toml" -- generate -l csharp -o "$SDK_PATH/examples~/quickstart/client/module_bindings" --project-path "$STDB_PATH/modules/quickstart-chat"
