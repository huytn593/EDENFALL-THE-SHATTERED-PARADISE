using Edenfall.Core;
using Edenfall.NPC.Behavior;
using Edenfall.World;
using UnityEngine;
using UnityEngine.InputSystem;

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

            // Modern Input System Handling
            float h = 0f;
            float v = 0f;
            bool wantSprint = false;
            bool triggerVision = false;
            bool triggerTime = false;
            bool triggerInteract = false;

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;

                wantSprint = kb.leftShiftKey.isPressed && (h != 0 || v != 0);
                triggerVision = kb.tabKey.wasPressedThisFrame || kb.vKey.wasPressedThisFrame;
                triggerTime = kb.tKey.wasPressedThisFrame;
                triggerInteract = kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
            }

            var pad = Gamepad.current;
            if (pad != null)
            {
                var stick = pad.leftStick.ReadValue();
                if (Mathf.Abs(stick.x) > 0.15f) h = stick.x;
                if (Mathf.Abs(stick.y) > 0.15f) v = stick.y;

                if (pad.leftShoulder.isPressed) wantSprint = true;
                if (pad.buttonNorth.wasPressedThisFrame) triggerVision = true;
                if (pad.buttonSouth.wasPressedThisFrame) triggerInteract = true;
            }

            moveInput = new Vector2(h, v).normalized;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                facingDirection = moveInput;
            }

            // Sprinting & Stamina
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
            if (triggerVision)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CycleVisionLayer();
                }
            }

            // Advance Time Shortcut (T key for simulation exploration)
            if (triggerTime)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AdvanceTime();
                }
            }

            // Interact
            if (triggerInteract)
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
