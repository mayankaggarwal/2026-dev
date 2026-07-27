# UV Python CLI Application Setup Guide (Windows)

This folder already contains a working minimal UV-based Python CLI project. The fastest path is to reuse the files here instead of recreating them from scratch.

---

## ⚡ Fastest Setup Path

From the project folder:

```powershell
cd .\python
uv sync
```

This creates the local virtual environment and installs the required packages.

---

## 🧑‍💻 Interactive Setup Questions

When you start from scratch, ask for these values first:

```powershell
$project_name = Read-Host "Project name (default: python-project-setup)"
if ([string]::IsNullOrWhiteSpace($project_name)) { $project_name = "python-project-setup" }

$default_service_name = Read-Host "Default service name (default: run-service)"
if ([string]::IsNullOrWhiteSpace($default_service_name)) { $default_service_name = "run-service" }
```

Use these values to replace the placeholders below:
- `<project_name>` for the package name in `pyproject.toml`
- `<default_service_name>` for the first command you want to expose

---

## ⚙️ VS Code Interpreter

To make IntelliSense and Pylance work correctly:

1. Open the Command Palette with Ctrl+Shift+P.
2. Run Python: Select Interpreter.
3. Choose the local environment at `./.venv/Scripts/python.exe`.

---

## 📁 Project Files

The current project already includes these files:

### `pyproject.toml`
```toml
[project]
name = "python-project-setup"
version = "0.1.0"
description = "Simple UV-based CLI app for environment-driven configuration"
readme = "README.md"
requires-python = ">=3.11"
dependencies = [
    "typer>=0.12.0",
    "python-dotenv>=1.0.0",
]

[build-system]
requires = ["setuptools>=61"]
build-backend = "setuptools.build_meta"

[project.scripts]
myapp = "main:app"

[tool.setuptools]
py-modules = ["main"]
```

### `main.py`
```python
import os
from pathlib import Path

import typer
from dotenv import load_dotenv

from commands import register_lc_agent_service_command, register_run_service_command

app = typer.Typer(help="CLI app reading configurations from environment files.", no_args_is_help=True)


def bootstrap_environment():
    """Determine the target environment and load the matching .env profile."""
    target_env = os.environ.get("APP_ENV", "development").lower()
    env_file_name = f".env.{target_env}"
    env_path = Path(__file__).resolve().parent / env_file_name

    if not env_path.exists():
        typer.secho(f"Warning: '{env_file_name}' not found. Using generic fallback.", fg=typer.colors.YELLOW)
        load_dotenv()
    else:
        load_dotenv(dotenv_path=env_path)


@app.callback(invoke_without_command=True)
def main():
    """CLI app reading configurations from environment files."""
    bootstrap_environment()


register_run_service_command(app)
register_lc_agent_service_command(app)


if __name__ == "__main__":
    app()
```

### `commands/run_service.py`
```python
import os

import typer


class RunServiceCommands:
    def __init__(self, app: typer.Typer) -> None:
        self.app = app
        self._register_commands()

    def _register_commands(self) -> None:
        @self.app.command("run-service")
        def run_service():
            """Execute background processes using loaded environmental contexts."""
            current_env = os.environ.get("APP_ENV", "UNKNOWN")
            host = os.environ.get("SERVER_HOST", "localhost")
            port = os.environ.get("SERVER_PORT", "N/A")
            secret = os.environ.get("DB_KEY", "NOT_SET")
            openai_api_key = os.getenv("OPENAI_API_KEY", "NOT_SET")

            theme = typer.colors.GREEN if current_env == "PRODUCTION" else typer.colors.CYAN

            typer.secho(f"--- RUNNING CONTEXT: {current_env} ---", fg=theme, bold=True)
            typer.echo(f"Service Endpoint : http://{host}:{port}")
            typer.echo(f"Secret Encryption Key: {secret}")
            typer.echo(f"OpenAI API Key: {openai_api_key}")


def register_run_service_command(app: typer.Typer) -> None:
    RunServiceCommands(app)
```

### `commands/lc_agent_service.py`
```python
import os

import typer


class LCAgentServiceCommands:
    def __init__(self, app: typer.Typer) -> None:
        self.app = app
        self._register_commands()

    def _register_commands(self) -> None:
        @self.app.command("lc-agent-service")
        def lc_agent_service():
            """Run the LC agent service command."""
            current_env = os.environ.get("APP_ENV", "UNKNOWN")
            typer.secho(f"LC agent service running in {current_env}", fg=typer.colors.MAGENTA)


def register_lc_agent_service_command(app: typer.Typer) -> None:
    LCAgentServiceCommands(app)
```

### `.env.development`
```env
APP_ENV=DEVELOPMENT
SERVER_HOST=127.0.0.1
SERVER_PORT=8080
DB_KEY=dev_secret_pass_123
```

### `.env.production`
```env
APP_ENV=PRODUCTION
SERVER_HOST=192.168.1.50
SERVER_PORT=443
DB_KEY=prod_secure_crypto_999
```

---

## ▶️ Run the Project

### Default development environment
```powershell
cd .\python
uv run python main.py run-service
```

### Switch to production environment
```powershell
cd .\python
$env:APP_ENV="production"
uv run python main.py run-service
```

### Check the CLI help
```powershell
cd .\python
uv run python main.py --help
```

### Run the additional LC agent command
```powershell
cd .\python
uv run python main.py lc-agent-service
```

---

## 🧠 Why this setup is faster

- No need to recreate the project structure every time.
- `uv sync` handles dependency installation in one step.
- The environment files already provide development and production values.
- The CLI command is already wired to `run-service`.
- A second command module is also available for `lc-agent-service`.
- Environment loading is centralized in the main app file so new command modules stay simple.
