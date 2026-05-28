from pyannote.core import Annotation, Segment

from speaker_sep.timeline import annotation_to_timeline, merge_annotations


def test_annotation_to_timeline_overlap():
    ann = Annotation()
    ann[Segment(0, 2)] = "A"
    ann[Segment(1, 3)] = "B"

    tl = annotation_to_timeline(
        ann,
        source="test",
        sample_rate=16000,
        pipeline="test",
        device="cpu",
        mode="file",
    )
    assert "A" in tl.speakers or "B" in tl.speakers
    overlap_segs = [s for s in tl.segments if s.overlap]
    assert len(overlap_segs) >= 1


def test_merge_annotations_assigns_global_labels():
    a = Annotation()
    a[Segment(0, 1)] = "0"
    b = Annotation()
    b[Segment(0, 1)] = "0"

    merged, _ = merge_annotations(None, a, time_offset=0.0)
    merged2, _ = merge_annotations(merged, b, time_offset=1.0)
    labels = {str(l) for _, _, l in merged2.itertracks(yield_label=True)}
    assert all(l.startswith("SPEAKER_") for l in labels)
