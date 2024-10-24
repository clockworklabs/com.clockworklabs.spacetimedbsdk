#!/bin/bash

set -euo pipefail

cd "$(dirname "$0")"

spacetime publish -s local untitled-circle-game
