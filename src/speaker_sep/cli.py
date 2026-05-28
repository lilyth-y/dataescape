from __future__ import annotations

from pathlib import Path

import click
from rich.console import Console

from speaker_sep.config import Settings
from speaker_sep.offline import OfflineDiarizer
from speaker_sep.streaming import StreamingDiarizer

console = Console()


@click.group()
@click.version_option(package_name="speaker-sep")
def main() -> None:
    """Speaker diarization: per-speaker audio + JSON timeline (GPU, variable speakers)."""


@main.command("file")
@click.argument("audio", type=click.Path(exists=True, path_type=Path))
@click.option("-o", "--output", type=click.Path(path_type=Path), default=None, help="Output directory")
@click.option("--min-speakers", type=int, default=None)
@click.option("--max-speakers", type=int, default=None)
@click.option("--device", type=click.Choice(["auto", "cuda", "cpu"]), default="auto")
@click.option("--pipeline", default=None, help="HuggingFace pipeline id")
def file_cmd(
    audio: Path,
    output: Path | None,
    min_speakers: int | None,
    max_speakers: int | None,
    device: str,
    pipeline: str | None,
) -> None:
    """Diarize an audio file."""
    settings = Settings(
        device=device,  # type: ignore[arg-type]
        min_speakers=min_speakers,
        max_speakers=max_speakers,
    )
    if pipeline:
        settings = settings.model_copy(update={"pipeline_id": pipeline})

    result = OfflineDiarizer(settings).run(audio, output)
    _print_result(result)


@main.command("stream")
@click.argument("source", default="mic", type=str)
@click.option("-o", "--output", type=click.Path(path_type=Path), default=None)
@click.option("--max-duration", type=float, default=None, help="Stop after N seconds (file/mic)")
@click.option("--window", type=float, default=None, help="Sliding window seconds")
@click.option("--step", type=float, default=None, help="Inference step seconds")
@click.option("--device", type=click.Choice(["auto", "cuda", "cpu"]), default="auto")
def stream_cmd(
    source: str,
    output: Path | None,
    max_duration: float | None,
    window: float | None,
    step: float | None,
    device: str,
) -> None:
    """Real-time or simulated streaming diarization."""
    updates: dict = {}
    if window is not None:
        updates["stream_window_sec"] = window
    if step is not None:
        updates["stream_step_sec"] = step

    settings = Settings(device=device, **updates)  # type: ignore[arg-type]
    result = StreamingDiarizer(settings).run(
        source,
        output,
        max_duration_sec=max_duration,
    )
    _print_result(result)


def _print_result(result: object) -> None:
    from speaker_sep.export import DiarizationResult

    if not isinstance(result, DiarizationResult):
        return
    console.print(f"[green]Done.[/] Output: {result.output_dir}")
    console.print(f"  timeline: {result.output_dir / 'timeline.json'}")
    for spk, path in sorted(result.speaker_wavs.items()):
        console.print(f"  {spk}: {path}")


if __name__ == "__main__":
    main()
