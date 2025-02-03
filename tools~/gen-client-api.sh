#!/bin/bash

set -ueo pipefail

STDB_PATH="$1"
SDK_PATH="$(dirname "$0")/.."

cargo run --manifest-path $STDB_PATH/crates/client-api-messages/Cargo.toml --example get_ws_schema |
cargo run --manifest-path $STDB_PATH/crates/cli/Cargo.toml -- generate -l csharp --namespace SpacetimeDB.ClientApi \
  --module-def \
  -o $SDK_PATH/src/SpacetimeDB/ClientApi

rm -rf $SDK_PATH/src/SpacetimeDB/ClientApi/_Globals
