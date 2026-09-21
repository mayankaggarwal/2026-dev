import asyncio

from tools.tavily_tool import tavily_search
from tools.flight_tool import search_flights
from backend_withguard import resume_travel_agent, run_travel_agent
from tools.mcp_client import tavily_mcp_search, get_all_tools

#res = tavily_search("Best places to visit in Europe")
#print(res)

# res = search_flights("Plan a 7 days Nepal trip from Bangladesh")
# print(res)

#user_input = input("Enter travel request: ")

# response = run_travel_agent(
#     user_input=user_input,
#     thread_id="test_user"
# )

# print(f"\nFINAL RESPONSE: {response['thread_id']}\n")
# print(response["answer"])
# print(response)

resume_response = resume_travel_agent(
    thread_id="test_user",
    approved=True,
    feedback="Looks good!"
)

print(f"\nRESUME RESPONSE: {resume_response['thread_id']}\n")
print(resume_response)

#print(asyncio.run(tavily_mcp_search("Delhi")))
# if __name__ == "__main__":
#     asyncio.run(get_all_tools())