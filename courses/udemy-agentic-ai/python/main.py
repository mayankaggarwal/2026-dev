import os
from pathlib import Path

import typer
from dotenv import load_dotenv

from logging_config import logger

app = typer.Typer(help="CLI app reading configurations from environment files.", no_args_is_help=True)


def register_commands() -> None:
    from commands import register_lc_agent_service_command, register_run_service_command, register_lcbb1_subcommands

    register_run_service_command(app)
    register_lc_agent_service_command(app)
    register_lcbb1_subcommands(app)


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

bootstrap_environment()
register_commands()


if __name__ == "__main__":
    app()
