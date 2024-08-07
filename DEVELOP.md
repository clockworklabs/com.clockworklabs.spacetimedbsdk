# Notes for maintainers

## `SpacetimeDB.ClientApi`

```sh
CL_HOME=~/clockworklabs
cd $CL_HOME/SpacetimeDB/crates/client-api-messages
cargo run --example get_ws_schema > $CL_HOME/schema.json
cd $CL_HOME/SpacetimeDB/crates/cli
cargo run -- generate -l csharp --json-module $CL_HOME/schema.json \
  -o $CL_HOME/spacetimedb-csharp-sdk/src/SpacetimeDB/ClientApi
rm -f $CL_HOME/schema.json
cd $CL_HOME/spacetimedb-csharp-sdk/src/SpacetimeDB/ClientApi
rm -rf _Globals
find . -name "*.cs" | xargs sed s/SpacetimeDB.Types/SpacetimeDB.ClientApi/
```

On Windows:
```bat
set CL_HOME=%USERPROFILE%\clockworklabs
cd %CL_HOME%\SpacetimeDB\crates\client-api-messages
cargo run --example get_ws_schema > %CL_HOME%/schema.json
cd %CL_HOME%\SpacetimeDB\crates\cli
cargo run -- generate -l csharp --json-module %CL_HOME%\schema.json ^
  -o %CL_HOME%\spacetimedb-csharp-sdk\src\SpacetimeDB\ClientApi
del %CL_HOME%\schema.json
cd %CL_HOME%\spacetimedb-csharp-sdk\src\SpacetimeDB\ClientApi
del /q _Globals
rg --files | rg "\.cs$" | xargs sed -i s/SpacetimeDB.Types/SpacetimeDB.ClientApi/
```
