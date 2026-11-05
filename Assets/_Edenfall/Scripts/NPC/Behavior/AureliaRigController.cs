using Edenfall.Characters;
using Edenfall.Dialogue;
using UnityEngine;

namespace Edenfall.NPC.Behavior
{
    public class AureliaRigController : MonoBehaviour
    {
        [SerializeField] private CharacterVisualRig visualRig;
        [SerializeField] private AureliaLiveRig liveRig;
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
            if (liveRig == null) liveRig = GetComponent<AureliaLiveRig>();
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

            if (liveRig != null)
            {
                var targetState = MapEmotionToAnimationState(node.emotion, true);
                liveRig.SetAnimationState(targetState);
                liveRig.SetTalking(true);
            }

            if (visualRig != null)
            {
                Sprite targetEyes = GetEyesForEmotion(node.emotion);
                Sprite targetMouth = (node.emotion == CharacterEmotion.Happy || node.emotion == CharacterEmotion.Loving) 
                    ? mouthSmile 
                    : mouthNeutral;

                visualRig.SetExpression(node.emotion, targetEyes, targetMouth);
                visualRig.SetTalking(true);
            }
        }

        private void HandleDialogueFinished()
        {
            if (liveRig != null)
            {
                liveRig.SetTalking(false);
                liveRig.SetAnimationState(AureliaAnimationState.Idle_Normal);
            }

            if (visualRig != null)
            {
                visualRig.SetTalking(false);
                visualRig.SetExpression(CharacterEmotion.Neutral, eyesNeutral, mouthNeutral);
            }
        }

        public void ApplyEmotion(CharacterEmotion emotion)
        {
            if (liveRig != null)
            {
                var targetState = MapEmotionToAnimationState(emotion, false);
                liveRig.SetAnimationState(targetState);
            }

            if (visualRig != null)
            {
                Sprite targetEyes = GetEyesForEmotion(emotion);
                Sprite targetMouth = (emotion == CharacterEmotion.Happy || emotion == CharacterEmotion.Loving) ? mouthSmile : mouthNeutral;
                visualRig.SetExpression(emotion, targetEyes, targetMouth);
            }
        }

        public AureliaAnimationState MapEmotionToAnimationState(CharacterEmotion emotion, bool isTalking)
        {
            return emotion switch
            {
                CharacterEmotion.Happy or CharacterEmotion.Smiling => isTalking ? AureliaAnimationState.Talk_Normal : AureliaAnimationState.Idle_Happy,
                CharacterEmotion.Flustered or CharacterEmotion.Blushing => isTalking ? AureliaAnimationState.Talk_Shy : AureliaAnimationState.Idle_Shy,
                CharacterEmotion.Intimate or CharacterEmotion.Loving => AureliaAnimationState.React_Romantic,
                CharacterEmotion.Surprised => AureliaAnimationState.React_Surprised,
                CharacterEmotion.Serious => isTalking ? AureliaAnimationState.Talk_Serious : AureliaAnimationState.Idle_Normal,
                CharacterEmotion.Melancholic => AureliaAnimationState.React_Sad,
                CharacterEmotion.Angry => AureliaAnimationState.React_Angry,
                CharacterEmotion.Confused => AureliaAnimationState.React_Surprised,
                CharacterEmotion.Pensive => AureliaAnimationState.Gesture_Thinking,
                CharacterEmotion.Gentle => isTalking ? AureliaAnimationState.Talk_Gentle : AureliaAnimationState.Idle_Normal,
                _ => isTalking ? AureliaAnimationState.Talk_Normal : AureliaAnimationState.Idle_Normal
            };
        }

        private Sprite GetEyesForEmotion(CharacterEmotion emotion)
        {
            return emotion switch
            {
                CharacterEmotion.Happy => eyesSmile != null ? eyesSmile : eyesNeutral,
                CharacterEmotion.Loving => eyesSmile != null ? eyesSmile : eyesNeutral,
                CharacterEmotion.Serious => eyesSerious != null ? eyesSerious : eyesNeutral,
                CharacterEmotion.Melancholic => eyesSorrow != null ? eyesSorrow : eyesNeutral,
                CharacterEmotion.Blushing => eyesBlush != null ? eyesBlush : eyesNeutral,
                CharacterEmotion.Surprised => eyesSurprise != null ? eyesSurprise : eyesNeutral,
                _ => eyesNeutral
            };
        }
    }
}
