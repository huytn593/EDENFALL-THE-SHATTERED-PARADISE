using UnityEngine;

namespace Edenfall.World
{
    public enum StagingMode
    {
        DialoguePortrait,
        FullBodyGrounded
    }

    [ExecuteAlways]
    public class NPCStageAnchor : MonoBehaviour
    {
        [Header("Staging Configuration")]
        [SerializeField] private StagingMode mode = StagingMode.DialoguePortrait;
        [SerializeField] private float groundContactY = -3.20f;
        [SerializeField] private float portraitBaselineY = -4.95f;
        [SerializeField] private float targetPortraitHeightPercent = 0.85f; // 85% of screen height for intimate VN staging
        [SerializeField] private float targetFullBodyHeightPercent = 0.65f;

        [Header("Framing Offsets")]
        [SerializeField] private Vector3 screenCenterOffset = new Vector3(0.15f, 0f, 0f); // Intimate center framing for VN

        [Header("Allowed Idle Movement Bounds")]
        [SerializeField] private Vector2 maxIdleTranslationOffset = new Vector2(0.05f, 0.08f);

        public StagingMode CurrentMode => mode;
        public float GroundContactY => groundContactY;
        public float PortraitBaselineY => portraitBaselineY;

        /// <summary>
        /// Mathematically aligns the character root so that either its bottom bounding edge (feet in full body)
        /// or waist baseline (in portrait) exactly touches the stage anchor line.
        /// </summary>
        public void AlignCharacter(Transform characterRoot, SpriteRenderer primaryRenderer, StagingMode stagingMode, Camera cam)
        {
            if (characterRoot == null || primaryRenderer == null || primaryRenderer.sprite == null) return;

            mode = stagingMode;
            Sprite sprite = primaryRenderer.sprite;

            // Calculate vertical world bounds of sprite given its pixels-per-unit and scale
            float spritePPU = sprite.pixelsPerUnit > 0 ? sprite.pixelsPerUnit : 100f;
            float rawSpriteHeight = sprite.rect.height / spritePPU;
            float pivotYRatio = sprite.pivot.y / sprite.rect.height; // e.g. 0.0 = bottom, 0.5 = center

            // In an orthographic camera, visible world height is 2 * orthoSize
            float visibleWorldHeight = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 8.4f;
            float targetHeight = visibleWorldHeight * (mode == StagingMode.DialoguePortrait ? targetPortraitHeightPercent : targetFullBodyHeightPercent);

            // Compute ideal uniform scale
            float calculatedScale = targetHeight / rawSpriteHeight;
            characterRoot.localScale = new Vector3(calculatedScale, calculatedScale, 1f);

            // Calculate bottom-most contact point in world space
            float distanceToBottom = pivotYRatio * rawSpriteHeight * calculatedScale;

            float targetWorldY;
            if (mode == StagingMode.DialoguePortrait)
            {
                // In portrait mode, align so character chest/face is squarely in the upper two thirds
                targetWorldY = portraitBaselineY + distanceToBottom;
            }
            else
            {
                // In full body mode, feet must precisely touch ground contact line
                targetWorldY = groundContactY + distanceToBottom;
            }

            Vector3 finalPos = new Vector3(screenCenterOffset.x, targetWorldY, characterRoot.position.z);
            characterRoot.position = finalPos;
        }

        public Vector3 ClampIdleMotion(Vector3 basePos, Vector3 currentOffset)
        {
            float cx = Mathf.Clamp(currentOffset.x, -maxIdleTranslationOffset.x, maxIdleTranslationOffset.x);
            // Never allow idle animation to dip below ground contact line
            float cy = Mathf.Clamp(currentOffset.y, 0f, maxIdleTranslationOffset.y);
            return basePos + new Vector3(cx, cy, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw ground contact line
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(-10f, groundContactY, 0), new Vector3(10f, groundContactY, 0));

            // Draw portrait baseline
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(-10f, portraitBaselineY, 0), new Vector3(10f, portraitBaselineY, 0));
        }
    }
}
