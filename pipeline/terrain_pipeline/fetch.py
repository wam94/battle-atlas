"""Query and download USGS 3DEP 1 m DEM GeoTIFFs via the TNM Access API."""
from pathlib import Path

import requests

TNM_API = "https://tnmaccess.nationalmap.gov/api/v1/products"
DATASET = "Digital Elevation Model (DEM) 1 meter"


def query_dem_products(bbox_wgs84, get=requests.get):
    west, south, east, north = bbox_wgs84
    params = {
        "datasets": DATASET,
        "bbox": f"{west},{south},{east},{north}",
        "prodFormats": "GeoTIFF",
        "outputFormat": "JSON",
        "max": 100,
    }
    resp = get(TNM_API, params=params, timeout=60)
    resp.raise_for_status()
    items = resp.json()["items"]
    return [{"title": i["title"], "url": i["downloadURL"]} for i in items]


def download_products(products, dest_dir, get=requests.get):
    dest = Path(dest_dir)
    dest.mkdir(parents=True, exist_ok=True)
    paths = []
    for p in products:
        out = dest / p["url"].rsplit("/", 1)[-1]
        if not out.exists():
            resp = get(p["url"], timeout=600)
            resp.raise_for_status()
            out.write_bytes(resp.content)
        paths.append(out)
    return paths
