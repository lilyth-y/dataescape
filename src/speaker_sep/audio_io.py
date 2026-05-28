from __future__ import annotations

from collections.abc import Callable, Iterator
from pathlib import Path

import numpy as np
import soundfile as sf
import torch
import torchaudio


def load_mono(path: str | Path, sample_rate: int = 16_000) -> tuple[torch.Tensor, int]:
    """Load audio as mono float tensor shaped (1, num_samples)."""
    waveform, sr = torchaudio.load(str(path))
    if waveform.shape[0] > 1:
        waveform = waveform.mean(dim=0, keepdim=True)
    if sr != sample_rate:
        waveform = torchaudio.functional.resample(waveform, sr, sample_rate)
    return waveform, sample_rate


def tensor_to_numpy(waveform: torch.Tensor) -> np.ndarray:
    w = waveform.detach().cpu()
    if w.ndim == 2:
        w = w[0]
    return w.numpy().astype(np.float32)


def save_wav(path: Path, audio: np.ndarray, sample_rate: int) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    sf.write(str(path), audio, sample_rate, subtype="FLOAT")


class MicrophoneStream:
    """Real-time microphone chunk iterator."""

    def __init__(
        self,
        sample_rate: int = 16_000,
        block_duration_sec: float = 0.5,
        device: int | None = None,
    ) -> None:
        import sounddevice as sd

        self.sample_rate = sample_rate
        self.blocksize = int(sample_rate * block_duration_sec)
        self.device = device
        self._sd = sd

    def chunks(self) -> Iterator[np.ndarray]:
        with self._sd.InputStream(
            samplerate=self.sample_rate,
            channels=1,
            dtype="float32",
            blocksize=self.blocksize,
            device=self.device,
        ) as stream:
            while True:
                data, _ = stream.read(self.blocksize)
                yield data[:, 0].copy()


class FileChunkStream:
    """Simulate a live stream by chunking a file."""

    def __init__(
        self,
        path: str | Path,
        sample_rate: int = 16_000,
        block_duration_sec: float = 0.5,
    ) -> None:
        waveform, _ = load_mono(path, sample_rate)
        self.audio = tensor_to_numpy(waveform)
        self.sample_rate = sample_rate
        self.blocksize = int(sample_rate * block_duration_sec)

    def chunks(self) -> Iterator[np.ndarray]:
        for start in range(0, len(self.audio), self.blocksize):
            yield self.audio[start : start + self.blocksize]


def iter_stream(
    source: str | Path | Callable[[], Iterator[np.ndarray]],
    sample_rate: int = 16_000,
    block_duration_sec: float = 0.5,
    mic_device: int | None = None,
) -> Iterator[np.ndarray]:
    if callable(source):
        yield from source()
    path = Path(str(source))
    if path.exists():
        yield from FileChunkStream(path, sample_rate, block_duration_sec).chunks()
        return
    if str(source) in ("mic", "microphone", "-"):
        yield from MicrophoneStream(sample_rate, block_duration_sec, mic_device).chunks()
        return
    raise FileNotFoundError(f"Unknown audio source: {source}")
