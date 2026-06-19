import math
from datetime import datetime, timezone

from app.models.schemas import RiskScoreRequest, RiskScoreResponse


class RiskService:
    MODEL_VERSION = "rule-based-v1"

    def evaluate(self, request: RiskScoreRequest) -> RiskScoreResponse:
        factors: list[dict] = []
        base_score = 20.0

        # Factor 1: defect rate
        defects = request.defect_qty or 0
        sample = request.sample_size or 1
        defect_rate = (defects / sample) * 100
        if request.recent_defect_rate is not None:
            defect_rate = request.recent_defect_rate
        rate_score = min(defect_rate * 2, 40)
        base_score += rate_score
        factors.append({
            "name": "defect_rate",
            "value": defect_rate,
            "weight": rate_score,
        })

        # Factor 2: sample size penalty (small samples = higher uncertainty)
        if request.sample_size is not None and request.sample_size < 30:
            penalty = max(0, (30 - request.sample_size) * 0.5)
            base_score += penalty
            factors.append({
                "name": "small_sample_penalty",
                "value": request.sample_size,
                "weight": penalty,
            })

        # Factor 3: process risk base
        if request.equipment_id:
            base_score += 5
            factors.append({
                "name": "equipment_factor",
                "value": request.equipment_id,
                "weight": 5,
            })

        # Clamp and determine level
        final_score = min(max(base_score, 0), 100)
        risk_level = self._level(final_score)

        recommendations = self._recommendations(final_score, factors)

        return RiskScoreResponse(
            risk_score=round(final_score, 2),
            risk_level=risk_level,
            factors=factors,
            recommendations=recommendations,
            model_version=self.MODEL_VERSION,
            timestamp=datetime.now(timezone.utc),
        )

    def _level(self, score: float) -> str:
        if score < 30:
            return "low"
        if score < 60:
            return "medium"
        if score < 85:
            return "high"
        return "critical"

    def _recommendations(self, score: float, factors: list[dict]) -> list[str]:
        recs = []
        if score < 30:
            recs.append("Current risk level is acceptable. Continue routine monitoring.")
        elif score < 60:
            recs.append("Moderate risk detected. Consider increasing inspection frequency.")
            recs.append("Review process parameters for potential adjustments.")
        elif score < 85:
            recs.append("High risk. Immediate process review required.")
            recs.append("Stop production and perform root cause analysis.")
        else:
            recs.append("Critical risk. Escalate to quality manager immediately.")
            recs.append("Quarantine affected batches and initiate CAPA.")
        return recs
