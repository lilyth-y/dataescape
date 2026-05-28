from speaker_sep.config import Settings
from speaker_sep.device import resolve_device


def test_settings_defaults():
    s = Settings(hf_token="test")
    assert "community-1" in s.pipeline_id
    assert s.sample_rate == 16000


def test_resolve_device_cpu():
    s = Settings(hf_token="x", device="cpu")
    assert str(resolve_device(s)) == "cpu"
