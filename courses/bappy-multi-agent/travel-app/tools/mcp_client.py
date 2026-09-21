import os
import certifi
from dotenv import load_dotenv
from langchain_mcp_adapters.client import MultiServerMCPClient, BaseTool

os.environ["SSL_CERT_FILE"] = certifi.where()
os.environ["REQUESTS_CA_BUNDLE"] = certifi.where()

load_dotenv()

TAVILY_API_KEY = os.getenv("TAVILY_API_KEY")
AVIATION_STACK_API_KEY = os.getenv("AVIATIONSTACK_API_KEY")
TAVILY_MCP_URL = f"https://mcp.tavily.com/mcp/?tavilyApiKey={TAVILY_API_KEY}"

AVIATION_ENV = os.environ.copy()
AVIATION_ENV["AVIATION_STACK_API_KEY"] = (
    AVIATION_STACK_API_KEY or ""
)

client = MultiServerMCPClient(
    {
        "tavily":{
            "transport": "streamable_http",
            "url": TAVILY_MCP_URL
        },
        "aviationstack": {
            "transport": "stdio",
            "command": "uvx",
            "args": [
                "--with", "mcp<2.0.0",
                "aviationstack-mcp"
            ],
            "env": AVIATION_ENV
        },
    }
)

async def get_mcp_tool(tool_name:str) -> BaseTool:
    tools = await client.get_tools()
    return next(
        tool
        for tool in tools
        if tool.name == tool_name
    )

async def get_all_tools():
    tools = await client.get_tools()
    print("\nAvailable MCP Tools:")

    for tool in tools:
        print(tool.name)


search_tool = None
aviation_tools = {}

async def initialize_mcp():
    global search_tool
    global aviation_tools

    if search_tool is not None and aviation_tools:
        return
    tools = await client.get_tools()

    for tool in tools:
        print(tool.name) 

    search_tool = next(
        tool
        for tool in tools
        if tool.name == "tavily_search"
    )

    aviation_tools = {
        tool.name: tool
        for tool in tools
        if tool.name != "tavily_search"
    }

async def tavily_mcp_search(query: str):
    await initialize_mcp()
    global search_tool
    if search_tool is None:
        search_tool = await get_mcp_tool("tavily_search")

    result = await search_tool.ainvoke(
        {
            "query": query
        }
    )
    return result

async def aviation_mcp_call(
        tool_name: str,
        tool_args: dict = None
):
    tools = await client.get_tools()
    tool = next(
        t for t in tools
        if t.name == tool_name
    )

    result = await tool.ainvoke(
        tool_args or {}
    )

    return result

