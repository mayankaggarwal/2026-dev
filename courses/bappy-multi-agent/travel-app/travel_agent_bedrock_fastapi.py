
"""
travel_agent_bedrock.py

Production-oriented skeleton for LangChain 1.x + create_agent.

Features
--------
- FastAPI
- Async implementation
- AWS Bedrock (ChatBedrockConverse placeholder)
- PostgreSQL checkpointer
- Thread memory
- Tavily tool
- Flight tool
- Pydantic structured output
- Streaming endpoint
- Logging
- Retry wrappers

NOTE:
This is a reference architecture. Replace placeholder implementations
(search_flights, AWS credentials, database URL, etc.) with your project code.
"""

from __future__ import annotations

import asyncio
import logging
import uuid
from functools import wraps
from typing import AsyncIterator

import psycopg
from fastapi import FastAPI
from fastapi.responses import StreamingResponse
from langchain.agents import create_agent
from langchain.tools import tool
from langchain_aws import ChatBedrockConverse
from langchain_core.messages import HumanMessage
from langgraph.checkpoint.postgres import PostgresSaver
from pydantic import BaseModel, Field
from psycopg.rows import dict_row

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("travel-agent")


class TravelResponse(BaseModel):
    trip_summary: str
    flights: str
    hotels: str
    itinerary: str
    estimated_budget: str
    recommendations: list[str] = Field(default_factory=list)


def retry(times=3):
    def deco(fn):
        @wraps(fn)
        async def wrapper(*args, **kwargs):
            last = None
            for _ in range(times):
                try:
                    return await fn(*args, **kwargs)
                except Exception as ex:
                    last = ex
                    logger.exception("Retrying...")
                    await asyncio.sleep(1)
            raise last
        return wrapper
    return deco


def get_database_url() -> str:
    return "postgresql://user:password@localhost:5432/travel"


async def search_flights(query: str) -> str:
    return f"Flight results for: {query}"


async def tavily_search(query: str) -> str:
    return f"Hotel search via Tavily: {query}"


llm = ChatBedrockConverse(
    model="anthropic.claude-sonnet-4",
    region_name="us-east-1",
)

@tool
async def flight_search(query: str) -> str:
    """Search flights."""
    return await search_flights(query)


@tool
async def hotel_search(query: str) -> str:
    """Search hotels."""
    return await tavily_search(query)


conn = psycopg.connect(
    get_database_url(),
    autocommit=True,
    row_factory=dict_row,
)
checkpointer = PostgresSaver(conn)
checkpointer.setup()

agent = create_agent(
    model=llm,
    tools=[flight_search, hotel_search],
    checkpointer=checkpointer,
    response_format=TravelResponse,
    system_prompt="""
You are an enterprise travel planner.

Workflow:
1. Search flights.
2. Search hotels.
3. Produce itinerary.
4. Return TravelResponse.
""",
)

app = FastAPI(title="Travel Agent")


class TravelRequest(BaseModel):
    query: str
    thread_id: str | None = None


@retry()
async def invoke_agent(req: TravelRequest):
    thread = req.thread_id or f"user_{uuid.uuid4().hex}"
    config = {"configurable": {"thread_id": thread}}

    result = await agent.ainvoke(
        {
            "messages": [
                HumanMessage(content=req.query)
            ]
        },
        config=config,
    )

    structured = result.get("structured_response")

    return {
        "thread_id": thread,
        "response": structured.model_dump() if structured else None,
        "messages": [m.content for m in result["messages"]],
    }


@app.post("/travel")
async def travel(req: TravelRequest):
    return await invoke_agent(req)


async def event_stream(req: TravelRequest) -> AsyncIterator[str]:
    thread = req.thread_id or f"user_{uuid.uuid4().hex}"
    config = {"configurable": {"thread_id": thread}}

    async for event in agent.astream(
        {
            "messages": [
                HumanMessage(content=req.query)
            ]
        },
        config=config,
    ):
        yield f"data: {event}\n\n"


@app.post("/travel/stream")
async def stream(req: TravelRequest):
    return StreamingResponse(
        event_stream(req),
        media_type="text/event-stream",
    )


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
