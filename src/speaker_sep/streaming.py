from __future__ import annotations

from collections.abc import Callable, Iterator
from pathlib import Path

import numpy as np
import torch
from pyannote.core import Annotation
from rich.console import Console

from speaker_sep.audio_io import iter_stream, tensor_to_numpy
from speaker_sep.config import Settings
from speaker_sep.device import resolve_device
from speaker_sep.export import DiarizationResult, export_results
from speaker_sep.offline import OfflineDiarizer
from speaker_sep.timeline import (
    DiarizationTimeline,
    annotation_to_timeline,
    merge_annotations,
)

console = Console()


class StreamingDiarizer:
    """
    Incremental diarization for real-time streams.

    Uses a sliding window over accumulated audio and merges speaker labels
    across windows via embedding similarity (from pyannote when available).
  """

    def __init__(self, settings: Settings | None = None) -> None:
        self.settings = settings or Settings()
        self._offline = OfflineDiarizer(self.settings)

    def run(
        self,
        source: str | Path | Callable[[], Iterator[np.ndarray]],
        output_dir: str | Path | None = None,
        *,
        max_duration_sec: float | None = None,
        on_update: Callable[[DiarizationTimeline], None] | None = None,
    ) -> DiarizationResult:
        device = resolve_device(self.settings)
        out = Path(output_dir or self.settings.output_dir / "stream")
        out.mkdir(parents=True, exist_ok=True)

        buffer: list[np.ndarray] = []
        global_ann: Annotation | None = None
        embedding_map: dict[str, np.ndarray] = {}
        last_infer_sample = 0
        total_samples = 0
        sr = self.settings.sample_rate

        window_samples = int(self.settings.stream_window_sec * sr)
        step_samples = int(self.settings.stream_step_sec * sr)
        min_samples = int(self.settings.stream_min_buffer_sec * sr)

        console.print(
            f"[bold]Streaming diarization[/] scene={self.settings.scene} "
            f"window={self.settings.stream_window_sec}s "
            f"step={self.settings.stream_step_sec}s ({device})",
        )
        if self.settings.scene == "crowd":
            console.print(
                "[cyan]군중 모드:[/] 긴 윈도우 + 음성대역 채널 선택 + 짧은 발화 제거 → "
                "군중 속 [dominant] 화자 위주 추적",
            )

        for chunk in iter_stream(
            source,
            sample_rate=sr,
            block_duration_sec=self.settings.stream_step_sec,
        ):
            buffer.append(chunk)
            total_samples += len(chunk)

            if max_duration_sec and total_samples / sr > max_duration_sec:
                break

            if total_samples < min_samples:
                continue

            if total_samples - last_infer_sample < step_samples:
                continue

            audio_np = np.concatenate(buffer)[-window_samples:]
            waveform = torch.from_numpy(audio_np).float().unsqueeze(0)

            try:
                output, window_ann = self._offline.run_tensor(
                    waveform,
                    uri="stream",
                )
            except Exception as exc:
                console.print(f"[yellow]Skip window:[/] {exc}")
                last_infer_sample = total_samples
                continue

            # Offset: window is tail of buffer
            buffer_start_sec = max(0.0, total_samples / sr - len(audio_np) / sr)
            embs = _embeddings_from_output(output)
            global_ann, embedding_map = merge_annotations(
                global_ann,
                window_ann,
                time_offset=buffer_start_sec,
                embedding_map=embedding_map,
                new_embeddings=embs,
                similarity_threshold=self.settings.merge_similarity_threshold,
            )

            last_infer_sample = total_samples

            if global_ann is not None and on_update:
                timeline = annotation_to_timeline(
                    global_ann,
                    source=str(source),
                    sample_rate=sr,
                    pipeline=self.settings.pipeline_id,
                    device=str(device),
                    mode="stream",
                    duration_sec=total_samples / sr,
                    scene=self.settings.scene,
                )
                on_update(timeline)
                timeline.save_json(out / "timeline.json")

        if global_ann is None:
            raise RuntimeError("No diarization produced; stream too short or pipeline failed")

        full_audio = np.concatenate(buffer)
        waveform = torch.from_numpy(full_audio).float().unsqueeze(0)
        timeline = annotation_to_timeline(
            global_ann,
            source=str(source),
            sample_rate=sr,
            pipeline=self.settings.pipeline_id,
            device=str(device),
            mode="stream",
            duration_sec=len(full_audio) / sr,
            scene=self.settings.scene,
        )

        return export_results(
            waveform,
            global_ann,
            timeline,
            self.settings,
            out,
        )


def _embeddings_from_output(output: object) -> dict[str, np.ndarray]:
    embs = getattr(output, "speaker_embeddings", None)
    ann = getattr(output, "speaker_diarization", None)
    if embs is None or ann is None:
        return {}
    labels = list(ann.labels())
    result: dict[str, np.ndarray] = {}
    for i, label in enumerate(labels):
        if i < len(embs):
            vec = np.asarray(embs[i], dtype=np.float32)
            vec /= np.linalg.norm(vec) + 1e-8
            result[str(label)] = vec
    return result
