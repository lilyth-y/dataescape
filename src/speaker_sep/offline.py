from __future__ import annotations

import tempfile
from pathlib import Path

import torch
from pyannote.audio.pipelines.utils.hook import ProgressHook
from rich.console import Console

from speaker_sep.audio_io import load_mono
from speaker_sep.config import Settings
from speaker_sep.device import resolve_device
from speaker_sep.export import DiarizationResult, export_results
from speaker_sep.models import PipelineLoadError, get_pipeline
from speaker_sep.timeline import annotation_to_timeline

console = Console()


class OfflineDiarizer:
    """Batch diarization for audio files (pyannote community-1)."""

    def __init__(self, settings: Settings | None = None) -> None:
        self.settings = settings or Settings()

    def run(
        self,
        audio_path: str | Path,
        output_dir: str | Path | None = None,
        *,
        show_progress: bool = True,
    ) -> DiarizationResult:
        audio_path = Path(audio_path)
        if not audio_path.exists():
            raise FileNotFoundError(audio_path)

        out = Path(output_dir or self.settings.output_dir / audio_path.stem)
        device = resolve_device(self.settings)

        try:
            pipeline = get_pipeline(self.settings)
        except PipelineLoadError as exc:
            raise

        console.print(
            f"[bold]Offline diarization[/] {audio_path.name} "
            f"→ {out} ({device})",
        )

        waveform, _ = load_mono(audio_path, self.settings.sample_rate)
        duration = waveform.shape[1] / self.settings.sample_rate

        kwargs: dict = {}
        if self.settings.min_speakers is not None:
            kwargs["min_speakers"] = self.settings.min_speakers
        if self.settings.max_speakers is not None:
            kwargs["max_speakers"] = self.settings.max_speakers

        if show_progress:
            with ProgressHook() as hook:
                output = pipeline(str(audio_path), hook=hook, **kwargs)
        else:
            output = pipeline(str(audio_path), **kwargs)

        annotation = output.speaker_diarization
        timeline = annotation_to_timeline(
            annotation,
            source=str(audio_path),
            sample_rate=self.settings.sample_rate,
            pipeline=self.settings.pipeline_id,
            device=str(device),
            mode="file",
            duration_sec=duration,
        )

        return export_results(
            waveform,
            annotation,
            timeline,
            self.settings,
            out,
        )

    def run_tensor(
        self,
        waveform: torch.Tensor,
        *,
        uri: str = "stream_buffer",
        output_dir: str | Path | None = None,
    ) -> tuple[object, Annotation]:
        """Run pipeline on in-memory audio (used by streaming)."""
        pipeline = get_pipeline(self.settings)
        kwargs: dict = {}
        if self.settings.min_speakers is not None:
            kwargs["min_speakers"] = self.settings.min_speakers
        if self.settings.max_speakers is not None:
            kwargs["max_speakers"] = self.settings.max_speakers

        with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as tmp:
            tmp_path = tmp.name

        try:
            import soundfile as sf

            audio = waveform.detach().cpu()
            if audio.ndim == 2:
                audio = audio[0]
            sf.write(tmp_path, audio.numpy(), self.settings.sample_rate)
            output = pipeline(tmp_path, **kwargs)
            return output, output.speaker_diarization
        finally:
            Path(tmp_path).unlink(missing_ok=True)
