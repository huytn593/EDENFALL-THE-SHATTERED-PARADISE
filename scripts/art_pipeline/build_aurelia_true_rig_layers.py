"""
build_aurelia_true_rig_layers.py
Phase 3.6.3: Autonomous Art Layer Extraction, Inpainting & Rig Registration Pipeline.
Extracts, inpaints, and registers all independently controllable character layers for Aurelia
on a unified 2080x2900 canvas with registered local offsets and bone definitions.
"""
import os
import cv2
import json
import numpy as np
from PIL import Image, ImageFilter, ImageDraw

def create_feathered_mask(size, ellipse_center, ellipse_axes, angle=0, blur_radius=5):
    """Generate a smooth feathered alpha mask for seamless layer extraction."""
    mask = np.zeros(size[::-1], dtype=np.uint8)
    cv2.ellipse(mask, ellipse_center, ellipse_axes, angle, 0, 360, 255, -1)
    mask_pil = Image.fromarray(mask)
    if blur_radius > 0:
        mask_pil = mask_pil.filter(ImageFilter.GaussianBlur(blur_radius))
    return np.array(mask_pil, dtype=np.float32) / 255.0

def inpaint_region(img_np, mask_uint8, radius=7):
    """Inpaint masked region seamlessly using OpenCV Telea algorithm."""
    bgr = cv2.cvtColor(img_np, cv2.COLOR_RGBA2BGR) if img_np.shape[2] == 4 else img_np
    inpainted = cv2.inpaint(bgr, mask_uint8, radius, cv2.INPAINT_TELEA)
    rgb = cv2.cvtColor(inpainted, cv2.COLOR_BGR2RGB)
    out = np.zeros_like(img_np)
    out[:, :, :3] = rgb
    out[:, :, 3] = img_np[:, :, 3]
    return out

def main():
    repo_root = os.path.abspath(".")
    master_path = os.path.join(repo_root, r"Assets\_Edenfall\Art\Characters\Aurelia\Aurelia_HalfBody_Canonical.png")
    out_rig_unity = os.path.join(repo_root, r"Assets\_Edenfall\Art\Characters\Aurelia\RigLayers")
    out_rig_deliverable = os.path.join(repo_root, "cleaned_character_layers")
    os.makedirs(out_rig_unity, exist_ok=True)
    os.makedirs(out_rig_deliverable, exist_ok=True)

    print(f"Loading Master Artwork: {master_path}")
    master_pil = Image.open(master_path).convert("RGBA")
    W, H = master_pil.size
    print(f"Master Canvas Size: {W} x {H}")
    master_np = np.array(master_pil)

    # Coordinates calibrated from high-res canonical master
    # Head crop root: (750, 350)
    # Left Eye: Center (915, 735), Box (840, 680, 1000, 790)
    # Right Eye: Center (1248, 592), Box (1170, 540, 1330, 650)
    # Mouth: Center (1190, 895), Box (1110, 850, 1270, 940)
    # Eyebrows: Left (870, 660, 990, 710), Right (1160, 510, 1280, 560)
    # Lily Ornament: (700, 250, 950, 550)
    # Front Bangs: (850, 300, 1400, 750)
    # Hair Side Left (Braid): (700, 500, 960, 1400)
    # Hair Side Right (Lock): (1350, 500, 1680, 1450)
    # Hair Back: behind shoulders (500, 200, 1850, 2100)
    # Torso / Dress: (300, 900, 1800, 2900)
    # Bust / Chest: (850, 1050, 1550, 1650)
    # Right Arm: (1100, 950, 1500, 1450)

    layers = {}

    def save_layer(name, img_data, order_in_layer, z_depth, parent_bone):
        """Save layer to both Unity directory and deliverable directory, and register in manifest."""
        pil_img = Image.fromarray(img_data) if isinstance(img_data, np.ndarray) else img_data
        p_unity = os.path.join(out_rig_unity, f"{name}.png")
        p_deliv = os.path.join(out_rig_deliverable, f"{name}.png")
        pil_img.save(p_unity, format="PNG")
        pil_img.save(p_deliv, format="PNG")
        print(f"Extracted Rig Layer: {name}.png ({pil_img.size[0]}x{pil_img.size[1]}) [Order={order_in_layer}, Z={z_depth}]")
        layers[name] = {
            "filename": f"{name}.png",
            "width": pil_img.size[0],
            "height": pil_img.size[1],
            "order_in_layer": order_in_layer,
            "z_depth": z_depth,
            "parent_bone": parent_bone
        }

    # =========================================================================
    # 1. LAYER 00: HAIR BACK (Flowing long hair behind shoulders & neck)
    # =========================================================================
    mask_hair_back = np.zeros((H, W), dtype=np.uint8)
    # Polygons covering back hair behind character silhouette
    cv2.ellipse(mask_hair_back, (1080, 1100), (600, 900), 0, 0, 360, 255, -1)
    # Cut out the central face and body
    body_core_cutout = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(body_core_cutout, (1120, 1400), (380, 800), 0, 0, 360, 255, -1)
    mask_hair_back = cv2.subtract(mask_hair_back, body_core_cutout)
    f_mask_hb = create_feathered_mask((W, H), (1080, 1100), (590, 890), blur_radius=7)
    
    hair_back_np = master_np.copy()
    hair_back_np[:, :, 3] = (hair_back_np[:, :, 3].astype(np.float32) * (mask_hair_back.astype(np.float32)/255.0) * f_mask_hb).astype(np.uint8)
    save_layer("Layer_00_Hair_Back", hair_back_np, 5, 0.20, "Bone_HairBack")

    # =========================================================================
    # 2. LAYER 01: BODY TORSO (Gown, neckline, shoulders, corset, sleeves)
    # Inpainted behind the right hand so hand movement leaves no hole!
    # =========================================================================
    torso_np = master_np.copy()
    mask_torso = np.zeros((H, W), dtype=np.uint8)
    # Torso starts below jaw (y > 880)
    cv2.rectangle(mask_torso, (0, 880), (W, H), 255, -1)
    # Inpaint mask for right hand region (x: 1100..1450, y: 1000..1350)
    hand_inpaint_mask = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(hand_inpaint_mask, (1260, 1180), (140, 110), -20, 0, 360, 255, -1)
    torso_inpainted = inpaint_region(torso_np, hand_inpaint_mask, radius=9)
    torso_inpainted[:, :, 3] = (torso_inpainted[:, :, 3].astype(np.float32) * (mask_torso.astype(np.float32)/255.0)).astype(np.uint8)
    save_layer("Layer_01_Body_Torso", torso_inpainted, 10, 0.10, "Bone_Torso")

    # =========================================================================
    # 3. LAYER 02: HEAD & FACE BASE (Neck, chin, ears, blushing cheeks, nose)
    # INPAINTED EYES & MOUTH: perfectly clean skin so eyes & mouth can animate
    # on top with ZERO ghosting or double-feature artifacts!
    # =========================================================================
    face_base_np = master_np.copy()
    mask_face_bounds = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_face_bounds, (1120, 750), (450, 420), 0, 0, 360, 255, -1)
    
    # Inpaint masks for eye sockets and mouth
    inpaint_face_mask = np.zeros((H, W), dtype=np.uint8)
    # Left eye socket
    cv2.ellipse(inpaint_face_mask, (905, 735), (95, 62), -15, 0, 360, 255, -1)
    # Right eye socket
    cv2.ellipse(inpaint_face_mask, (1248, 592), (92, 60), -15, 0, 360, 255, -1)
    # Mouth region
    cv2.ellipse(inpaint_face_mask, (1190, 895), (75, 45), -15, 0, 360, 255, -1)

    face_base_inpainted = inpaint_region(face_base_np, inpaint_face_mask, radius=8)
    face_base_inpainted[:, :, 3] = (face_base_inpainted[:, :, 3].astype(np.float32) * (mask_face_bounds.astype(np.float32)/255.0)).astype(np.uint8)
    save_layer("Layer_02_Head_FaceBase", face_base_inpainted, 20, 0.00, "Bone_Head")

    # =========================================================================
    # 4. LAYER 03: EYES LEFT (Viewer's Left Eye)
    # Open, Closed (Blink), Half-Closed, and Separated Iris
    # =========================================================================
    # A. Left Eye Open
    eye_l_open = np.zeros((H, W, 4), dtype=np.uint8)
    mask_el = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_el, (905, 735), (92, 58), -15, 0, 360, 255, -1)
    f_el = create_feathered_mask((W, H), (905, 735), (88, 55), angle=-15, blur_radius=3)
    eye_l_open[:, :, :3] = master_np[:, :, :3]
    eye_l_open[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_el.astype(np.float32)/255.0) * f_el).astype(np.uint8)
    save_layer("Layer_03_EyeL_Open", eye_l_open, 25, -0.02, "Bone_EyeL")

    # B. Left Eye Closed (Blink frame: clean skin patch + curved downward anime eyelashes)
    eye_l_blink = face_base_inpainted.copy()
    eye_l_blink[:, :, 3] = (eye_l_blink[:, :, 3].astype(np.float32) * (mask_el.astype(np.float32)/255.0) * f_el).astype(np.uint8)
    pil_blink_l = Image.fromarray(eye_l_blink)
    draw_bl = ImageDraw.Draw(pil_blink_l)
    lash_pts_l = [
        (830, 742), (855, 755), (885, 764), (915, 766), (945, 762), (972, 752), (990, 738)
    ]
    draw_bl.line(lash_pts_l, fill=(55, 25, 48, 255), width=8, joint="curve")
    fold_pts_l = [(850, 742), (890, 748), (930, 747), (965, 738)]
    draw_bl.line(fold_pts_l, fill=(195, 115, 130, 200), width=4, joint="curve")
    save_layer("Layer_03_EyeL_Blink", np.array(pil_blink_l), 25, -0.02, "Bone_EyeL")

    # C. Left Iris (for gaze tracking)
    eye_l_iris = np.zeros((H, W, 4), dtype=np.uint8)
    mask_il = np.zeros((H, W), dtype=np.uint8)
    cv2.circle(mask_il, (905, 740), 38, 255, -1)
    f_il = create_feathered_mask((W, H), (905, 740), (36, 36), blur_radius=2)
    eye_l_iris[:, :, :3] = master_np[:, :, :3]
    eye_l_iris[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_il.astype(np.float32)/255.0) * f_il).astype(np.uint8)
    save_layer("Layer_03_IrisL", eye_l_iris, 26, -0.025, "Bone_IrisL")

    # =========================================================================
    # 5. LAYER 04: EYES RIGHT (Viewer's Right Eye)
    # Open, Closed (Blink), and Separated Iris
    # =========================================================================
    # A. Right Eye Open
    eye_r_open = np.zeros((H, W, 4), dtype=np.uint8)
    mask_er = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_er, (1248, 592), (85, 55), -15, 0, 360, 255, -1)
    f_er = create_feathered_mask((W, H), (1248, 592), (82, 52), angle=-15, blur_radius=3)
    eye_r_open[:, :, :3] = master_np[:, :, :3]
    eye_r_open[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_er.astype(np.float32)/255.0) * f_er).astype(np.uint8)
    save_layer("Layer_04_EyeR_Open", eye_r_open, 25, -0.02, "Bone_EyeR")

    # B. Right Eye Closed (Blink frame: clean skin patch + curved eyelashes)
    eye_r_blink = face_base_inpainted.copy()
    eye_r_blink[:, :, 3] = (eye_r_blink[:, :, 3].astype(np.float32) * (mask_er.astype(np.float32)/255.0) * f_er).astype(np.uint8)
    pil_blink_r = Image.fromarray(eye_r_blink)
    draw_br = ImageDraw.Draw(pil_blink_r)
    lash_pts_r = [
        (1175, 595), (1198, 612), (1228, 624), (1258, 626), (1288, 618), (1312, 604), (1328, 590)
    ]
    draw_br.line(lash_pts_r, fill=(55, 25, 48, 255), width=8, joint="curve")
    fold_pts_r = [(1190, 596), (1230, 604), (1270, 602), (1305, 590)]
    draw_br.line(fold_pts_r, fill=(195, 115, 130, 200), width=4, joint="curve")
    save_layer("Layer_04_EyeR_Blink", np.array(pil_blink_r), 25, -0.02, "Bone_EyeR")

    # C. Right Iris (for gaze tracking)
    eye_r_iris = np.zeros((H, W, 4), dtype=np.uint8)
    mask_ir = np.zeros((H, W), dtype=np.uint8)
    cv2.circle(mask_ir, (1248, 595), 40, 255, -1)
    f_ir = create_feathered_mask((W, H), (1248, 595), (38, 38), blur_radius=2)
    eye_r_iris[:, :, :3] = master_np[:, :, :3]
    eye_r_iris[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_ir.astype(np.float32)/255.0) * f_ir).astype(np.uint8)
    save_layer("Layer_04_IrisR", eye_r_iris, 26, -0.025, "Bone_IrisR")

    # =========================================================================
    # 6. LAYER 05: EYEBROWS (Left and Right)
    # =========================================================================
    eyebrows_np = np.zeros((H, W, 4), dtype=np.uint8)
    mask_brows = np.zeros((H, W), dtype=np.uint8)
    # Left brow
    cv2.ellipse(mask_brows, (925, 680), (65, 25), -12, 0, 360, 255, -1)
    # Right brow
    cv2.ellipse(mask_brows, (1220, 535), (65, 25), -12, 0, 360, 255, -1)
    eyebrows_np[:, :, :3] = master_np[:, :, :3]
    eyebrows_np[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_brows.astype(np.float32)/255.0)).astype(np.uint8)
    save_layer("Layer_05_Eyebrows", eyebrows_np, 28, -0.03, "Bone_Head")

    # =========================================================================
    # 7. LAYER 06: MOUTH VISEMES (5 Canonical Shapes from Talk Animation Sheet)
    # 0. Closed (Resting)
    # 1. Half-Open (Breath / Talk onset)
    # 2. Open (Speaking)
    # 3. Wide (Accented speaking)
    # 4. Smile (Tender closed smile)
    # =========================================================================
    # Common mouth mask & feathered alpha
    mask_m_common = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_m_common, (1190, 895), (75, 45), -15, 0, 360, 255, -1)
    f_mc = create_feathered_mask((W, H), (1190, 895), (70, 40), angle=-15, blur_radius=3)

    # Base skin patch for visemes
    mouth_skin_base = face_base_inpainted.copy()
    mouth_skin_base[:, :, 3] = (mouth_skin_base[:, :, 3].astype(np.float32) * (mask_m_common.astype(np.float32)/255.0) * f_mc).astype(np.uint8)

    # A. Open Mouth (from Master)
    mouth_open = np.zeros((H, W, 4), dtype=np.uint8)
    mouth_open[:, :, :3] = master_np[:, :, :3]
    mouth_open[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_m_common.astype(np.float32)/255.0) * f_mc).astype(np.uint8)
    save_layer("Layer_06_Mouth_Open", mouth_open, 30, -0.035, "Bone_Mouth")

    # B. Closed Mouth (Delicate resting lips with skin patch)
    mouth_closed = mouth_skin_base.copy()
    pil_mc = Image.fromarray(mouth_closed)
    draw_mc = ImageDraw.Draw(pil_mc)
    lip_pts = [(1135, 902), (1165, 896), (1192, 893), (1220, 895), (1248, 900)]
    draw_mc.line(lip_pts, fill=(165, 45, 65, 245), width=5, joint="curve")
    draw_mc.arc([1155, 895, 1235, 918], 0, 180, fill=(225, 140, 155, 190), width=6)
    save_layer("Layer_06_Mouth_Closed", np.array(pil_mc), 30, -0.035, "Bone_Mouth")

    # C. Half-Open Mouth
    mouth_half = mouth_skin_base.copy()
    pil_mh = Image.fromarray(mouth_half)
    draw_mh = ImageDraw.Draw(pil_mh)
    draw_mh.line([(1140, 900), (1170, 894), (1195, 892), (1222, 894), (1245, 898)], fill=(165, 45, 65, 245), width=5, joint="curve")
    draw_mh.ellipse([1175, 895, 1215, 905], fill=(95, 30, 45, 240))
    draw_mh.arc([1158, 896, 1232, 915], 0, 180, fill=(225, 140, 155, 190), width=5)
    save_layer("Layer_06_Mouth_HalfOpen", np.array(pil_mh), 30, -0.035, "Bone_Mouth")

    # D. Wide Mouth (Emotive / Exclamation)
    mouth_wide = mouth_skin_base.copy()
    pil_mw = Image.fromarray(mouth_wide)
    draw_mw = ImageDraw.Draw(pil_mw)
    draw_mw.ellipse([1165, 888, 1228, 915], fill=(110, 32, 50, 255), outline=(165, 45, 65, 255), width=3)
    draw_mw.ellipse([1180, 902, 1215, 913], fill=(235, 130, 150, 230))
    draw_mw.arc([1178, 888, 1216, 898], 0, 180, fill=(255, 255, 255, 245), width=3)
    save_layer("Layer_06_Mouth_Wide", np.array(pil_mw), 30, -0.035, "Bone_Mouth")

    # E. Smile Mouth
    mouth_smile = mouth_skin_base.copy()
    pil_ms = Image.fromarray(mouth_smile)
    draw_ms = ImageDraw.Draw(pil_ms)
    smile_pts = [(1130, 896), (1160, 902), (1192, 904), (1222, 901), (1252, 893)]
    draw_ms.line(smile_pts, fill=(165, 45, 65, 245), width=5, joint="curve")
    draw_ms.arc([1150, 900, 1235, 918], 0, 180, fill=(230, 145, 160, 190), width=5)
    save_layer("Layer_06_Mouth_Smile", np.array(pil_ms), 30, -0.035, "Bone_Mouth")

    # =========================================================================
    # 8. LAYER 07: HAIR SIDE LOCKS & BRAIDS (Left & Right)
    # =========================================================================
    # Left Braid / Side Lock
    hair_sidel = np.zeros((H, W, 4), dtype=np.uint8)
    mask_hsl = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_hsl, (810, 950), (120, 420), 10, 0, 360, 255, -1)
    # Cut out left eye socket so side lock never paints the static open eye over blink layers
    eye_cutout_sidel = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(eye_cutout_sidel, (905, 735), (92, 58), -15, 0, 360, 255, -1)
    mask_hsl = cv2.subtract(mask_hsl, eye_cutout_sidel)
    f_hsl = create_feathered_mask((W, H), (810, 950), (115, 410), angle=10, blur_radius=6)
    hair_sidel[:, :, :3] = master_np[:, :, :3]
    hair_sidel[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_hsl.astype(np.float32)/255.0) * f_hsl).astype(np.uint8)
    save_layer("Layer_07_Hair_SideL", hair_sidel, 35, -0.045, "Bone_HairSideL")

    # Right Side Lock (over shoulder)
    hair_sider = np.zeros((H, W, 4), dtype=np.uint8)
    mask_hsr = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_hsr, (1510, 980), (130, 450), -12, 0, 360, 255, -1)
    f_hsr = create_feathered_mask((W, H), (1510, 980), (125, 440), angle=-12, blur_radius=6)
    hair_sider[:, :, :3] = master_np[:, :, :3]
    hair_sider[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_hsr.astype(np.float32)/255.0) * f_hsr).astype(np.uint8)
    save_layer("Layer_07_Hair_SideR", hair_sider, 35, -0.045, "Bone_HairSideR")

    # =========================================================================
    # 9. LAYER 08: HAIR FRONT BANGS (Front fringe framing forehead and eyes)
    # =========================================================================
    hair_front = np.zeros((H, W, 4), dtype=np.uint8)
    mask_hf = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_hf, (1100, 520), (320, 220), -10, 0, 360, 255, -1)
    
    # Cut out eye and mouth regions so hair bangs never paint eyes onto the scene
    eye_cutout = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(eye_cutout, (915, 735), (85, 55), -15, 0, 360, 255, -1)
    cv2.ellipse(eye_cutout, (1248, 592), (90, 60), -15, 0, 360, 255, -1)
    cv2.ellipse(eye_cutout, (1190, 895), (80, 50), -15, 0, 360, 255, -1)
    mask_hf = cv2.subtract(mask_hf, eye_cutout)

    f_hf = create_feathered_mask((W, H), (1100, 520), (310, 210), angle=-10, blur_radius=5)
    hair_front[:, :, :3] = master_np[:, :, :3]
    hair_front[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_hf.astype(np.float32)/255.0) * f_hf).astype(np.uint8)
    save_layer("Layer_08_Hair_Front_Bangs", hair_front, 40, -0.07, "Bone_HairFront")

    # =========================================================================
    # 10. LAYER 09: ACCESSORY LILY & RIBBON (Pinned in hair on left)
    # =========================================================================
    lily_ribbon = np.zeros((H, W, 4), dtype=np.uint8)
    mask_lr = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_lr, (810, 410), (130, 160), -20, 0, 360, 255, -1)
    f_lr = create_feathered_mask((W, H), (810, 410), (125, 155), angle=-20, blur_radius=4)
    lily_ribbon[:, :, :3] = master_np[:, :, :3]
    lily_ribbon[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_lr.astype(np.float32)/255.0) * f_lr).astype(np.uint8)
    save_layer("Layer_09_Accessory_Lily_Ribbon", lily_ribbon, 45, -0.085, "Bone_LilyOrnament")

    # =========================================================================
    # 11. LAYER 10: BODY BREATHING CHEST (Bust & collarbone highlight)
    # =========================================================================
    chest_np = np.zeros((H, W, 4), dtype=np.uint8)
    mask_ch = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_ch, (1180, 1320), (320, 220), -5, 0, 360, 255, -1)
    f_ch = create_feathered_mask((W, H), (1180, 1320), (310, 210), angle=-5, blur_radius=6)
    chest_np[:, :, :3] = master_np[:, :, :3]
    chest_np[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_ch.astype(np.float32)/255.0) * f_ch).astype(np.uint8)
    save_layer("Layer_10_Body_Breathing_Chest", chest_np, 15, 0.05, "Bone_Chest")

    # =========================================================================
    # 12. LAYER 11: RIGHT ARM & HAND (Tenderly raised near collarbone)
    # =========================================================================
    arm_r = np.zeros((H, W, 4), dtype=np.uint8)
    mask_ar = np.zeros((H, W), dtype=np.uint8)
    cv2.ellipse(mask_ar, (1260, 1180), (135, 105), -20, 0, 360, 255, -1)
    f_ar = create_feathered_mask((W, H), (1260, 1180), (130, 100), angle=-20, blur_radius=4)
    arm_r[:, :, :3] = master_np[:, :, :3]
    arm_r[:, :, 3] = (master_np[:, :, 3].astype(np.float32) * (mask_ar.astype(np.float32)/255.0) * f_ar).astype(np.uint8)
    save_layer("Layer_11_Arm_Right", arm_r, 38, -0.06, "Bone_ArmR")

    # =========================================================================
    # Export Master Image (improved_aurelia_master.png)
    # =========================================================================
    master_out_path = os.path.join(repo_root, "improved_aurelia_master.png")
    master_pil.save(master_out_path, format="PNG")
    print(f"\nMaster Image Saved to Root: {master_out_path} ({W}x{H})")

    # =========================================================================
    # Export Rig Definition JSON
    # =========================================================================
    rig_manifest = {
        "character": "Aurelia",
        "canvas_size": {"width": W, "height": H},
        "pixels_per_unit": 100.0,
        "root_pivot": {"x": 0.5, "y": 0.0},
        "bone_hierarchy": {
            "Bone_Root": {"pos": [0.0, 0.0, 0.0], "parent": None},
            "Bone_Pelvis": {"pos": [0.0, 0.4, 0.0], "parent": "Bone_Root"},
            "Bone_Torso": {"pos": [0.0, 1.1, 0.0], "parent": "Bone_Pelvis"},
            "Bone_Chest": {"pos": [0.0, 1.5, 0.0], "parent": "Bone_Torso"},
            "Bone_Neck": {"pos": [0.0, 1.9, 0.0], "parent": "Bone_Chest"},
            "Bone_Head": {"pos": [0.0, 2.2, 0.0], "parent": "Bone_Neck"},
            "Bone_EyeL": {"pos": [-0.22, 0.05, 0.0], "parent": "Bone_Head"},
            "Bone_IrisL": {"pos": [-0.22, 0.05, 0.0], "parent": "Bone_EyeL"},
            "Bone_EyeR": {"pos": [0.15, 0.18, 0.0], "parent": "Bone_Head"},
            "Bone_IrisR": {"pos": [0.15, 0.18, 0.0], "parent": "Bone_EyeR"},
            "Bone_Mouth": {"pos": [0.08, -0.22, 0.0], "parent": "Bone_Head"},
            "Bone_HairFront": {"pos": [0.0, 0.25, 0.0], "parent": "Bone_Head"},
            "Bone_HairSideL": {"pos": [-0.35, -0.15, 0.0], "parent": "Bone_Head"},
            "Bone_HairSideR": {"pos": [0.38, -0.12, 0.0], "parent": "Bone_Head"},
            "Bone_HairBack": {"pos": [0.0, -0.10, 0.0], "parent": "Bone_Torso"},
            "Bone_LilyOrnament": {"pos": [-0.32, 0.35, 0.0], "parent": "Bone_Head"},
            "Bone_ArmR": {"pos": [0.18, 0.05, 0.0], "parent": "Bone_Chest"}
        },
        "layers": layers
    }

    rig_json_path = os.path.join(repo_root, "rig_definition.json")
    with open(rig_json_path, "w", encoding="utf-8") as f:
        json.dump(rig_manifest, f, indent=2, ensure_ascii=False)
    print(f"Rig Definition JSON written to: {rig_json_path}")
    print(f"Total Layers Extracted & Registered: {len(layers)}")

if __name__ == "__main__":
    main()
