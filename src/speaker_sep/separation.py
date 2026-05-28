from __future__ import annotations

from pathlib import Path

import numpy as np
import torch
from pyannote.core import Annotation, Segment

from speaker_sep.audio_io import save_wav, tensor_to_numpy
from speaker_sep.config import Settings


def build_frame_masks(
    annotation: Annotation,
    num_samples: int,
    sample_rate: int,
    frame_ms: float,
) -> dict[str, np.ndarray]:
    """Per-speaker float masks in [0, 1] at frame resolution."""
    hop = max(1, int(sample_rate * frame_ms / 1000.0))
    n_frames = (num_samples + hop - 1) // hop
    speakers = sorted({str(l) for _, _, l in annotation.itertracks(yield_label=True)})
    masks = {spk: np.zeros(n_frames, dtype=np.float32) for spk in speakers}

    for segment, _, label in annotation.itertracks(yield_label=True):
        spk = str(label)
        start_f = int(segment.start * sample_rate / hop)
        end_f = int(segment.end * sample_rate / hop)
        start_f = max(0, min(start_f, n_frames))
        end_f = max(start_f, min(end_f, n_frames))
        masks[spk][start_f:end_f] = 1.0

    return masks, hop


def _resolve_overlap(
    masks: dict[str, np.ndarray],
    mode: str,
) -> dict[str, np.ndarray]:
    """Normalize masks where multiple speakers are active."""
    if not masks:
        return masks
    stack = np.stack(list(masks.values()), axis=0)
    active_count = np.maximum(stack.sum(axis=0), 1e-8)
    if mode == "equal":
        scale = np.where(stack > 0, 1.0 / active_count, 0.0)
    else:  # sqrt — reduces pumping on dense overlap (concerts, panels)
        scale = np.where(stack > 0, 1.0 / np.sqrt(active_count), 0.0)
    out: dict[str, np.ndarray] = {}
    for i, spk in enumerate(masks):
        out[spk] = (stack[i] * scale[i]).astype(np.float32)
    return out


def masks_to_waveforms(
    audio: np.ndarray,
    masks: dict[str, np.ndarray],
    hop: int,
) -> dict[str, np.ndarray]:
    """Upsample frame masks and apply to waveform."""
    tracks: dict[str, np.ndarray] = {}
    for spk, mask in masks.items():
        expanded = np.repeat(mask, hop)[: len(audio)]
        if len(expanded) < len(audio):
            expanded = np.pad(expanded, (0, len(audio) - len(expanded)))
        tracks[spk] = (audio * expanded).astype(np.float32)
    return tracks


def extract_speaker_audio(
    waveform: torch.Tensor,
    annotation: Annotation,
    settings: Settings,
    output_dir: Path,
    prefix: str = "",
) -> dict[str, Path]:
    """Write one WAV per speaker with overlap-aware masking."""
    audio = tensor_to_numpy(waveform)
    num_samples = len(audio)
    masks, hop = build_frame_masks(
        annotation,
        num_samples,
        settings.sample_rate,
        settings.separation_frame_ms,
    )
    masks = _resolve_overlap(masks, settings.overlap_energy_split)
    tracks = masks_to_waveforms(audio, masks, hop)

    paths: dict[str, Path] = {}
    for spk, data in tracks.items():
        fname = f"{prefix}{spk}.wav" if prefix else f"{spk}.wav"
        path = output_dir / "speakers" / fname
        save_wav(path, data, settings.sample_rate)
        paths[spk] = path
    return paths


def crop_segment_audio(
    waveform: torch.Tensor,
    segment: Segment,
    sample_rate: int,
) -> np.ndarray:
    audio = tensor_to_numpy(waveform)
    start = int(segment.start * sample_rate)
    end = int(segment.end * sample_rate)
    return audio[start:end]
