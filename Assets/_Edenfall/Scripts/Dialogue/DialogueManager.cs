using System;
using System.Collections;
using System.Collections.Generic;
using Edenfall.Characters;
using Edenfall.Core;
using Edenfall.NPC.Relationships;
using UnityEngine;

namespace Edenfall.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Runtime State")]
        private CharacterDefinition currentSpeaker;
        private DialogueTree currentTree;
        private DialogueNode currentNode;
        private RelationshipModel currentRelationship;

        public event Action<CharacterDefinition, DialogueNode, Sprite> OnNodeDisplayed;
        public event Action<List<DialogueChoice>> OnChoicesAvailable;
        public event Action OnDialogueFinished;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartDialogue(CharacterDefinition speaker, DialogueTree tree, RelationshipModel relationship)
        {
            currentSpeaker = speaker;
            currentTree = tree;
            currentRelationship = relationship;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Dialogue);
            }

            GameEvents.TriggerDialogueStarted(speaker.characterId, tree.initialNodeId);

            // Dynamically select node based on affection if intimate
            string startNode = tree.initialNodeId;
            if (relationship != null && relationship.affection >= 50 && tree.GetNode("aur_intimate") != null)
            {
                startNode = "aur_intimate";
            }

            GoToNode(startNode);
        }

        public void GoToNode(string nodeId)
        {
            if (currentTree == null) return;
            currentNode = currentTree.GetNode(nodeId);

            if (currentNode == null)
            {
                EndDialogue();
                return;
            }

            GameEvents.TriggerDialogueNodeEntered(nodeId);

            Sprite portrait = currentSpeaker != null ? currentSpeaker.GetPortrait(currentNode.emotion) : null;
            OnNodeDisplayed?.Invoke(currentSpeaker, currentNode, portrait);

            // Filter choices by requirements
            var validChoices = new List<DialogueChoice>();
            foreach (var choice in currentNode.choices)
            {
                if (currentRelationship != null)
                {
                    if (choice.reqAffection > 0 && currentRelationship.affection < choice.reqAffection) continue;
                    if (choice.reqTrust > 0 && currentRelationship.trust < choice.reqTrust) continue;
                }
                validChoices.Add(choice);
            }

            OnChoicesAvailable?.Invoke(validChoices);
        }

        public void SelectChoice(DialogueChoice choice)
        {
            if (choice == null)
            {
                EndDialogue();
                return;
            }

            // Apply relationship impact
            if (currentRelationship != null)
            {
                currentRelationship.ApplyDeltas(
                    choice.affectionDelta,
                    choice.trustDelta,
                    choice.familiarityDelta,
                    choice.respectDelta,
                    choice.comfortDelta,
                    choice.conflictDelta
                );

                bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
                string memText = choice.GetMemoryText(isVi);
                if (!string.IsNullOrEmpty(memText))
                {
                    var impact = choice.tone == DialogueTone.Romantic ? NPCMemory.MemoryImpact.Romantic :
                                 choice.conflictDelta > 0 ? NPCMemory.MemoryImpact.Negative :
                                 NPCMemory.MemoryImpact.Positive;

                    string dayTime = GameManager.Instance != null
                        ? $"Ngày {GameManager.Instance.CurrentDay} - {GameManager.Instance.CurrentTime.ToDisplayString(isVi)}"
                        : "Ngày 1";

                    currentRelationship.AddMemory(memText, impact, dayTime);
                }
            }

            // Trigger cinematics
            if (!string.IsNullOrEmpty(choice.triggerCinematicId))
            {
                GameEvents.TriggerCinematicRequested(choice.triggerCinematicId);
            }

            // Trigger quest updates
            if (!string.IsNullOrEmpty(choice.triggerQuestId))
            {
                if (choice.triggerQuestStep >= 0)
                {
                    GameEvents.TriggerQuestStepUpdated(choice.triggerQuestId, choice.triggerQuestStep);
                }
                else
                {
                    GameEvents.TriggerQuestStarted(choice.triggerQuestId);
                }
            }

            // Branching
            if (string.IsNullOrEmpty(choice.nextNodeId))
            {
                EndDialogue();
            }
            else
            {
                GoToNode(choice.nextNodeId);
            }
        }

        public void EndDialogue()
        {
            string speakerId = currentSpeaker != null ? currentSpeaker.characterId : "";
            currentSpeaker = null;
            currentNode = null;
            currentTree = null;
            currentRelationship = null;

            if (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.Dialogue)
            {
                GameManager.Instance.SetGameMode(GameMode.Exploration);
            }

            GameEvents.TriggerDialogueEnded(speakerId);
            OnDialogueFinished?.Invoke();
        }
    }
}
