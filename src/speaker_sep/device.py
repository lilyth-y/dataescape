from __future__ import annotations

import torch

from speaker_sep.config import Settings


def resolve_device(settings: Settings) -> torch.device:
    if settings.device == "cpu":
        return torch.device("cpu")
    if settings.device == "cuda":
        if not torch.cuda.is_available():
            raise RuntimeError("SPEAKER_SEP_DEVICE=cuda but CUDA is not available")
        return torch.device("cuda")
    # auto
    return torch.device("cuda" if torch.cuda.is_available() else "cpu")
