# aero-vein task runner.
# Thin orchestration over the per-package scripts — `just` delegates into
# `bun run --filter`, `dotnet`, and `playwright` rather than duplicating logic.
# Run `just` with no arguments to list every recipe.

set shell := ["bash", "-uc"]

# List available recipes.
default:
    @just --list

# --- Dev -------------------------------------------------------------------

# Ensure the dev Postgres service is up and the aerovein_dev database exists.
# Assumes PostgreSQL is already installed — this starts the service, it does not install it.
db:
    @if ! pg_isready -q -h 127.0.0.1 -p 5432 2>/dev/null; then \
        echo "Starting PostgreSQL service..."; \
        sudo service postgresql start; \
    fi
    @sudo -u postgres psql -tc "SELECT 1 FROM pg_database WHERE datname='aerovein_dev'" 2>/dev/null \
        | grep -q 1 \
        || (echo "Creating aerovein_dev database..." && sudo -u postgres createdb aerovein_dev)

# Run the web dev server and the API watcher together (Ctrl-C stops both),
# each line prefixed with a colored [web]/[api] tag.
dev:
    export FORCE_COLOR=1; trap 'kill 0' EXIT; \
    bun run --filter web dev 2>&1 | sed -u $'s/^/\e[36m[web]\e[0m /' & \
    dotnet watch run --project packages/api 2>&1 | sed -u $'s/^/\e[32m[api]\e[0m /' & \
    wait

# Web dev server only (Vite on :3000).
dev-web:
    bun run --filter web dev

# API only (dotnet watch).
dev-api:
    dotnet watch run --project packages/api

# --- Build -----------------------------------------------------------------

# Build the web bundle.
build:
    bun run --filter web build

# Restore the .NET solution.
#
# `-m:1` is mandatory under the local sandbox. A multi-node build spawns
# out-of-proc MSBuild worker nodes, and each one opens a named-pipe socket the
# sandbox denies (`SocketException (13): Permission denied`), which fails the
# whole restore with a misleading MSB4276 fallback. A single in-process node
# sidesteps the socket entirely — the same reason `test` below passes `-m:1`.
# (Sibling sandbox note: Directory.Build.props, on SourceLink.)
restore:
    dotnet restore aero-vein.slnx -m:1

# Build the .NET solution (single-node — see `restore` for why `-m:1`).
build-api:
    dotnet build aero-vein.slnx -m:1

# --- Test ------------------------------------------------------------------

# Run a stack's tests (web|api|unit|int|e2e); no arg runs all.
test stack="all":
    case "{{stack}}" in \
        web)  bun run --filter web test --run ;; \
        api)  dotnet test aero-vein.slnx -m:1 ;; \
        unit) dotnet test packages/api.tests/api.tests.csproj -m:1 ;; \
        int)  dotnet test packages/api.integration.tests/api.integration.tests.csproj -m:1 ;; \
        e2e)  bun run test:e2e ;; \
        all)  just test web && just test api && just test e2e ;; \
        *)    echo "unknown stack: {{stack}} (web|api|unit|int|e2e|all)" >&2; exit 2 ;; \
    esac

# --- Quality ---------------------------------------------------------------

# Lint / format / typecheck across all packages.
check:
    bun run --filter '*' check

# Full PR gate: quality checks, then the whole test suite.
ci: check
    just test

# --- Codegen ---------------------------------------------------------------

# Regenerate the typed API client (API must be up on :5204 — see dev-api).
gen-api:
    bun run --filter web gen:api

# --- Housekeeping ----------------------------------------------------------

# Remove build artifacts across all packages.
cleanup:
    bun run --filter '*' cleanup
