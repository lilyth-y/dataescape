from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

import torch
from pyannote.core import Annotation

from speaker_sep.config import Settings
from speaker_sep.separation import extract_speaker_audio
from speaker_sep.timeline import DiarizationTimeline


@dataclass
class DiarizationResult:
    annotation: Annotation
    timeline: DiarizationTimeline
    speaker_wavs: dict[str, Path]
    output_dir: Path


def export_results(
    waveform: torch.Tensor,
    annotation: Annotation,
    timeline: DiarizationTimeline,
    settings: Settings,
    output_dir: Path,
    *,
    write_timeline: bool = True,
    write_audio: bool = True,
) -> DiarizationResult:
    output_dir.mkdir(parents=True, exist_ok=True)
    speaker_paths: dict[str, Path] = {}

    if write_timeline:
        timeline.save_json(output_dir / "timeline.json")

    if write_audio:
        speaker_paths = extract_speaker_audio(
            waveform,
            annotation,
            settings,
            output_dir,
        )

    return DiarizationResult(
        annotation=annotation,
        timeline=timeline,
        speaker_wavs=speaker_paths,
        output_dir=output_dir,
    )
