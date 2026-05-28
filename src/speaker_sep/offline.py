from __future__ import annotations

import tempfile
from pathlib import Path

import torch
from pyannote.audio.pipelines.utils.hook import ProgressHook
from pyannote.core import Annotation
from rich.console import Console

from speaker_sep.annotation_filter import filter_short_segments, keep_dominant_speakers
from speaker_sep.config import Settings
from speaker_sep.device import resolve_device
from speaker_sep.export import DiarizationResult, export_results
from speaker_sep.ingest import AudioIngest
from speaker_sep.models import PipelineLoadError, get_pipeline
from speaker_sep.timeline import annotation_to_timeline

console = Console()


class OfflineDiarizer:
    """Batch diarization for audio files (pyannote community-1)."""

    def __init__(self, settings: Settings | None = None) -> None:
        self.settings = settings or Settings()

    def _postprocess(self, annotation: Annotation) -> Annotation:
        ann = filter_short_segments(annotation, self.settings.min_segment_sec)
        return keep_dominant_speakers(ann, self.settings.max_dominant_speakers)

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
        except PipelineLoadError:
            raise

        ingest = AudioIngest(self.settings)
        ingested = ingest.from_file(audio_path)
        ingested.save_report(out / "ingest.json")

        console.print(
            f"[bold]Offline diarization[/] scene={self.settings.scene} "
            f"{audio_path.name} → {out} ({device})",
        )
        if ingested.channels_in > 1:
            console.print(
                f"  [cyan]군중/다채널 입력:[/] {ingested.channels_in}ch → "
                f"선택 ch={ingested.channel_selected if ingested.channel_selected is not None else 0}",
            )

        waveform = ingested.waveform
        duration = ingested.duration_sec

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

            if show_progress:
                with ProgressHook() as hook:
                    output = pipeline(tmp_path, hook=hook, **kwargs)
            else:
                output = pipeline(tmp_path, **kwargs)
        finally:
            Path(tmp_path).unlink(missing_ok=True)

        annotation = self._postprocess(output.speaker_diarization)
        timeline = annotation_to_timeline(
            annotation,
            source=str(audio_path),
            sample_rate=self.settings.sample_rate,
            pipeline=self.settings.pipeline_id,
            device=str(device),
            mode="file",
            duration_sec=duration,
            scene=self.settings.scene,
            ingest_stages=[s.name for s in ingested.stages],
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
        sample_rate: int | None = None,
    ) -> tuple[object, Annotation]:
        """Run pipeline on in-memory audio (used by streaming)."""
        pipeline = get_pipeline(self.settings)
        sr = sample_rate or self.settings.sample_rate
        ingested = AudioIngest(self.settings).from_tensor(
            waveform,
            sr,
            source=uri,
            channels_in=waveform.shape[0],
        )
        kwargs: dict = {}
        if self.settings.min_speakers is not None:
            kwargs["min_speakers"] = self.settings.min_speakers
        if self.settings.max_speakers is not None:
            kwargs["max_speakers"] = self.settings.max_speakers

        with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as tmp:
            tmp_path = tmp.name

        try:
            import soundfile as sf

            audio = ingested.waveform.detach().cpu()
            if audio.ndim == 2:
                audio = audio[0]
            sf.write(tmp_path, audio.numpy(), self.settings.sample_rate)
            output = pipeline(tmp_path, **kwargs)
            ann = self._postprocess(output.speaker_diarization)
            return output, ann
        finally:
            Path(tmp_path).unlink(missing_ok=True)
