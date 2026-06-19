from fastapi import APIRouter, HTTPException

from app.models.schemas import AnalysisRequest, AnalysisResponse

router = APIRouter()


@router.post("/detect", summary="Anomaly detection")
async def detect_anomaly(request: AnalysisRequest):
    raise HTTPException(status_code=501, detail="Not implemented")


@router.post("/predict", summary="Quality prediction")
async def predict_quality(request: AnalysisRequest):
    raise HTTPException(status_code=501, detail="Not implemented")


@router.post("/root-cause", summary="Root cause analysis")
async def root_cause_analysis(request: AnalysisRequest):
    raise HTTPException(status_code=501, detail="Not implemented")
