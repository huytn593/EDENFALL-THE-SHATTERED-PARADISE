using Edenfall.Core;
using Edenfall.NPC.Behavior;
using Edenfall.World;
using UnityEngine;

namespace Edenfall.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 7.0f;
        [SerializeField] private float staminaMax = 100f;
        [SerializeField] private float staminaDrainRate = 25f;
        [SerializeField] private float staminaRecoveryRate = 20f;

        [Header("Runtime State")]
        [SerializeField] private float currentStamina;
        [SerializeField] private Vector2 moveInput;
        [SerializeField] private Vector2 facingDirection = Vector2.down;
        [SerializeField] private bool isSprinting;

        public float CurrentStamina => currentStamina;
        public float MaxStamina => staminaMax;
        public Vector2 FacingDirection => facingDirection;
        public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

        private Rigidbody2D rb;
        private NPCController nearbyNPC;
        private InteractiveProp nearbyProp;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            currentStamina = staminaMax;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentMode != GameMode.Exploration)
            {
                moveInput = Vector2.zero;
                return;
            }

            // Input
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(h, v).normalized;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                facingDirection = moveInput;
            }

            // Sprinting & Stamina
            bool wantSprint = Input.GetKey(KeyCode.LeftShift) && IsMoving;
            if (wantSprint && currentStamina > 5f)
            {
                isSprinting = true;
                currentStamina = Mathf.Max(currentStamina - staminaDrainRate * Time.deltaTime, 0f);
            }
            else
            {
                isSprinting = false;
                currentStamina = Mathf.Min(currentStamina + staminaRecoveryRate * Time.deltaTime, staminaMax);
            }

            // Vision Layer Toggle
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.V))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CycleVisionLayer();
                }
            }

            // Advance Time Shortcut (T key for simulation exploration)
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AdvanceTime();
                }
            }

            // Interact
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                TryInteract();
            }
        }

        private void FixedUpdate()
        {
            float speed = isSprinting ? sprintSpeed : walkSpeed;
            rb.linearVelocity = moveInput * speed;
        }

        private void TryInteract()
        {
            if (nearbyNPC != null)
            {
                nearbyNPC.StartInteraction();
            }
            else if (nearbyProp != null)
            {
                nearbyProp.Interact();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<NPCController>(out var npc))
            {
                nearbyNPC = npc;
                npc.CanInteract(transform.position);
            }
            else if (other.TryGetComponent<InteractiveProp>(out var prop))
            {
                nearbyProp = prop;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<NPCController>(out var npc) && nearbyNPC == npc)
            {
                nearbyNPC = null;
            }
            else if (other.TryGetComponent<InteractiveProp>(out var prop) && nearbyProp == prop)
            {
                nearbyProp = null;
            }
        }
    }
}
