from pathlib import Path

from terrain_pipeline import fetch


class FakeResponse:
    def __init__(self, payload=None, content=b""):
        self._payload = payload
        self.content = content

    def raise_for_status(self):
        pass

    def json(self):
        return self._payload


def test_query_parses_products():
    captured = {}

    def fake_get(url, params=None, timeout=None):
        captured["url"] = url
        captured["params"] = params
        return FakeResponse(payload={"items": [
            {"title": "USGS 1m x44y441 PA", "downloadURL": "https://example.com/a.tif"},
            {"title": "USGS 1m x45y441 PA", "downloadURL": "https://example.com/b.tif"},
        ]})

    products = fetch.query_dem_products((-77.28, 39.77, -77.195, 39.845), get=fake_get)
    assert captured["url"] == fetch.TNM_API
    assert captured["params"]["bbox"] == "-77.28,39.77,-77.195,39.845"
    assert captured["params"]["datasets"] == fetch.DATASET
    assert captured["params"]["max"] == 100
    assert [p["url"] for p in products] == ["https://example.com/a.tif", "https://example.com/b.tif"]


def test_download_writes_and_skips_existing(tmp_path: Path):
    calls = []

    def fake_get(url, timeout=None):
        calls.append(url)
        return FakeResponse(content=b"tif-bytes")

    products = [{"title": "t", "url": "https://example.com/tile.tif"}]
    paths = fetch.download_products(products, tmp_path, get=fake_get)
    assert paths[0].read_bytes() == b"tif-bytes"

    fetch.download_products(products, tmp_path, get=fake_get)  # second call: cached
    assert len(calls) == 1
