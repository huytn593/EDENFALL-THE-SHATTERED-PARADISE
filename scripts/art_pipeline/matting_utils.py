"""
Matting & Alpha Cleaning Utilities
Provides edge defringing, alpha speckle removal, and color bleeding for pristine alpha cutouts.
"""
import cv2
import numpy as np
from PIL import Image

def clean_alpha_mask(alpha_channel, min_area=150):
    """
    Remove floating specks/islands and smooth rough boundary artifacts.
    Keeps large connected components and removes tiny detached background remnants.
    """
    # Threshold binary mask
    _, thresh = cv2.threshold(alpha_channel, 10, 255, cv2.THRESH_BINARY)
    
    # Find all connected components
    num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(thresh)
    
    # Create clean binary mask
    clean_mask = np.zeros_like(alpha_channel)
    for i in range(1, num_labels):
        area = stats[i, cv2.CC_STAT_AREA]
        if area >= min_area:
            clean_mask[labels == i] = 255

    # Multiply original alpha with clean mask to preserve semi-transparency inside valid areas
    cleaned_alpha = np.where(clean_mask > 0, alpha_channel, 0)
    return cleaned_alpha

def defringe_rgb_edges(rgba_img):
    """
    Color bleed / un-premultiply semi-transparent edge pixels to prevent dark or white halos.
    Extrapolates opaque foreground RGB into translucent edge pixels.
    """
    arr = np.array(rgba_img)
    if arr.ndim != 3 or arr.shape[2] != 4:
        return rgba_img

    rgb = arr[:, :, :3]
    alpha = arr[:, :, 3]

    # Clean stray specks first
    clean_alpha = clean_alpha_mask(alpha)

    # Inpaint translucent edges using opaque pixels as reference
    opaque_mask = (clean_alpha > 220).astype(np.uint8) * 255
    edge_mask = ((clean_alpha > 0) & (clean_alpha <= 220)).astype(np.uint8) * 255

    if np.any(edge_mask):
        # Bleed RGB into edge region
        bgr = cv2.cvtColor(rgb, cv2.COLOR_RGB2BGR)
        inpaint_mask = (clean_alpha == 0).astype(np.uint8) * 255
        # 1-pixel dilation of opaque mask into edges
        kernel = np.ones((3, 3), np.uint8)
        dilated_opaque = cv2.dilate(opaque_mask, kernel, iterations=2)
        bleed_region = (dilated_opaque > 0) & (clean_alpha < 220)
        
        # Where edge pixels exist, blend with inpainted color
        inpaint_bgr = cv2.inpaint(bgr, (255 - opaque_mask), inpaintRadius=3, flags=cv2.INPAINT_TELEA)
        inpaint_rgb = cv2.cvtColor(inpaint_bgr, cv2.COLOR_BGR2RGB)
        
        rgb = np.where(bleed_region[:, :, None], inpaint_rgb, rgb)

    result = np.dstack([rgb, clean_alpha])
    return Image.fromarray(result)
