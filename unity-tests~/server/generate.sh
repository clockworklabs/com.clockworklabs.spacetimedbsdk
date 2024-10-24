#!/bin/bash

set -euo pipefail

cd "$(dirname "$0")"

spacetime generate --out-dir ../client/Assets/Scripts/autogen --lang cs $@
