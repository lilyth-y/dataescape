from __future__ import annotations

from functools import lru_cache

import torch
from pyannote.audio import Pipeline

from speaker_sep.config import Settings
from speaker_sep.device import resolve_device
from speaker_sep.scene import get_profile


class PipelineLoadError(RuntimeError):
    pass


@lru_cache(maxsize=4)
def _load_pipeline_cached(
    pipeline_id: str,
    hf_token: str | None,
    device_name: str,
    scene: str,
) -> Pipeline:
    if not hf_token:
        raise PipelineLoadError(
            "HF_TOKEN is required. Set it in .env or the environment. "
            "Accept model terms at https://huggingface.co/pyannote/speaker-diarization-community-1"
        )
    pipeline = Pipeline.from_pretrained(pipeline_id, token=hf_token)
    profile = get_profile(scene)  # type: ignore[arg-type]
    try:
        pipeline.instantiate(
            {
                "segmentation": {"min_duration_off": profile.segmentation_min_duration_off},
                "clustering": {"threshold": profile.clustering_threshold},
            },
        )
    except Exception:
        pass  # community-1 may ignore some keys; defaults still work
    pipeline.to(torch.device(device_name))
    return pipeline


def get_pipeline(settings: Settings) -> Pipeline:
    device = resolve_device(settings)
    return _load_pipeline_cached(
        settings.pipeline_id,
        settings.hf_token,
        str(device),
        settings.scene,
    )


def clear_pipeline_cache() -> None:
    _load_pipeline_cached.cache_clear()
