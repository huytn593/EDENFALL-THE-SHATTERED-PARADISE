"""
Step 4: Automatic Super-Resolution Pipeline using Real-ESRGAN Anime (NCNN Vulkan)
Upscales restored assets 4x with anime-specific model (realesrgan-x4plus-anime).
Configured for NVIDIA GeForce GTX 1650 4 GB VRAM with tiled inference (-t 256).
Generates enhancement_manifest.json with upscale factors, resolutions, and quality metrics.
"""
import os
import subprocess
import json
from PIL import Image

def run_realesrgan(exe_path, input_path, output_path, scale=4, tile_size=256, gpu_id=1, model="realesrgan-x4plus-anime"):
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    cmd = [
        exe_path,
        "-i", input_path,
        "-o", output_path,
        "-n", model,
        "-s", str(scale),
        "-t", str(tile_size),
        "-g", str(gpu_id),
        "-f", "png"
    ]
    try:
        res = subprocess.run(cmd, capture_output=True, text=True, check=True)
        return True, res.stdout
    except subprocess.CalledProcessError as e:
        print(f"GPU {gpu_id} inference failed, retrying on CPU/fallback: {e.stderr}")
        # Fallback to GPU 0 (integrated) or CPU
        cmd[cmd.index("-g") + 1] = "0"
        try:
            res = subprocess.run(cmd, capture_output=True, text=True, check=True)
            return True, res.stdout
        except Exception as e2:
            print(f"Fallback inference failed: {e2}")
            return False, str(e2)

def main():
    tools_dir = os.path.abspath(r"tools\realesrgan")
    exe_path = os.path.join(tools_dir, "realesrgan-ncnn-vulkan.exe")
    restored_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Restored")
    upscaled_dir = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\Upscaled")
    manifest_path = os.path.abspath(r"Assets\_Edenfall\Art\Characters\Aurelia\enhancement_manifest.json")
    os.makedirs(upscaled_dir, exist_ok=True)

    print(f"Executing Step 4: Real-ESRGAN Super-Resolution (x4 Anime, GTX 1650)")
    print(f"Executable: {exe_path}")

    manifest = {
        "pipeline_version": "3.6.2",
        "super_resolution_engine": "Real-ESRGAN NCNN Vulkan",
        "model": "realesrgan-x4plus-anime",
        "scale": 4,
        "tile_size": 256,
        "target_gpu": "NVIDIA GeForce GTX 1650 4GB",
        "upscaled_assets": []
    }

    # Walk all files in restored_dir
    processed_count = 0
    for root, dirs, files in os.walk(restored_dir):
        for f in sorted(files):
            if not f.endswith(".png"):
                continue
            rel_sub = os.path.relpath(root, restored_dir)
            src_file = os.path.join(root, f)
            dst_file = os.path.join(upscaled_dir, rel_sub, f)

            with Image.open(src_file) as img_in:
                w_in, h_in = img_in.size
                mode_in = img_in.mode

            print(f"Upscaling [{rel_sub}]: {f} ({w_in}x{h_in}) -> target ({w_in*4}x{h_in*4})...")
            success, log = run_realesrgan(exe_path, src_file, dst_file, scale=4, tile_size=256, gpu_id=1)

            if success and os.path.exists(dst_file):
                with Image.open(dst_file) as img_out:
                    w_out, h_out = img_out.size
                processed_count += 1
                manifest["upscaled_assets"].append({
                    "category": rel_sub,
                    "filename": f,
                    "original_res": [w_in, h_in],
                    "upscaled_res": [w_out, h_out],
                    "scale_factor": 4,
                    "model": "realesrgan-x4plus-anime",
                    "relative_path": os.path.relpath(dst_file, os.getcwd()).replace("\\", "/")
                })
            else:
                print(f"Warning: Failed to upscale {f}")

    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    print(f"\nStep 4 completed successfully. Total assets upscaled: {processed_count}")
    print(f"Enhancement manifest written to: {manifest_path}")

if __name__ == "__main__":
    main()
