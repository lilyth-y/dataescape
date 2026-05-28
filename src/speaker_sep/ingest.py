from __future__ import annotations

import json
from dataclasses import asdict, dataclass, field
from pathlib import Path
from typing import Any

import torch
import torchaudio

from speaker_sep.config import Settings
from speaker_sep.preprocess import downmix, preprocess_waveform
from speaker_sep.scene import Scene, SceneProfile, get_profile


@dataclass
class IngestStage:
    name: str
    detail: str


@dataclass
class IngestResult:
    """Describes exactly how audio entered the diarization pipeline."""

    waveform: torch.Tensor
    sample_rate: int
    scene: Scene
    source: str
    channels_in: int
    channel_selected: int | None
    duration_sec: float
    stages: list[IngestStage] = field(default_factory=list)
    profile: dict[str, Any] = field(default_factory=dict)

    def save_report(self, path: Path) -> None:
        payload = {
            "source": self.source,
            "scene": self.scene,
            "sample_rate": self.sample_rate,
            "duration_sec": round(self.duration_sec, 3),
            "channels_in": self.channels_in,
            "channel_selected": self.channel_selected,
            "stages": [asdict(s) for s in self.stages],
            "profile": self.profile,
        }
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")


def apply_scene_to_settings(settings: Settings, scene: Scene) -> Settings:
    profile = get_profile(scene)
    updates: dict[str, Any] = {
        "scene": scene,
        "stream_window_sec": profile.stream_window_sec,
        "stream_step_sec": profile.stream_step_sec,
        "stream_min_buffer_sec": profile.stream_min_buffer_sec,
        "min_segment_sec": profile.min_segment_sec,
        "max_dominant_speakers": profile.max_dominant_speakers,
        "overlap_energy_split": profile.separation_energy_split,
        "merge_similarity_threshold": profile.merge_similarity_threshold,
    }
    if settings.max_speakers is None and profile.default_max_speakers is not None:
        updates["max_speakers"] = profile.default_max_speakers
    return settings.model_copy(update=updates)


class AudioIngest:
    """
    Crowd-aware audio entry path:

    capture → channel select → resample → preprocess → (downstream diarization)
    """

    def __init__(self, settings: Settings) -> None:
        self.settings = settings
        self.profile: SceneProfile = get_profile(settings.scene)

    def from_file(self, path: str | Path) -> IngestResult:
        path = Path(path)
        stages: list[IngestStage] = [
            IngestStage("capture", f"Read file: {path.name}"),
        ]

        waveform, sr = torchaudio.load(str(path))
        channels_in = waveform.shape[0]
        channel_selected: int | None = None

        stages.append(
            IngestStage(
                "channel_select",
                f"strategy={self.profile.channel_strategy}, channels_in={channels_in}",
            ),
        )
        if channels_in > 1 and self.profile.channel_strategy == "best_speech_band":
            scores = []
            for c in range(channels_in):
                from speaker_sep.preprocess import speech_band_energy

                scores.append(speech_band_energy(waveform[c : c + 1], sr))
            channel_selected = int(max(range(len(scores)), key=lambda i: scores[i]))
            waveform = waveform[channel_selected : channel_selected + 1]
        else:
            waveform = downmix(waveform, self.profile.channel_strategy, sr)

        if sr != self.settings.sample_rate:
            stages.append(
                IngestStage("resample", f"{sr} Hz → {self.settings.sample_rate} Hz"),
            )
            waveform = torchaudio.functional.resample(
                waveform,
                sr,
                self.settings.sample_rate,
            )
            sr = self.settings.sample_rate

        stages.append(
            IngestStage(
                "preprocess",
                f"highpass={self.profile.highpass_hz}Hz, "
                f"rms={self.profile.target_rms_db}dB, "
                f"gate={self.profile.noise_gate_db}dB",
            ),
        )
        waveform = preprocess_waveform(
            waveform,
            sr,
            highpass_hz=self.profile.highpass_hz,
            target_rms_db=self.profile.target_rms_db,
            noise_gate_db=self.profile.noise_gate_db,
        )

        duration = waveform.shape[-1] / sr
        stages.append(IngestStage("ready", "Mono 16 kHz tensor ready for diarization"))

        return IngestResult(
            waveform=waveform,
            sample_rate=sr,
            scene=self.settings.scene,
            source=str(path),
            channels_in=channels_in,
            channel_selected=channel_selected,
            duration_sec=duration,
            stages=stages,
            profile={
                "channel_strategy": self.profile.channel_strategy,
                "stream_window_sec": self.profile.stream_window_sec,
                "clustering_threshold": self.profile.clustering_threshold,
            },
        )

    def from_tensor(
        self,
        waveform: torch.Tensor,
        sample_rate: int,
        *,
        source: str = "buffer",
        channels_in: int | None = None,
    ) -> IngestResult:
        channels_in = channels_in or waveform.shape[0]
        channel_selected = None
        w = waveform
        if w.shape[0] > 1:
            if self.profile.channel_strategy == "best_speech_band":
                from speaker_sep.preprocess import select_best_channel

                scores_before = w.shape[0]
                w = select_best_channel(w, sample_rate)
                for c in range(scores_before):
                    from speaker_sep.preprocess import speech_band_energy

                    if torch.allclose(
                        w,
                        waveform[c : c + 1],
                        atol=1e-6,
                    ):
                        channel_selected = c
                        break
            else:
                w = downmix(w, self.profile.channel_strategy, sample_rate)
        if sample_rate != self.settings.sample_rate:
            w = torchaudio.functional.resample(w, sample_rate, self.settings.sample_rate)
            sample_rate = self.settings.sample_rate
        w = preprocess_waveform(
            w,
            sample_rate,
            highpass_hz=self.profile.highpass_hz,
            target_rms_db=self.profile.target_rms_db,
            noise_gate_db=self.profile.noise_gate_db,
        )
        return IngestResult(
            waveform=w,
            sample_rate=sample_rate,
            scene=self.settings.scene,
            source=source,
            channels_in=channels_in,
            channel_selected=channel_selected,
            duration_sec=w.shape[-1] / sample_rate,
            stages=[
                IngestStage("capture", "In-memory stream buffer"),
                IngestStage("preprocess", f"scene={self.settings.scene}"),
                IngestStage("ready", "Ready for diarization"),
            ],
            profile={"channel_strategy": self.profile.channel_strategy},
        )
