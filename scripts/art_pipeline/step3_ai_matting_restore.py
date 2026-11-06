"""
Step 3: AI Background Removal & Restoration Pipeline
Applies state-of-the-art AI matting (BRIA RMBG 2.0) with edge defringing and alpha speckle cleanup.
Restores clean transparent cutouts for character masters and rig layers.
"""
import os
import shutil
import rembg
from PIL import Image
from matting_utils import defringe_rgb_edges

def process_matting(src_path, dst_path, session=None):
    print(f"Processing AI Matting: {os.path.basename(src_path)}...")
    src_img = Image.open(src_path).convert("RGB")
    # Apply AI background removal
    raw_rgba = rembg.remove(src_img, session=session)
    # Apply defringing and alpha cleanup
    clean_rgba = defringe_rgb_edges(raw_rgba)
    os.makedirs(os.path.dirname(dst_path), exist_ok=True)
    clean_rgba.save(dst_path, format="PNG")
    print(f"  -> Saved clean cutout: {dst_path} ({clean_rgba.size[0]}x{clean_rgba.size[1]})")

def main():
    extracted_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Extracted")
    restored_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Restored")
    os.makedirs(restored_dir, exist_ok=True)

    print(f"Executing Step 3: AI Matting & Restoration from: {extracted_dir}")

    # Use default BRIA RMBG 2.0 session
    session = rembg.new_session()

    # 1. Matting on Character Masters
    masters = [
        "Aurelia_HalfBody_Master.png",
        "Aurelia_HalfBody_Reaching.png",
        "Aurelia_FullBody_Master.png"
    ]
    for m in masters:
        src = os.path.join(extracted_dir, "Masters", m)
        dst = os.path.join(restored_dir, "Masters", m)
        if os.path.exists(src):
            process_matting(src, dst, session=session)

    # 2. Matting on Rig Layers
    rig_dir = os.path.join(extracted_dir, "RigLayers")
    if os.path.exists(rig_dir):
        for f in os.listdir(rig_dir):
            if f.endswith(".png"):
                src = os.path.join(rig_dir, f)
                dst = os.path.join(restored_dir, "RigLayers", f)
                process_matting(src, dst, session=session)

    # 3. Copy CGs, Expressions and TalkFrames (clean compositions)
    for cat in ["CG", "Expressions", "TalkFrames"]:
        src_cat = os.path.join(extracted_dir, cat)
        dst_cat = os.path.join(restored_dir, cat)
        os.makedirs(dst_cat, exist_ok=True)
        if os.path.exists(src_cat):
            for f in os.listdir(src_cat):
                if f.endswith(".png"):
                    shutil.copy2(os.path.join(src_cat, f), os.path.join(dst_cat, f))
            print(f"Copied {len(os.listdir(src_cat))} assets for category: {cat}")

    print("\nStep 3 completed successfully. All cutouts and restored assets saved to:")
    print(restored_dir)

if __name__ == "__main__":
    main()
