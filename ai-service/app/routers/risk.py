from fastapi import APIRouter, HTTPException

from app.models.schemas import RiskScoreRequest, RiskScoreResponse
from app.services.risk_service import RiskService

router = APIRouter()
risk_service = RiskService()


@router.post("/score", response_model=RiskScoreResponse, summary="Calculate risk score")
async def calculate_risk_score(request: RiskScoreRequest):
    try:
        result = risk_service.evaluate(request)
        return result
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
