from __future__ import annotations

from pathlib import Path

import click
from rich.console import Console

from speaker_sep.config import Settings
from speaker_sep.ingest import apply_scene_to_settings
from speaker_sep.offline import OfflineDiarizer
from speaker_sep.streaming import StreamingDiarizer

console = Console()


def _build_settings(
    *,
    device: str,
    scene: str,
    min_speakers: int | None,
    max_speakers: int | None,
    pipeline: str | None,
    window: float | None,
    step: float | None,
) -> Settings:
    settings = Settings(
        device=device,  # type: ignore[arg-type]
        scene=scene,  # type: ignore[arg-type]
        min_speakers=min_speakers,
        max_speakers=max_speakers,
    )
    settings = apply_scene_to_settings(settings, scene)  # type: ignore[arg-type]
    if pipeline:
        settings = settings.model_copy(update={"pipeline_id": pipeline})
    updates: dict = {}
    if window is not None:
        updates["stream_window_sec"] = window
    if step is not None:
        updates["stream_step_sec"] = step
    if updates:
        settings = settings.model_copy(update=updates)
    return settings


@click.group()
@click.version_option(package_name="speaker-sep")
def main() -> None:
    """Speaker diarization: per-speaker audio + JSON timeline (GPU, variable speakers)."""


@main.command("file")
@click.argument("audio", type=click.Path(exists=True, path_type=Path))
@click.option("-o", "--output", type=click.Path(path_type=Path), default=None, help="Output directory")
@click.option(
    "--scene",
    type=click.Choice(["default", "crowd", "meeting"]),
    default="default",
    help="Acoustic scene preset (crowd = public venue / dense babble)",
)
@click.option("--min-speakers", type=int, default=None)
@click.option("--max-speakers", type=int, default=None)
@click.option("--device", type=click.Choice(["auto", "cuda", "cpu"]), default="auto")
@click.option("--pipeline", default=None, help="HuggingFace pipeline id")
def file_cmd(
    audio: Path,
    output: Path | None,
    scene: str,
    min_speakers: int | None,
    max_speakers: int | None,
    device: str,
    pipeline: str | None,
) -> None:
    """Diarize an audio file."""
    settings = _build_settings(
        device=device,
        scene=scene,
        min_speakers=min_speakers,
        max_speakers=max_speakers,
        pipeline=pipeline,
        window=None,
        step=None,
    )
    result = OfflineDiarizer(settings).run(audio, output)
    _print_result(result)


@main.command("stream")
@click.argument("source", default="mic", type=str)
@click.option("-o", "--output", type=click.Path(path_type=Path), default=None)
@click.option(
    "--scene",
    type=click.Choice(["default", "crowd", "meeting"]),
    default="default",
)
@click.option("--max-duration", type=float, default=None, help="Stop after N seconds (file/mic)")
@click.option("--window", type=float, default=None, help="Sliding window seconds")
@click.option("--step", type=float, default=None, help="Inference step seconds")
@click.option("--device", type=click.Choice(["auto", "cuda", "cpu"]), default="auto")
def stream_cmd(
    source: str,
    output: Path | None,
    scene: str,
    max_duration: float | None,
    window: float | None,
    step: float | None,
    device: str,
) -> None:
    """Real-time or simulated streaming diarization."""
    settings = _build_settings(
        device=device,
        scene=scene,
        min_speakers=None,
        max_speakers=None,
        pipeline=None,
        window=window,
        step=step,
    )
    result = StreamingDiarizer(settings).run(
        source,
        output,
        max_duration_sec=max_duration,
    )
    _print_result(result)


@main.command("scenes")
def scenes_cmd() -> None:
    """List scene presets and how audio enters the pipeline."""
    from speaker_sep.scene import PROFILES

    for name, profile in PROFILES.items():
        console.print(f"\n[bold]{name}[/]: {profile.description}")
        console.print(
            f"  ingest: channel={profile.channel_strategy}, "
            f"highpass={profile.highpass_hz}Hz, gate={profile.noise_gate_db}dB",
        )
        console.print(
            f"  stream: window={profile.stream_window_sec}s step={profile.stream_step_sec}s",
        )


def _print_result(result: object) -> None:
    from speaker_sep.export import DiarizationResult

    if not isinstance(result, DiarizationResult):
        return
    console.print(f"[green]Done.[/] Output: {result.output_dir}")
    console.print(f"  ingest:   {result.output_dir / 'ingest.json'}")
    console.print(f"  timeline: {result.output_dir / 'timeline.json'}")
    for spk, path in sorted(result.speaker_wavs.items()):
        console.print(f"  {spk}: {path}")


if __name__ == "__main__":
    main()
