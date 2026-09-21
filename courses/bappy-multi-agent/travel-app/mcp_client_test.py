import os
import asyncio
import certifi
from dotenv import load_dotenv
from langchain_mcp_adapters.client import MultiServerMCPClient

os.environ["SSL_CERT_FILE"] = certifi.where()
os.environ["REQUESTS_CA_BUNDLE"] = certifi.where()

load_dotenv()

TAVILY_API_KEY = os.getenv("TAVILY_API_KEY")
TAVILY_MCP_URL = f"https://mcp.tavily.com/mcp/?tavilyApiKey={TAVILY_API_KEY}"

client = MultiServerMCPClient(
    {
        "tavily":{
            "transport": "streamable_http",
            "url": TAVILY_MCP_URL
        }
    }
)

async def get_all_tools():
    tools = await client.get_tools()
    return tools

asyncio.run(get_all_tools())
