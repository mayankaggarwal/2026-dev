import os

import typer
from langchain_core.tools import tool
from langchain_openai import ChatOpenAI

from logging_config import logger

class LCAgentServiceCommands:
    def __init__(self, app: typer.Typer) -> None:
        self.app = app
        self._register_commands()

    @staticmethod
    @tool
    def calculate_word_count(text: str) -> int:
        """Calculates and returns the exact number of words inside a given text string."""
        return len(text.split())

    def _register_commands(self) -> None:
        @self.app.command("lc-agent-service")
        def lc_agent_service():
            """Run the LC agent service command."""
            current_env = os.environ.get("APP_ENV", "UNKNOWN")
            typer.secho(f"LC agent service running in {current_env}", fg=typer.colors.MAGENTA)

            if not os.environ.get("OPENAI_API_KEY"):
                typer.secho("Error: OPENAI_API_KEY is missing from your active .env file.", fg=typer.colors.RED, bold=True)
                raise typer.Exit(code=1)

            typer.secho("Initializing LangChain tool-calling model...", fg=typer.colors.CYAN)

            model = ChatOpenAI(model="gpt-4o-mini", temperature=0)
            tools = [self.calculate_word_count]
            model_with_tools = model.bind_tools(tools)

            try:
                user_prompt = typer.prompt("Enter text to analyze")
                response = model_with_tools.invoke(user_prompt)

                if response.tool_calls:
                    typer.secho("\n[Agent Decision: Using Local Tools]", fg=typer.colors.MAGENTA, bold=True)
                    for tool_call in response.tool_calls:
                        if tool_call["name"] == "calculate_word_count":
                            args = tool_call["args"]
                            result = self.calculate_word_count.invoke(args)
                            typer.echo(f"Tool executed: calculate_word_count('{args.get('text')}') -> Result: {result}")

                            final_response = model.invoke([
                                {"role": "user", "content": user_prompt},
                                response,
                                {"role": "tool", "content": str(result), "tool_call_id": tool_call["id"]},
                            ])
                            typer.secho(f"\nFinal AI Response:\n{final_response.content}", fg=typer.colors.GREEN)
                else:
                    logger.info(f"Final Agent Text Response (No tools used): {response.content.strip()}")

                    typer.secho(f"\nFinal AI Response:\n{response.content}", fg=typer.colors.GREEN)

            except Exception as e:
                logger.error(f"API Error encountered: {str(e)}")
                typer.secho(f"API Error encountered: {str(e)}", fg=typer.colors.RED)
                raise typer.Exit(code=1)

    def _get_context(self) -> dict[str, str]:
        return {
            "APP_ENV": os.environ.get("APP_ENV", "UNKNOWN"),
            "OPENAI_API_KEY": os.getenv("OPENAI_API_KEY", "NOT_SET"),
        }


def register_lc_agent_service_command(app: typer.Typer) -> None:
    LCAgentServiceCommands(app)
