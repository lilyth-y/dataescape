from __future__ import annotations

from pathlib import Path
from typing import Literal

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Runtime configuration (env vars or defaults)."""

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    hf_token: str | None = Field(default=None, validation_alias="HF_TOKEN")
    pipeline_id: str = Field(
        default="pyannote/speaker-diarization-community-1",
        validation_alias="SPEAKER_SEP_PIPELINE",
    )
    device: Literal["auto", "cuda", "cpu"] = Field(
        default="auto",
        validation_alias="SPEAKER_SEP_DEVICE",
    )
    sample_rate: int = 16_000

    # Streaming
    stream_window_sec: float = Field(default=10.0, validation_alias="SPEAKER_SEP_STREAM_WINDOW_SEC")
    stream_step_sec: float = Field(default=0.5, validation_alias="SPEAKER_SEP_STREAM_STEP_SEC")
    stream_min_buffer_sec: float = 3.0

    # Diarization hints (optional; unset = auto speaker count)
    min_speakers: int | None = None
    max_speakers: int | None = None

    # Separation
    separation_frame_ms: float = 10.0
    overlap_energy_split: Literal["equal", "sqrt"] = "sqrt"

    # Output
    output_dir: Path = Path("outputs")


def get_settings(**overrides: object) -> Settings:
    return Settings(**overrides)  # type: ignore[arg-type]
