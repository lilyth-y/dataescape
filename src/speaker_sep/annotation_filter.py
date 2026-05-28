from __future__ import annotations

from pyannote.core import Annotation, Segment


def filter_short_segments(annotation: Annotation, min_duration_sec: float) -> Annotation:
    if min_duration_sec <= 0:
        return annotation
    out = Annotation(uri=annotation.uri)
    for segment, track, label in annotation.itertracks(yield_label=True):
        if segment.duration >= min_duration_sec:
            out[segment, track] = label
    return out


def keep_dominant_speakers(annotation: Annotation, max_speakers: int | None) -> Annotation:
    """In crowds, keep only the N voices with the most speech time."""
    if max_speakers is None or max_speakers <= 0:
        return annotation

    durations: dict[str, float] = {}
    for segment, _, label in annotation.itertracks(yield_label=True):
        key = str(label)
        durations[key] = durations.get(key, 0.0) + segment.duration

    if len(durations) <= max_speakers:
        return annotation

    keep = {s for s, _ in sorted(durations.items(), key=lambda x: -x[1])[:max_speakers]}
    out = Annotation(uri=annotation.uri)
    for segment, track, label in annotation.itertracks(yield_label=True):
        if str(label) in keep:
            out[segment, track] = label
    return out
