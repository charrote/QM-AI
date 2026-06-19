from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.routers import health, risk, analysis


@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: load AI models, connect to external services
    app.state.models = {}
    yield
    # Shutdown: release resources
    app.state.models.clear()


app = FastAPI(
    title="QM-AI Industrial AI Quality Decision Platform",
    description="AI-powered quality analysis, risk scoring, and prediction service",
    version="1.0.0",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(health.router, tags=["Health"])
app.include_router(risk.router, prefix="/api/v1/risk", tags=["Risk"])
app.include_router(analysis.router, prefix="/api/v1/analysis", tags=["Analysis"])
