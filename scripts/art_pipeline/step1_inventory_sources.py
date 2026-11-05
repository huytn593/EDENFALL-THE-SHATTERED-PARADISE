"""
Step 1: Inventory all art sources and copy to Assets/_Edenfall/Art/Characters/Aurelia/Sources/
Generates source_manifest.json with hashes, dimensions, format, and metadata.
"""
import os
import shutil
import hashlib
import json
from PIL import Image

def get_file_hash(filepath):
    h = hashlib.sha256()
    with open(filepath, "rb") as f:
        while chunk := f.read(65536):
            h.update(chunk)
    return h.hexdigest()

def main():
    downloads_dir = r"C:\Users\Huy Pham\Downloads\EDENFALL"
    sources_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Sources")
    manifest_path = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\source_manifest.json")
    os.makedirs(sources_dir, exist_ok=True)

    print(f"Auditing art sources in: {downloads_dir}")
    source_files = [
        "Aurelia_ Edenfall Character Design Board.png",
        "Aurelia_ Edenfall Heroine Design Sheet.png",
        "Aurelia_ Embarrassed Expression Sheet-1.png",
        "Aurelia’s Edenfall Animation Sheet.png",
        "Aurelia’s Edenfall Expression Gallery.png",
        "Aurelia’s Edenfall Expression Sheet.png",
        "Aurelia’s Edenfall Talk Animation Sheet.png",
        "Aurelia’s Purple-Gold CG Showcase.png",
        "Aurelia’s Shy Expression Board-2.png"
    ]

    manifest = {
        "pipeline_version": "3.6.2",
        "character": "Aurelia",
        "provenance": "Original User Reference Sheets",
        "canonical_spec": {
            "race": "Half-Elf (Bán tiên)",
            "age": 24,
            "hair": "Platinum-blonde with soft pink highlights",
            "eyes": "Violet/purple",
            "ornaments": "White lily hair ornament",
            "ears": "Pointed elf ears",
            "clothing": "Royal white, purple and gold gown with violet jewel choker",
            "accessories": "Gold leg ring/thigh strap, gold bracelets, choker"
        },
        "sources": []
    }

    actual_files = os.listdir(downloads_dir)
    for expected in source_files:
        norm_expected = expected.lower().replace("’", "'").replace(" ", "")
        match = None
        for actual in actual_files:
            if actual.lower().replace("’", "'").replace(" ", "") == norm_expected:
                match = actual
                break
        
        if not match:
            print(f"Warning: Missing source sheet '{expected}'")
            continue

        src_path = os.path.join(downloads_dir, match)
        dst_path = os.path.join(sources_dir, match)
        
        # Copy to sources folder
        shutil.copy2(src_path, dst_path)

        file_size = os.path.getsize(dst_path)
        sha256 = get_file_hash(dst_path)

        with Image.open(dst_path) as img:
            width, height = img.size
            mode = img.mode
            fmt = img.format
            has_alpha = mode in ("RGBA", "LA") or (mode == "P" and "transparency" in img.info)

        print(f"Indexed: {match} [{width}x{height}, {mode}, {file_size/1024/1024:.2f} MB]")

        manifest["sources"].append({
            "filename": match,
            "relative_path": os.path.relpath(dst_path, os.getcwd()).replace("\\", "/"),
            "absolute_path": dst_path.replace("\\", "/"),
            "width": width,
            "height": height,
            "format": fmt,
            "color_mode": mode,
            "has_alpha": has_alpha,
            "file_size_bytes": file_size,
            "sha256": sha256
        })

    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    print(f"Successfully generated source manifest with {len(manifest['sources'])} sheets at:\n{manifest_path}")

if __name__ == "__main__":
    main()
