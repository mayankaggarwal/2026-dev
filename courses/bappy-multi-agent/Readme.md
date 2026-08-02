## Video URL
https://www.youtube.com/watch?v=BM39OouLNsM
https://github.com/entbappy/TripMate-AI-A-Multi-Agent-Travel-Planner-with-LangGraph
https://github.com/entbappy/TripMate-AI-Using-MCP
https://github.com/entbappy/Multi-Agent-System-using-LangGraph-MCP-Supervisor-Guardrails-HITL/tree/main

## Tasks

## How to run?

1. Create a Virtual environment
```
uv init travel-app
```

2. Activate the environment
   
```
cd travel-app
```

3. Pin python version

```
uv python pin 3.12
```

4. install packages
```
uv add langgraph langchain langchain-openai langchain-community langchain-tavily python-dotenv tavily-python requests langgraph-checkpoint-postgres airportsdata pycountry fastapi uvicorn jinja2 langchain-mcp-adapters nest-asyncio mcp certifi psycopg[binary] psycopg_pool
```

5. Create the Postgres DB on render.com and connect with local pgadmin

6. add the api keys in .env file

7. Create tavily_tool and flights_tool and test them

8. Add Agents using backend.py

