"""
Step 2: Automated Art Extraction Pipeline
Extracts panels, characters, expression portraits, talk frames, CGs, and rig layers from source sheets.
Generates extraction_manifest.json with exact coordinates and source provenance.
"""
import os
import json
import cv2
import numpy as np
from PIL import Image

def inpaint_label_area(img_pil, rect):
    """Cleanly inpaint small text label bars using Telea algorithm."""
    arr = np.array(img_pil)
    bgr = cv2.cvtColor(arr, cv2.COLOR_RGB2BGR) if arr.ndim == 3 and arr.shape[2] == 3 else arr
    h, w = bgr.shape[:2]
    mask = np.zeros((h, w), dtype=np.uint8)
    x1, y1, x2, y2 = rect
    x1, y1 = max(0, x1), max(0, y1)
    x2, y2 = min(w, x2), min(h, y2)
    mask[y1:y2, x1:x2] = 255
    kernel = np.ones((3, 3), np.uint8)
    mask = cv2.dilate(mask, kernel, iterations=1)
    inpainted = cv2.inpaint(bgr, mask, inpaintRadius=5, flags=cv2.INPAINT_TELEA)
    rgb = cv2.cvtColor(inpainted, cv2.COLOR_BGR2RGB)
    return Image.fromarray(rgb)

def main():
    sources_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Sources")
    extracted_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Extracted")
    manifest_path = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\extraction_manifest.json")
    os.makedirs(extracted_dir, exist_ok=True)

    print(f"Executing Step 2: Automated Panel Extraction from: {sources_dir}")

    manifest = {
        "pipeline_version": "3.6.2",
        "extracted_assets": []
    }

    # Load Source Sheets
    src_talk_sheet = os.path.join(sources_dir, "Aurelia’s Edenfall Talk Animation Sheet.png")
    src_exp_sheet = os.path.join(sources_dir, "Aurelia’s Edenfall Expression Sheet.png")
    src_cg_sheet = os.path.join(sources_dir, "Aurelia’s Purple-Gold CG Showcase.png")
    src_heroine_sheet = os.path.join(sources_dir, "Aurelia_ Edenfall Heroine Design Sheet.png")
    src_board_sheet = os.path.join(sources_dir, "Aurelia_ Edenfall Character Design Board.png")

    img_talk = Image.open(src_talk_sheet).convert("RGB")
    img_exp = Image.open(src_exp_sheet).convert("RGB")
    img_cg = Image.open(src_cg_sheet).convert("RGB")
    img_heroine = Image.open(src_heroine_sheet).convert("RGB")
    img_board = Image.open(src_board_sheet).convert("RGB")

    def save_crop(category, name, crop_img, src_name, coords):
        cat_dir = os.path.join(extracted_dir, category)
        os.makedirs(cat_dir, exist_ok=True)
        out_path = os.path.join(cat_dir, f"{name}.png")
        crop_img.save(out_path, format="PNG")
        print(f"Extracted [{category}]: {name}.png ({crop_img.size[0]}x{crop_img.size[1]})")
        manifest["extracted_assets"].append({
            "category": category,
            "name": name,
            "filename": f"{name}.png",
            "source_sheet": src_name,
            "crop_coords_xyxy": coords,
            "width": crop_img.size[0],
            "height": crop_img.size[1],
            "relative_path": os.path.relpath(out_path, os.getcwd()).replace("\\", "/")
        })

    # 1. Master Half-Body (Canonical VN Dialogue Staging)
    # Crop from Expression Sheet (Waist up, affectionate gaze, hand at collarbone)
    hb_coords = (0, 0, 520, 725)
    hb_crop = img_exp.crop(hb_coords)
    save_crop("Masters", "Aurelia_HalfBody_Master", hb_crop, "Aurelia’s Edenfall Expression Sheet.png", hb_coords)

    # Secondary Half-Body (Reaching Pose)
    hb_reach_coords = (0, 0, 550, 680)
    hb_reach_crop = img_talk.crop(hb_reach_coords)
    save_crop("Masters", "Aurelia_HalfBody_Reaching", hb_reach_crop, "Aurelia’s Edenfall Talk Animation Sheet.png", hb_reach_coords)

    # 2. Master Full-Body (Standing with Golden Heels & Veil)
    fb_coords = (0, 0, 420, 1024)
    fb_crop = img_board.crop(fb_coords)
    save_crop("Masters", "Aurelia_FullBody_Master", fb_crop, "Aurelia_ Edenfall Character Design Board.png", fb_coords)

    # 3. 16 Canonical Expression Portraits (Grid inside Expression Set)
    exp_names_r1 = [
        "Portrait_Normal", "Portrait_Smile", "Portrait_Happy", "Portrait_Excited",
        "Portrait_Surprised", "Portrait_Confused", "Portrait_Thinking", "Portrait_Concerned"
    ]
    exp_names_r2 = [
        "Portrait_Shy", "Portrait_Blush", "Portrait_Teasing", "Portrait_Seductive",
        "Portrait_Sad", "Portrait_Angry", "Portrait_Hurt", "Portrait_Satisfied"
    ]
    
    # Row 1 (y: 355 to 475)
    for col, name in enumerate(exp_names_r1):
        x = int(525 + col * 123)
        coords = (x, 355, x + 115, 475)
        p_crop = img_exp.crop(coords)
        save_crop("Expressions", name, p_crop, "Aurelia’s Edenfall Expression Sheet.png", coords)

    # Row 2 (y: 525 to 645)
    for col, name in enumerate(exp_names_r2):
        x = int(525 + col * 123)
        coords = (x, 525, x + 115, 645)
        p_crop = img_exp.crop(coords)
        save_crop("Expressions", name, p_crop, "Aurelia’s Edenfall Expression Sheet.png", coords)

    # 4. 5 Talk Animation Frames (0.0s to 3.0s Speech Cycle)
    talk_names = [
        "Aurelia_Talk_00", "Aurelia_Talk_01", "Aurelia_Talk_02", "Aurelia_Talk_03", "Aurelia_Talk_04"
    ]
    for i, name in enumerate(talk_names):
        x = int(645 + i * 175)
        coords = (x, 50, x + 160, 240)
        t_crop = img_exp.crop(coords)
        save_crop("TalkFrames", name, t_crop, "Aurelia’s Edenfall Expression Sheet.png", coords)

    # 5. 7 Narrative Story CG Scenes
    cg_definitions = [
        ("CG_01_FirstMeeting", (325, 10, 985, 305), (0, 260, 240, 295)),
        ("CG_02_PrivateTalk", (1005, 10, 1665, 305), (0, 260, 250, 295)),
        ("CG_03_Relax", (325, 355, 985, 635), (0, 245, 230, 280)),
        ("CG_04_Caring", (1005, 355, 1665, 635), (0, 245, 220, 280)),
        ("CG_05_Romantic", (325, 685, 765, 935), (0, 215, 200, 250)),
        ("CG_06_RestTogether", (780, 685, 1220, 935), (0, 215, 240, 250)),
        ("CG_07_NightScene", (1235, 685, 1665, 935), (0, 215, 230, 250))
    ]

    for name, coords, label_rect in cg_definitions:
        cg_crop = img_cg.crop(coords)
        cg_clean = inpaint_label_area(cg_crop, label_rect)
        save_crop("CG", name, cg_clean, "Aurelia’s Purple-Gold CG Showcase.png", coords)

    # 6. Live2D Rig Layers (from Heroine Design Sheet)
    rig_layers = [
        ("Layer_Body_Base", (975, 690, 1140, 1010)),
        ("Layer_Eyes_Mouth", (1100, 700, 1185, 800)),
        ("Layer_Hair_Front", (1160, 690, 1335, 970)),
        ("Layer_Hair_Braid", (1280, 700, 1355, 890)),
        ("Layer_Ribbon_Lily", (1345, 690, 1390, 835)),
        ("Layer_Hair_Back", (1360, 700, 1450, 935)),
        ("Layer_Sleeve_Ribbon", (1460, 700, 1500, 825)),
        ("Layer_Cape_Train", (1360, 810, 1515, 1010)),
        ("Layer_Skirt_Front", (1170, 875, 1350, 1010))
    ]

    for name, coords in rig_layers:
        r_crop = img_heroine.crop(coords)
        save_crop("RigLayers", name, r_crop, "Aurelia_ Edenfall Heroine Design Sheet.png", coords)

    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    print(f"\nStep 2 completed successfully. Total panels extracted: {len(manifest['extracted_assets'])}")
    print(f"Extraction manifest written to: {manifest_path}")

if __name__ == "__main__":
    main()
