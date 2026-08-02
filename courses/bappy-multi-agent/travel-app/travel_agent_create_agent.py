"""
travel_agent_create_agent.py

Example migration from a LangGraph pipeline to LangChain create_agent().

NOTE:
- This demonstrates the architectural migration.
- Replace the placeholder imports/model initialization with the versions
  used in your project.
"""

from __future__ import annotations

import uuid
from typing import Any

import psycopg
from psycopg.rows import dict_row

from langchain.tools import tool
from langchain.agents import create_agent
from langchain_core.messages import HumanMessage
from langgraph.checkpoint.postgres import PostgresSaver

# ---------------------------------------------------------------------
# Replace these with your own implementations
# ---------------------------------------------------------------------

def get_database_url() -> str:
    raise NotImplementedError

def search_flights(query: str) -> str:
    raise NotImplementedError

def tavily_search(query: str) -> str:
    raise NotImplementedError

# Example:
# from langchain_openai import ChatOpenAI
# llm = ChatOpenAI(model="gpt-4.1")
llm = None


# ---------------------------------------------------------------------
# Tools
# ---------------------------------------------------------------------

@tool
def search_flights_tool(query: str) -> str:
    """Search for flights."""
    return search_flights(query)


@tool
def search_hotels_tool(query: str) -> str:
    """Search for hotels."""
    return tavily_search(f"Best hotels for {query}")


@tool
def create_itinerary_tool(
    user_query: str,
    flights: str,
    hotels: str,
) -> str:
    """Create an itinerary from flight and hotel data."""

    prompt = f"""
Create a complete travel itinerary.

User:
{user_query}

Flights:
{flights}

Hotels:
{hotels}

Make it practical and budget aware.
"""

    response = llm.invoke(prompt)
    return response.content


# ---------------------------------------------------------------------
# Checkpointer
# ---------------------------------------------------------------------

DATABASE_URL = get_database_url()

_conn = psycopg.connect(
    DATABASE_URL,
    autocommit=True,
    row_factory=dict_row,
)

checkpointer = PostgresSaver(_conn)
checkpointer.setup()

# ---------------------------------------------------------------------
# Agent
# ---------------------------------------------------------------------

travel_agent = create_agent(
    model=llm,
    tools=[
        search_flights_tool,
        search_hotels_tool,
        create_itinerary_tool,
    ],
    checkpointer=checkpointer,
    system_prompt="""
You are an expert travel assistant.

For travel planning requests:

1. Search flights.
2. Search hotels.
3. Create itinerary.
4. Produce a polished response.

Use tools whenever required.
""",
)

# ---------------------------------------------------------------------
# FastAPI helper
# ---------------------------------------------------------------------

def run_travel_agent(
    user_input: str,
    thread_id: str | None = None,
) -> dict[str, Any]:

    if not thread_id:
        thread_id = f"user_{uuid.uuid4().hex}"

    config = {
        "configurable": {
            "thread_id": thread_id
        }
    }

    result = travel_agent.invoke(
        {
            "messages": [
                HumanMessage(content=user_input)
            ]
        },
        config=config,
    )

    return {
        "thread_id": thread_id,
        "answer": result["messages"][-1].content,
        "messages": result["messages"],
    }


if __name__ == "__main__":
    response = run_travel_agent(
        "Plan a five day trip to Tokyo from Delhi."
    )

    print(response["answer"])
