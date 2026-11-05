using UnityEngine;

namespace Edenfall.Gameplay
{
    public class CameraController2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-25f, -25f);
        [SerializeField] private Vector2 maxBounds = new Vector2(25f, 25f);

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPos = target.position + offset;
            if (useBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
                targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
