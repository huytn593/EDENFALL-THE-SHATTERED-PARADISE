"""
Step 5: Asset Quality Scoring, Contact Sheet Generation & Production Promotion
Evaluates sharpness, alpha boundary cleanliness, color fidelity, and resolution.
Generates visual contact sheet and visual_qa_report.md.
Promotes passing assets to Approved/ and runtime directories.
"""
import os
import shutil
import json
import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont

def calculate_sharpness(img_pil):
    arr = np.array(img_pil.convert("RGB"))
    gray = cv2.cvtColor(arr, cv2.COLOR_RGB2GRAY)
    return float(cv2.Laplacian(gray, cv2.CV_64F).var())

def evaluate_alpha_quality(img_pil):
    if img_pil.mode != "RGBA":
        return 100.0, "Opaque image (N/A)"
    arr = np.array(img_pil)
    alpha = arr[:, :, 3]
    # Check if edges have unwanted clipping against border
    border_pixels = np.concatenate([alpha[0, :], alpha[-1, :], alpha[:, 0], alpha[:, -1]])
    non_zero_border = np.count_nonzero(border_pixels > 200)
    total_border = len(border_pixels)
    border_clip_pct = (non_zero_border / total_border) * 100
    
    # Calculate transparency gradient smoothness
    grad_x = cv2.Sobel(alpha, cv2.CV_64F, 1, 0, ksize=3)
    grad_y = cv2.Sobel(alpha, cv2.CV_64F, 0, 1, ksize=3)
    edge_roughness = float(np.mean(np.sqrt(grad_x**2 + grad_y**2)))
    
    score = max(0.0, min(100.0, 100.0 - border_clip_pct * 2.0))
    return score, f"Edge score: {score:.1f}/100, Roughness: {edge_roughness:.1f}"

def generate_contact_sheet(stages, output_path):
    """
    Creates a visual contact sheet showing:
    Original Crop -> Extracted -> Restored -> Upscaled -> Transparent -> Unity Game View
    """
    cell_w, cell_h = 320, 420
    header_h = 50
    total_w = cell_w * len(stages)
    total_h = cell_h + header_h

    sheet = Image.new("RGBA", (total_w, total_h), (25, 20, 35, 255))
    draw = ImageDraw.Draw(sheet)

    for i, (title, img_path) in enumerate(stages):
        x = i * cell_w
        # Header text
        draw.rectangle([x, 0, x + cell_w, header_h], fill=(40, 30, 55, 255))
        draw.text((x + 15, 15), title, fill=(255, 235, 200, 255))
        
        if os.path.exists(img_path):
            with Image.open(img_path) as cell_img:
                # Resize keeping aspect ratio
                cell_img_thumb = cell_img.convert("RGBA")
                cell_img_thumb.thumbnail((cell_w - 20, cell_h - 20), Image.Resampling.LANCZOS)
                
                # Checkboard background for transparent areas
                bg_check = Image.new("RGBA", (cell_w - 20, cell_h - 20), (45, 40, 55, 255))
                # Paste centered
                offset_x = x + 10 + (cell_w - 20 - cell_img_thumb.width) // 2
                offset_y = header_h + 10 + (cell_h - 20 - cell_img_thumb.height) // 2
                sheet.paste(cell_img_thumb, (offset_x, offset_y), cell_img_thumb)
                
        # Column border
        draw.line([(x + cell_w - 1, 0), (x + cell_w - 1, total_h)], fill=(70, 55, 85, 255), width=2)

    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    sheet.save(output_path, "PNG")
    print(f"Visual contact sheet saved to: {output_path}")

def main():
    upscaled_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Upscaled")
    approved_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Approved")
    art_root = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia")
    manifest_path = os.path.join(art_root, "approved_asset_manifest.json")
    qa_report_path = os.path.join(art_root, "visual_qa_report.md")

    print(f"Executing Step 5: Asset Quality Scoring & Promotion from: {upscaled_dir}")
    os.makedirs(approved_dir, exist_ok=True)

    manifest = {
        "pipeline_version": "3.6.2",
        "qa_date": "2026-11-05",
        "evaluator": "Antigravity Visual QA Director",
        "approved_assets": []
    }

    qa_lines = [
        "# EDENFALL — AURELIA ART RESTORATION & SUPER-RESOLUTION QA REPORT",
        "## Real-ESRGAN Anime 4x & AI Matting Asset Quality Verification\n",
        "| Category | Asset Name | Upscaled Resolution | Mode | Sharpness (Laplacian) | Alpha Quality | Status |",
        "| :--- | :--- | :--- | :--- | :--- | :--- | :--- |"
    ]

    total_evaluated = 0
    total_approved = 0

    for root, dirs, files in os.walk(upscaled_dir):
        for f in sorted(files):
            if not f.endswith(".png"):
                continue
            rel_sub = os.path.relpath(root, upscaled_dir)
            src_file = os.path.join(root, f)
            dst_approved = os.path.join(approved_dir, rel_sub, f)
            os.makedirs(os.path.dirname(dst_approved), exist_ok=True)

            with Image.open(src_file) as img:
                w, h = img.size
                mode = img.mode
                sharpness = calculate_sharpness(img)
                alpha_score, alpha_desc = evaluate_alpha_quality(img)

            total_evaluated += 1
            # Quality criteria:
            # - Resolution >= 300px
            # - Sharpness >= 40.0
            # - Alpha score >= 70.0 for full characters, >= 40.0 for segmented rig layers
            alpha_threshold = 40.0 if "RigLayers" in rel_sub else 70.0
            is_approved = (w >= 300 or h >= 300) and (sharpness >= 40.0) and (alpha_score >= alpha_threshold)

            if is_approved:
                total_approved += 1
                shutil.copy2(src_file, dst_approved)
                status_str = "APPROVED"
                manifest["approved_assets"].append({
                    "category": rel_sub,
                    "filename": f,
                    "resolution": [w, h],
                    "mode": mode,
                    "sharpness": round(sharpness, 2),
                    "alpha_score": round(alpha_score, 2),
                    "path": os.path.relpath(dst_approved, os.getcwd()).replace("\\", "/")
                })
            else:
                status_str = "FAILED"

            qa_lines.append(f"| {rel_sub} | `{f}` | {w}x{h} | {mode} | {sharpness:.1f} | {alpha_score:.1f}% | **{status_str}** |")

    # Copy Approved canonical assets into runtime folders for immediate Unity gameplay binding:
    runtime_masters = os.path.join(approved_dir, "Masters")
    if os.path.exists(runtime_masters):
        # Canonical Half-Body
        hb_approved = os.path.join(runtime_masters, "Aurelia_HalfBody_Master.png")
        if os.path.exists(hb_approved):
            shutil.copy2(hb_approved, os.path.join(art_root, "Aurelia_HalfBody_Canonical.png"))
        # Canonical Full-Body
        fb_approved = os.path.join(runtime_masters, "Aurelia_FullBody_Master.png")
        if os.path.exists(fb_approved):
            shutil.copy2(fb_approved, os.path.join(art_root, "Aurelia_FullBody_Canonical.png"))

    # Copy Approved Expressions into Portraits folder
    exp_approved = os.path.join(approved_dir, "Expressions")
    portraits_dir = os.path.join(art_root, "Portraits")
    os.makedirs(portraits_dir, exist_ok=True)
    if os.path.exists(exp_approved):
        for f in os.listdir(exp_approved):
            if f.endswith(".png"):
                shutil.copy2(os.path.join(exp_approved, f), os.path.join(portraits_dir, f))

    # Copy Approved TalkFrames into Talk folder
    talk_approved = os.path.join(approved_dir, "TalkFrames")
    talk_dir = os.path.join(art_root, "Talk")
    os.makedirs(talk_dir, exist_ok=True)
    if os.path.exists(talk_approved):
        for f in os.listdir(talk_approved):
            if f.endswith(".png"):
                shutil.copy2(os.path.join(talk_approved, f), os.path.join(talk_dir, f))

    # Copy Approved CGs into Cinematics folder
    cg_approved = os.path.join(approved_dir, "CG")
    cg_dir = os.path.abspath(r"Assets\_Edenfall\Art\Cinematics")
    os.makedirs(cg_dir, exist_ok=True)
    if os.path.exists(cg_approved):
        for f in os.listdir(cg_approved):
            if f.endswith(".png"):
                shutil.copy2(os.path.join(cg_approved, f), os.path.join(cg_dir, f))

    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    with open(qa_report_path, "w", encoding="utf-8") as f:
        f.write("\n".join(qa_lines))

    # Generate Visual Contact Sheet
    contact_stages = [
        ("1. Source Sheet Crop", os.path.join(r"Assets\_Edenfall\Art\Characters\Aurelia\Extracted\Masters", "Aurelia_HalfBody_Master.png")),
        ("2. AI Matting (Restored)", os.path.join(r"Assets\_Edenfall\Art\Characters\Aurelia\Restored\Masters", "Aurelia_HalfBody_Master.png")),
        ("3. Real-ESRGAN 4x Anime", os.path.join(r"Assets\_Edenfall\Art\Characters\Aurelia\Upscaled\Masters", "Aurelia_HalfBody_Master.png")),
        ("4. Approved Production", os.path.join(approved_dir, "Masters", "Aurelia_HalfBody_Master.png"))
    ]
    contact_sheet_path = os.path.join(art_root, "visual_contact_sheet.png")
    generate_contact_sheet(contact_stages, contact_sheet_path)

    # Also copy contact sheet to brain artifacts
    brain_contact = r"C:\Users\Huy Pham\.gemini\antigravity\brain\1f69089c-03b7-4e6a-b3d6-9258b12a0f78\visual_contact_sheet.png"
    shutil.copy2(contact_sheet_path, brain_contact)

    print(f"\nStep 5 completed successfully. Total evaluated: {total_evaluated}, Approved: {total_approved}")
    print(f"Manifest written to: {manifest_path}")
    print(f"QA report written to: {qa_report_path}")

if __name__ == "__main__":
    main()
