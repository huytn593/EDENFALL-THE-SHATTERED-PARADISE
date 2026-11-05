using Edenfall.Characters;
using Edenfall.Dialogue;
using UnityEngine;

namespace Edenfall.NPC.Behavior
{
    [RequireComponent(typeof(CharacterVisualRig))]
    public class AureliaRigController : MonoBehaviour
    {
        [SerializeField] private CharacterVisualRig visualRig;
        [SerializeField] private NPCController npcController;

        [Header("Aurelia Emotion Presets")]
        [SerializeField] private Sprite eyesNeutral;
        [SerializeField] private Sprite eyesSmile;
        [SerializeField] private Sprite eyesSerious;
        [SerializeField] private Sprite eyesSorrow;
        [SerializeField] private Sprite eyesBlush;
        [SerializeField] private Sprite eyesSurprise;

        [SerializeField] private Sprite mouthNeutral;
        [SerializeField] private Sprite mouthSmile;
        [SerializeField] private Sprite mouthSpeaking;

        public void ConfigurePresets(
            CharacterVisualRig rig,
            Sprite eyeNeut, Sprite eyeSmil, Sprite eyeSer, Sprite eyeSorr, Sprite eyeBlsh, Sprite eyeSurp,
            Sprite mthNeut, Sprite mthSmil, Sprite mthSpk)
        {
            visualRig = rig;
            eyesNeutral = eyeNeut;
            eyesSmile = eyeSmil;
            eyesSerious = eyeSer;
            eyesSorrow = eyeSorr;
            eyesBlush = eyeBlsh;
            eyesSurprise = eyeSurp;
            mouthNeutral = mthNeut;
            mouthSmile = mthSmil;
            mouthSpeaking = mthSpk;
        }

        private void Awake()
        {
            if (visualRig == null) visualRig = GetComponent<CharacterVisualRig>();
            if (npcController == null) npcController = GetComponent<NPCController>();
        }

        private void OnEnable()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed += HandleDialogueNode;
                DialogueManager.Instance.OnDialogueFinished += HandleDialogueFinished;
            }
        }

        private void OnDisable()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= HandleDialogueNode;
                DialogueManager.Instance.OnDialogueFinished -= HandleDialogueFinished;
            }
        }

        private void HandleDialogueNode(CharacterDefinition speaker, DialogueNode node, Sprite portrait)
        {
            if (speaker == null || speaker.characterId != "aurelia") return;

            Sprite targetEyes = GetEyesForEmotion(node.emotion);
            Sprite targetMouth = (node.emotion == CharacterEmotion.Happy || node.emotion == CharacterEmotion.Loving) 
                ? mouthSmile 
                : mouthNeutral;

            visualRig.SetExpression(node.emotion, targetEyes, targetMouth);
            visualRig.SetTalking(true);
        }

        private void HandleDialogueFinished()
        {
            visualRig.SetTalking(false);
            visualRig.SetExpression(CharacterEmotion.Neutral, eyesNeutral, mouthNeutral);
        }

        public void ApplyEmotion(CharacterEmotion emotion)
        {
            Sprite targetEyes = GetEyesForEmotion(emotion);
            Sprite targetMouth = (emotion == CharacterEmotion.Happy || emotion == CharacterEmotion.Loving) ? mouthSmile : mouthNeutral;
            visualRig.SetExpression(emotion, targetEyes, targetMouth);
        }

        private Sprite GetEyesForEmotion(CharacterEmotion emotion)
        {
            return emotion switch
            {
                CharacterEmotion.Happy => eyesSmile != null ? eyesSmile : eyesNeutral,
                CharacterEmotion.Loving => eyesSmile != null ? eyesSmile : eyesNeutral,
                CharacterEmotion.Serious => eyesSerious != null ? eyesSerious : eyesNeutral,
                CharacterEmotion.Melancholy => eyesSorrow != null ? eyesSorrow : eyesNeutral,
                CharacterEmotion.Blushing => eyesBlush != null ? eyesBlush : eyesNeutral,
                CharacterEmotion.Surprised => eyesSurprise != null ? eyesSurprise : eyesNeutral,
                _ => eyesNeutral
            };
        }
    }
}
