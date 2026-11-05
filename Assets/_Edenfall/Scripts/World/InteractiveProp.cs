using Edenfall.Core;
using Edenfall.Inventory;
using UnityEngine;

namespace Edenfall.World
{
    public class InteractiveProp : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string propId = "celestial_shrine";
        [SerializeField] private string propNameVi = "Đền Thờ Tinh Tú Cổ Đại";
        [SerializeField] private string propNameEn = "Ancient Celestial Shrine";
        [TextArea(2, 4)] [SerializeField] private string descriptionVi = "Một đài tế bằng đá cẩm thạch trắng phát ra hào quang ấm áp của vầng thái dương.";
        [TextArea(2, 4)] [SerializeField] private string descriptionEn = "A white marble dais pulsing with warm solar luminescence.";

        [Header("Layer Visibility")]
        [SerializeField] private bool requiresVisionLayer = false;
        [SerializeField] private VisionLayer requiredLayer = VisionLayer.EnergyPathways;

        [Header("Rewards & State")]
        [SerializeField] private ItemDefinition rewardItem;
        [SerializeField] private float sanctuaryRestorationBonus = 5f;
        [SerializeField] private bool oneTimeOnly = true;
        [SerializeField] private bool isExhausted = false;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject visualIndicator;
        [SerializeField] private ParticleSystem interactionParticles;

        private void OnEnable()
        {
            GameEvents.OnVisionLayerChanged += HandleVisionLayerChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnVisionLayerChanged -= HandleVisionLayerChanged;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleVisionLayerChanged(GameManager.Instance.ActiveVisionLayer);
            }
        }

        private void HandleVisionLayerChanged(VisionLayer layer)
        {
            if (requiresVisionLayer)
            {
                bool active = layer == requiredLayer;
                gameObject.SetActive(active);
            }
        }

        public void Interact()
        {
            if (oneTimeOnly && isExhausted)
            {
                return;
            }

            if (rewardItem != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(rewardItem, 1);
            }

            if (sanctuaryRestorationBonus > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.AddSanctuaryRestoration(sanctuaryRestorationBonus);
            }

            if (interactionParticles != null)
            {
                interactionParticles.Play();
            }

            isExhausted = true;
            if (visualIndicator != null)
            {
                visualIndicator.SetActive(false);
            }
        }
    }
}
