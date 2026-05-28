import numpy as np
import torch

from speaker_sep.annotation_filter import filter_short_segments, keep_dominant_speakers
from speaker_sep.ingest import apply_scene_to_settings
from speaker_sep.preprocess import select_best_channel, speech_band_energy
from speaker_sep.scene import get_profile
from pyannote.core import Annotation, Segment


def test_crowd_scene_profile():
    p = get_profile("crowd")
    assert p.channel_strategy == "best_speech_band"
    assert p.stream_window_sec >= 15.0
    assert p.max_dominant_speakers == 8


def test_apply_scene_sets_max_speakers():
    from speaker_sep.config import Settings

    s = Settings(hf_token="x")
    s2 = apply_scene_to_settings(s, "crowd")
    assert s2.max_speakers == 20
    assert s2.scene == "crowd"


def test_select_best_channel_picks_louder_speech_band():
    sr = 16000
    t = torch.linspace(0, 1, sr)
    quiet = 0.01 * torch.sin(2 * np.pi * 200 * t)
    loud = 0.5 * torch.sin(2 * np.pi * 1000 * t)
    stereo = torch.stack([quiet, loud])
    best = select_best_channel(stereo, sr)
    assert speech_band_energy(best, sr) >= speech_band_energy(stereo[0:1], sr)


def test_keep_dominant_speakers():
    ann = Annotation()
    ann[Segment(0, 10)] = "A"
    ann[Segment(0, 0.5)] = "B"
    out = keep_dominant_speakers(ann, max_speakers=1)
    labels = {str(l) for _, _, l in out.itertracks(yield_label=True)}
    assert labels == {"A"}


def test_filter_short_segments():
    ann = Annotation()
    ann[Segment(0, 0.1)] = "X"
    ann[Segment(1, 3)] = "Y"
    out = filter_short_segments(ann, min_duration_sec=0.35)
    labels = {str(l) for _, _, l in out.itertracks(yield_label=True)}
    assert "X" not in labels
    assert "Y" in labels
