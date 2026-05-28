from __future__ import annotations

import numpy as np
import torch
import torchaudio


def highpass_filter(waveform: torch.Tensor, sample_rate: int, cutoff_hz: float) -> torch.Tensor:
    if cutoff_hz <= 0:
        return waveform
    return torchaudio.functional.highpass_biquad(waveform, sample_rate, cutoff_hz)


def rms_normalize(waveform: torch.Tensor, target_db: float = -20.0) -> torch.Tensor:
    rms = torch.sqrt(torch.mean(waveform**2) + 1e-8)
    target = 10 ** (target_db / 20.0)
    gain = target / rms
    out = waveform * gain
    return torch.clamp(out, -1.0, 1.0)


def soft_noise_gate(waveform: torch.Tensor, sample_rate: int, threshold_db: float) -> torch.Tensor:
    """Attenuate frames below threshold (reduces crowd bed / HVAC rumble between talkers)."""
    hop = sample_rate // 50  # 20 ms
    audio = waveform.detach().cpu()
    if audio.ndim == 2:
        audio = audio[0]
    out = audio.clone()
    thresh = 10 ** (threshold_db / 20.0)
    for start in range(0, audio.shape[-1], hop):
        frame = audio[start : start + hop]
        if frame.numel() == 0:
            continue
        rms = torch.sqrt(torch.mean(frame**2))
        if rms < thresh:
            out[start : start + hop] *= 0.15
    if waveform.ndim == 2:
        return out.unsqueeze(0)
    return out.unsqueeze(0)


def speech_band_energy(channel: torch.Tensor, sample_rate: int) -> float:
    """Energy in ~300–3400 Hz — speech band for channel ranking in crowds."""
    if channel.ndim == 2:
        channel = channel[0]
    spec = torch.fft.rfft(channel)
    freqs = torch.fft.rfftfreq(channel.shape[-1], d=1.0 / sample_rate)
    mask = (freqs >= 300) & (freqs <= 3400)
    band = spec[:, mask] if spec.ndim > 1 else spec[mask]
    return float((band.abs() ** 2).mean())


def select_best_channel(waveform: torch.Tensor, sample_rate: int) -> torch.Tensor:
    """Pick single channel with strongest speech-band energy (not average — critical in crowds)."""
    if waveform.shape[0] == 1:
        return waveform
    scores = [speech_band_energy(waveform[c : c + 1], sample_rate) for c in range(waveform.shape[0])]
    best = int(np.argmax(scores))
    return waveform[best : best + 1]


def downmix(waveform: torch.Tensor, strategy: str, sample_rate: int) -> torch.Tensor:
    if waveform.shape[0] == 1:
        return waveform
    if strategy == "best_speech_band":
        return select_best_channel(waveform, sample_rate)
    return waveform.mean(dim=0, keepdim=True)


def preprocess_waveform(
    waveform: torch.Tensor,
    sample_rate: int,
    *,
    highpass_hz: float = 0.0,
    target_rms_db: float = -20.0,
    noise_gate_db: float | None = None,
) -> torch.Tensor:
    w = highpass_filter(waveform, sample_rate, highpass_hz)
    w = rms_normalize(w, target_rms_db)
    if noise_gate_db is not None:
        w = soft_noise_gate(w, sample_rate, noise_gate_db)
    return w
