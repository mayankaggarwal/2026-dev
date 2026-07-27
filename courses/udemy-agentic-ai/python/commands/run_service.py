import os

import typer

from logging_config import logger


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
            logger.info("run-service executed with environment %s", current_env)

    def _get_context(self) -> dict[str, str]:
        return {
            "APP_ENV": os.environ.get("APP_ENV", "UNKNOWN"),
            "SERVER_HOST": os.environ.get("SERVER_HOST", "localhost"),
            "SERVER_PORT": os.environ.get("SERVER_PORT", "N/A"),
            "DB_KEY": os.environ.get("DB_KEY", "NOT_SET"),
            "OPENAI_API_KEY": os.getenv("OPENAI_API_KEY", "NOT_SET"),
        }


def register_run_service_command(app: typer.Typer) -> None:
    RunServiceCommands(app)

