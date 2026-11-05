using Edenfall.Characters;
using Edenfall.Core;
using Edenfall.Dialogue;
using Edenfall.NPC.Relationships;
using Edenfall.NPC.Scheduling;
using UnityEngine;

namespace Edenfall.NPC.Behavior
{
    public class NPCController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CharacterDefinition definition;
        [SerializeField] private NPCSchedule schedule;
        [SerializeField] private DialogueTree dialogueTree;

        [Header("State")]
        [SerializeField] private RelationshipModel relationship;
        public RelationshipModel Relationship => relationship;
        public CharacterDefinition Definition => definition;

        [Header("Movement & Interaction")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float interactionRadius = 2.5f;
        [SerializeField] private GameObject interactIndicator;

        private Vector2 targetPosition;
        private bool isMoving = false;

        private void Awake()
        {
            if (definition != null)
            {
                relationship = new RelationshipModel(definition.characterId)
                {
                    affection = definition.startingAffection,
                    trust = definition.startingTrust,
                    familiarity = definition.startingFamiliarity,
                    respect = definition.startingRespect,
                    comfort = definition.startingComfort,
                    conflict = definition.startingConflict
                };
                relationship.EvaluateStage();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnTimeOfDayChanged += HandleTimeOfDayChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnTimeOfDayChanged -= HandleTimeOfDayChanged;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleTimeOfDayChanged(GameManager.Instance.CurrentTime);
            }
        }

        private void Update()
        {
            if (isMoving)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
                {
                    isMoving = false;
                }
            }
        }

        private void HandleTimeOfDayChanged(TimeOfDay time)
        {
            if (schedule == null) return;
            var entry = schedule.GetEntryForTime(time);
            if (entry != null)
            {
                targetPosition = entry.targetPosition;
                isMoving = true;
            }
        }

        public bool CanInteract(Vector2 playerPosition)
        {
            float dist = Vector2.Distance(transform.position, playerPosition);
            bool can = dist <= interactionRadius;
            if (interactIndicator != null)
            {
                interactIndicator.SetActive(can);
            }
            return can;
        }

        public void StartInteraction()
        {
            if (dialogueTree != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(definition, dialogueTree, relationship);
            }
        }
    }
}
