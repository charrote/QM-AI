from datetime import datetime
from typing import Any

from pydantic import BaseModel, Field


class RiskScoreRequest(BaseModel):
    product_id: int
    process_id: int | None = None
    equipment_id: int | None = None
    recent_defect_rate: float | None = Field(None, ge=0, le=100)
    sample_size: int | None = Field(None, gt=0)
    defect_qty: int | None = Field(None, ge=0)
    param_values: dict[str, float] | None = None


class RiskScoreResponse(BaseModel):
    risk_score: float = Field(..., ge=0, le=100)
    risk_level: str
    factors: list[dict[str, Any]]
    recommendations: list[str]
    model_version: str
    timestamp: datetime


class AnalysisRequest(BaseModel):
    source_module: str
    source_ref_id: int
    params: dict[str, Any] | None = None


class AnalysisResponse(BaseModel):
    analysis_type: str
    result: dict[str, Any]
    confidence: float = Field(..., ge=0, le=1)
    model_version: str
    timestamp: datetime
