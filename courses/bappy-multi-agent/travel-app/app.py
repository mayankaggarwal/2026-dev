import uvicorn
from pathlib import Path
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse, HTMLResponse
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates
from pydantic import BaseModel
import traceback
from backend import run_travel_agent

BASE_DIR = Path(__file__).resolve().parent

app = FastAPI(
    title="TripMate AI",
    description="An AI-powered travel planning assistant that helps users plan their trips, find flights and hotels, and create personalized itineraries.",
    version="1.0.0",
)
app.mount(
    "/static", 
    StaticFiles(directory="static"), 
    name=str (BASE_DIR / "static"))

templates = Jinja2Templates(
    directory= str(BASE_DIR / "templates")
    )

class TravelRequest(BaseModel):
    message: str
    thread_id: str | None = None

@app.get("/health", response_class=JSONResponse)
async def health_check():
    return {
        "status": "ok",
        "message": "TripMate AI is running smoothly.",
        }

@app.get("/favicon.ico")
async def favicon():
    return JSONResponse(content={})

@app.post("/api/travel")
async def travel_planner(request_data: TravelRequest):
    try:
        user_input = request_data.message.strip()
        if not user_input:
            return JSONResponse(
                status_code=400,
                content={
                    "success": False,
                    "error": "The 'message' field cannot be empty."
                }
            )

        result = run_travel_agent(
            user_input=user_input,
            thread_id=request_data.thread_id
        )

        return JSONResponse(
            content={
                "success": True,
                "thread_id": result["thread_id"],
                "answer": result["answer"],
                "flight_results": result["flight_results"],
                "hotel_results": result["hotel_results"],
                "itinerary": result["itinerary"],
                "llm_calls": result["llm_calls"],
            }
        )
    except Exception as e:
        print("ERROR:", e)
        traceback.print_exc()
        return JSONResponse(
            status_code=500,
            content={
                "success": False,
                "error": str(e)
                }
        )

@app.get("/", response_class=HTMLResponse)
async def index(request: Request):
    return templates.TemplateResponse(
        request=request,
        name="index.html", 
        context={}
    )

if __name__ == "__main__":
    uvicorn.run(
        "app:app", 
        host="0.0.0.0", 
        port=8000,
        reload=True)