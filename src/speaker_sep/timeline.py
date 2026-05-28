from __future__ import annotations

import json
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import numpy as np
from pyannote.core import Annotation, Segment


@dataclass
class TimelineSegment:
    speaker: str
    start: float
    end: float
    overlap: bool = False
    co_speakers: list[str] = field(default_factory=list)


@dataclass
class DiarizationTimeline:
    source: str
    sample_rate: int
    speakers: list[str]
    segments: list[TimelineSegment]
    pipeline: str
    device: str
    mode: str  # "file" | "stream"
    scene: str = "default"
    ingest_stages: list[str] = field(default_factory=list)
    duration_sec: float | None = None
    created_at: str = field(
        default_factory=lambda: datetime.now(timezone.utc).isoformat(),
    )

    def to_dict(self) -> dict[str, Any]:
        return {
            "source": self.source,
            "sample_rate": self.sample_rate,
            "duration_sec": self.duration_sec,
            "speakers": self.speakers,
            "segments": [
                {
                    "speaker": s.speaker,
                    "start": round(s.start, 3),
                    "end": round(s.end, 3),
                    "overlap": s.overlap,
                    **({"co_speakers": s.co_speakers} if s.co_speakers else {}),
                }
                for s in self.segments
            ],
            "meta": {
                "pipeline": self.pipeline,
                "device": self.device,
                "mode": self.mode,
                "scene": self.scene,
                "ingest_stages": self.ingest_stages,
                "created_at": self.created_at,
            },
        }

    def save_json(self, path: Path) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(
            json.dumps(self.to_dict(), indent=2, ensure_ascii=False),
            encoding="utf-8",
        )


def _active_speakers_at(annotation: Annotation, t: float) -> list[str]:
    active: list[str] = []
    for segment, _, label in annotation.itertracks(yield_label=True):
        if segment.start <= t < segment.end:
            active.append(str(label))
    return sorted(set(active))


def annotation_to_timeline(
    annotation: Annotation,
    *,
    source: str,
    sample_rate: int,
    pipeline: str,
    device: str,
    mode: str,
    duration_sec: float | None = None,
    scene: str = "default",
    ingest_stages: list[str] | None = None,
) -> DiarizationTimeline:
    segments: list[TimelineSegment] = []
    for turn, _, speaker in annotation.itertracks(yield_label=True):
        spk = str(speaker)
        mid = 0.5 * (turn.start + turn.end)
        co = [s for s in _active_speakers_at(annotation, mid) if s != spk]
        segments.append(
            TimelineSegment(
                speaker=spk,
                start=float(turn.start),
                end=float(turn.end),
                overlap=len(co) > 0,
                co_speakers=co,
            ),
        )

    speakers = sorted({str(l) for _, _, l in annotation.itertracks(yield_label=True)})
    return DiarizationTimeline(
        source=source,
        sample_rate=sample_rate,
        speakers=speakers,
        segments=sorted(segments, key=lambda s: (s.start, s.speaker)),
        pipeline=pipeline,
        device=device,
        mode=mode,
        scene=scene,
        ingest_stages=ingest_stages or [],
        duration_sec=duration_sec,
    )


def merge_annotations(
    base: Annotation | None,
    incoming: Annotation,
    time_offset: float,
    embedding_map: dict[str, np.ndarray] | None = None,
    new_embeddings: dict[str, np.ndarray] | None = None,
    similarity_threshold: float = 0.65,
) -> tuple[Annotation, dict[str, np.ndarray]]:
    """Merge a window diarization into the global timeline."""
    global_embs = dict(embedding_map or {})
    new_embs = new_embeddings or {}

    # Shift incoming segments to global time
    shifted = Annotation(uri=incoming.uri)
    for segment, track, label in incoming.itertracks(yield_label=True):
        shifted[
            Segment(segment.start + time_offset, segment.end + time_offset),
            track,
        ] = label

    if base is None:
        return _rename_speakers(shifted), _register_embeddings(shifted, new_embs, global_embs)

    # Map window-local labels → global SPEAKER_XX
    label_map: dict[str, str] = {}
    for label in shifted.labels():
        key = str(label)
        vec = new_embs.get(key)
        if vec is None:
            vec = global_embs.get(key)
        if vec is None:
            label_map[key] = f"SPEAKER_{len(global_embs):02d}"
            global_embs[label_map[key]] = _random_unit_vector(key)
            continue

        best_label: str | None = None
        best_score = -1.0
        for glob_label, glob_vec in global_embs.items():
            score = _cosine(vec, glob_vec)
            if score > best_score:
                best_score = score
                best_label = glob_label

        if best_label is not None and best_score >= similarity_threshold:
            label_map[key] = best_label
            # Exponential moving average for centroid stability
            global_embs[best_label] = 0.7 * global_embs[best_label] + 0.3 * vec
            n = np.linalg.norm(global_embs[best_label])
            if n > 0:
                global_embs[best_label] /= n
        else:
            label_map[key] = f"SPEAKER_{len(global_embs):02d}"
            global_embs[label_map[key]] = vec

    merged = base
    for segment, track, label in shifted.itertracks(yield_label=True):
        mapped = label_map.get(str(label), str(label))
        merged[segment, track] = mapped

    return merged, global_embs


def _rename_speakers(annotation: Annotation) -> Annotation:
    out = Annotation(uri=annotation.uri)
    mapping = {
        str(label): f"SPEAKER_{i:02d}" for i, label in enumerate(annotation.labels())
    }
    for segment, track, label in annotation.itertracks(yield_label=True):
        out[segment, track] = mapping.get(str(label), str(label))
    return out


def _register_embeddings(
    annotation: Annotation,
    new_embs: dict[str, np.ndarray],
    global_embs: dict[str, np.ndarray],
) -> dict[str, np.ndarray]:
    out = dict(global_embs)
    labels = [str(l) for l in annotation.labels()]
    for i, label in enumerate(labels):
        mapped = f"SPEAKER_{i:02d}"
        if label in new_embs:
            out[mapped] = new_embs[label]
        elif mapped not in out:
            out[mapped] = _random_unit_vector(label)
    return out


def _random_unit_vector(seed_key: str) -> np.ndarray:
    rng = np.random.default_rng(abs(hash(seed_key)) % (2**32))
    vec = rng.standard_normal(192).astype(np.float32)
    return vec / (np.linalg.norm(vec) + 1e-8)


def _cosine(a: np.ndarray, b: np.ndarray) -> float:
    return float(np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b) + 1e-8))
