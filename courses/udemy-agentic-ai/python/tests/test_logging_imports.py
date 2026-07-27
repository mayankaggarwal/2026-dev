import sys
from pathlib import Path


def test_main_entrypoint_imports_without_circular_error():
    repo_root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(repo_root))

    for module_name in ["main", "commands", "commands.lc_agent_service", "commands.run_service"]:
        sys.modules.pop(module_name, None)

    import main

    assert main.logger.name == "cli_app"
