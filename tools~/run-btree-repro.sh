#!/usr/bin/env bash

set -ueo pipefail

STDB_PATH="$1"
SDK_PATH="$(dirname "$0")/.."
SDK_PATH="$(realpath "$SDK_PATH")"

"$STDB_PATH/target/debug/spacetimedb-cli" generate -y -l csharp -o "$SDK_PATH/examples~/btree-repro/client/module_bindings" --project-path "$SDK_PATH/examples~/btree-repro/server"
"$STDB_PATH/target/debug/spacetimedb-cli" start 1>/dev/null 2>/dev/null &
"$STDB_PATH/target/debug/spacetimedb-cli" publish -c -y -p "$SDK_PATH/examples~/btree-repro/server" btree-repro
cd "$SDK_PATH/examples~/btree-repro/client" && dotnet run -c Debug
trap "trap - SIGTERM && kill -- -$$" SIGINT SIGTERM EXIT