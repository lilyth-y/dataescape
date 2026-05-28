from __future__ import annotations

from dataclasses import dataclass
from typing import Literal

Scene = Literal["default", "crowd", "meeting"]


@dataclass(frozen=True)
class SceneProfile:
    """Tuned parameters for an acoustic scene."""

    name: Scene
    description: str
    stream_window_sec: float
    stream_step_sec: float
    stream_min_buffer_sec: float
    min_segment_sec: float
    max_dominant_speakers: int | None
    highpass_hz: float
    target_rms_db: float
    noise_gate_db: float | None
    channel_strategy: Literal["mono_avg", "best_speech_band"]
    clustering_threshold: float
    segmentation_min_duration_off: float
    merge_similarity_threshold: float
    default_max_speakers: int | None
    separation_energy_split: Literal["equal", "sqrt"]


PROFILES: dict[Scene, SceneProfile] = {
    "default": SceneProfile(
        name="default",
        description="General-purpose diarization",
        stream_window_sec=10.0,
        stream_step_sec=0.5,
        stream_min_buffer_sec=3.0,
        min_segment_sec=0.0,
        max_dominant_speakers=None,
        highpass_hz=0.0,
        target_rms_db=-20.0,
        noise_gate_db=None,
        channel_strategy="mono_avg",
        clustering_threshold=0.6,
        segmentation_min_duration_off=0.0,
        merge_similarity_threshold=0.65,
        default_max_speakers=None,
        separation_energy_split="sqrt",
    ),
    "crowd": SceneProfile(
        name="crowd",
        description=(
            "Public venue / dense crowd: emphasize how audio enters the model — "
            "pick the best mic channel, denoise, then track dominant voices over long windows"
        ),
        stream_window_sec=20.0,
        stream_step_sec=1.0,
        stream_min_buffer_sec=5.0,
        min_segment_sec=0.35,
        max_dominant_speakers=8,
        highpass_hz=80.0,
        target_rms_db=-18.0,
        noise_gate_db=-38.0,
        channel_strategy="best_speech_band",
        clustering_threshold=0.72,
        segmentation_min_duration_off=0.25,
        merge_similarity_threshold=0.70,
        default_max_speakers=20,
        separation_energy_split="sqrt",
    ),
    "meeting": SceneProfile(
        name="meeting",
        description="Indoor conversation with 2–10 speakers",
        stream_window_sec=8.0,
        stream_step_sec=0.5,
        stream_min_buffer_sec=2.0,
        min_segment_sec=0.15,
        max_dominant_speakers=None,
        highpass_hz=60.0,
        target_rms_db=-20.0,
        noise_gate_db=None,
        channel_strategy="mono_avg",
        clustering_threshold=0.58,
        segmentation_min_duration_off=0.1,
        merge_similarity_threshold=0.62,
        default_max_speakers=12,
        separation_energy_split="sqrt",
    ),
}


def get_profile(scene: Scene) -> SceneProfile:
    return PROFILES[scene]
