"""Speaker diarization with per-speaker audio export and JSON timelines."""

__version__ = "0.1.0"

from speaker_sep.config import Settings
from speaker_sep.offline import OfflineDiarizer
from speaker_sep.streaming import StreamingDiarizer

__all__ = ["Settings", "OfflineDiarizer", "StreamingDiarizer", "__version__"]
