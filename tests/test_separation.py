import numpy as np
import torch
from pyannote.core import Annotation, Segment

from speaker_sep.config import Settings
from speaker_sep.separation import build_frame_masks, masks_to_waveforms


def test_build_frame_masks():
    ann = Annotation()
    ann[Segment(0.0, 0.5)] = "SPEAKER_00"
    ann[Segment(0.5, 1.0)] = "SPEAKER_01"

    masks, hop = build_frame_masks(ann, num_samples=16000, sample_rate=16000, frame_ms=10.0)
    assert "SPEAKER_00" in masks
    assert masks["SPEAKER_00"].sum() > 0


def test_masks_to_waveforms_shape():
    audio = np.random.randn(16000).astype(np.float32)
    masks = {"A": np.ones(100, dtype=np.float32)}
    tracks = masks_to_waveforms(audio, masks, hop=160)
    assert tracks["A"].shape == audio.shape
