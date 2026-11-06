using UnityEngine;

namespace Edenfall.World
{
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Vector2 parallaxMultiplier = new Vector2(0.2f, 0.1f);
        [SerializeField] private bool infiniteHorizontal = true;
        [SerializeField] private float textureUnitSizeX = 30f;

        private Vector3 lastCameraPosition;

        public void SetMultiplier(Vector2 mult)
        {
            parallaxMultiplier = mult;
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                lastCameraPosition = targetCamera.transform.position;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            Vector3 delta = targetCamera.transform.position - lastCameraPosition;
            transform.position += new Vector3(delta.x * parallaxMultiplier.x, delta.y * parallaxMultiplier.y, 0);
            lastCameraPosition = targetCamera.transform.position;

            if (infiniteHorizontal && Mathf.Abs(targetCamera.transform.position.x - transform.position.x) >= textureUnitSizeX)
            {
                float offsetPositionX = (targetCamera.transform.position.x - transform.position.x) % textureUnitSizeX;
                transform.position = new Vector3(targetCamera.transform.position.x + offsetPositionX, transform.position.y, transform.position.z);
            }
        }
    }
}
