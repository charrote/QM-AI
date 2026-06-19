from fastapi import APIRouter

router = APIRouter()


@router.get("/health", summary="Health check")
async def health_check():
    return {
        "status": "ok",
        "service": "qm-ai-service",
        "version": "1.0.0",
    }
