import os

import typer

from logging_config import logger


class LCBuildingBlock1Command:
    def __init__(self, app: typer.Typer):
        self.app = app
        self.register_commands()

    def register_commands(self) -> None:
        @self.app.command(name="lc_bb1", help="Run the LC Building Block 1 command.")
        def lc_bb1():
            current_env = os.environ.get("APP_ENV", "UNKNOWN")
            typer.secho(f"LC Building Block running in {current_env}", fg=typer.colors.MAGENTA)


def register_lcbb1_subcommands(app: typer.Typer) -> None:
    LCBuildingBlock1Command(app)