using System.Collections;
using Edenfall.Characters;
using Edenfall.Core;
using Edenfall.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Edenfall.Cinematics
{
    public class SolsticeCinematicSequence : MonoBehaviour
    {
        [Header("Cinematic Cameras & Targets")]
        [SerializeField] private CameraController2D cameraController;
        [SerializeField] private Transform wideShotAnchor;
        [SerializeField] private Transform aureliaCloseUpAnchor;
        [SerializeField] private Transform twoShotAnchor;

        [Header("Character Rigs")]
        [SerializeField] private CharacterVisualRig aureliaRig;
        [SerializeField] private CharacterVisualRig echoRig;

        [Header("UI & Subtitles")]
        [SerializeField] private GameObject cinematicOverlayCanvas;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text speakerText;
        [SerializeField] private Button skipButton;

        [Header("Cinematic Data & CG")]
        [SerializeField] private string sceneId = "cg_solstice_vow";
        [SerializeField] private Sprite cgSprite;

        private Coroutine sequenceCoroutine;
        private bool isSkipping = false;

        public bool IsPlaying => sequenceCoroutine != null;

        public void ConfigureSequence(
            CameraController2D camCtrl,
            Transform wideAnchor,
            Transform aureliaAnchor,
            Transform twoAnchor,
            CharacterVisualRig aurelia,
            CharacterVisualRig echo,
            GameObject overlayCanvas,
            Text subtitle,
            Text speaker,
            Button skipBtn,
            Sprite cg)
        {
            cameraController = camCtrl;
            wideShotAnchor = wideAnchor;
            aureliaCloseUpAnchor = aureliaAnchor;
            twoShotAnchor = twoAnchor;
            aureliaRig = aurelia;
            echoRig = echo;
            cinematicOverlayCanvas = overlayCanvas;
            subtitleText = subtitle;
            speakerText = speaker;
            skipButton = skipBtn;
            cgSprite = cg;
        }

        private void Start()
        {
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(SkipCinematic);
            }

            if (cinematicOverlayCanvas != null)
            {
                cinematicOverlayCanvas.SetActive(false);
            }
        }

        public void PlaySequence()
        {
            if (sequenceCoroutine != null) return;
            sequenceCoroutine = StartCoroutine(CinematicRoutine());
        }

        public void SkipCinematic()
        {
            if (!IsPlaying) return;
            isSkipping = true;
        }

        private IEnumerator CinematicRoutine()
        {
            isSkipping = false;

            // 1. Enter Cinematic Mode
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Cinematic);
            }

            if (cinematicOverlayCanvas != null)
            {
                cinematicOverlayCanvas.SetActive(true);
            }

            // Shot 1: Wide Establishing Shot (3.0s)
            SetCameraAnchor(wideShotAnchor, 8f, 2.5f);
            SetSubtitle("Aurelia", "Dawn Sanctuary... Trái tim của vương quốc từng rực sáng dưới hào quang Tinh Tú.", 
                "Dawn Sanctuary... The radiant heart of a realm once bathed in celestial luminescence.");
            
            if (aureliaRig != null)
            {
                aureliaRig.SetFacing(false);
                aureliaRig.SetExpression(CharacterEmotion.Melancholy);
            }

            yield return WaitOrSkip(3.2f);
            if (isSkipping) goto FinishCinematic;

            // Shot 2: Close-up on Aurelia turning (3.5s)
            SetCameraAnchor(aureliaCloseUpAnchor, 5.0f, 4.0f);
            if (aureliaRig != null)
            {
                aureliaRig.SetFacing(true);
                aureliaRig.SetExpression(CharacterEmotion.Happy);
                aureliaRig.SetTalking(true);
            }

            SetSubtitle("Aurelia", "Ngươi đã tới, Echo. Lời tiên tri của Mặt Trời Vỡ Vụn cuối cùng đã tìm thấy người kế thừa.",
                "You have arrived, Echo. The prophecy of the Shattered Sun has at last found its rightful heir.");

            yield return WaitOrSkip(3.8f);
            if (aureliaRig != null) aureliaRig.SetTalking(false);
            if (isSkipping) goto FinishCinematic;

            // Shot 3: Two-Shot Meeting at Terrace (4.0s)
            SetCameraAnchor(twoShotAnchor, 6.0f, 3.5f);
            if (echoRig != null)
            {
                echoRig.SetFacing(false);
                echoRig.SetExpression(CharacterEmotion.Serious);
            }

            SetSubtitle("Echo", "Dù thiên đàng này đã vỡ vụn, ta nguyện cùng nàng thắp lại ánh sáng rực rỡ thuở ban sơ.",
                "Though this paradise lies fractured, I vow to rekindle its primordial radiance beside you.");

            yield return WaitOrSkip(4.0f);
            if (isSkipping) goto FinishCinematic;

            // Shot 4: Mutual Vow & Starlight Flare (3.0s)
            if (aureliaRig != null)
            {
                aureliaRig.SetExpression(CharacterEmotion.Loving);
            }

            SetSubtitle("Aurelia", "Vậy hãy để lời thề này được khắc ghi mãi mãi giữa các vì sao.",
                "Then let this sacred vow be etched forever across the boundless stars.");

            yield return WaitOrSkip(3.5f);

        FinishCinematic:
            // Unlock CG in Gallery
            if (GalleryManager.Instance != null && !string.IsNullOrEmpty(sceneId))
            {
                GalleryManager.Instance.UnlockScene(sceneId);
            }

            // Restore Camera to Player
            var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (cameraController != null && player != null)
            {
                cameraController.SetTarget(player.transform);
                cameraController.SetSpeed(4f);
            }

            if (cinematicOverlayCanvas != null)
            {
                cinematicOverlayCanvas.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Exploration);
            }

            sequenceCoroutine = null;
            isSkipping = false;
        }

        private void SetCameraAnchor(Transform anchor, float orthoSize, float speed)
        {
            if (cameraController == null || anchor == null) return;
            cameraController.SetTarget(anchor);
            cameraController.SetSpeed(speed);
        }

        private void SetSubtitle(string speaker, string viText, string enText)
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;
            if (speakerText != null) speakerText.text = speaker;
            if (subtitleText != null) subtitleText.text = isVi ? viText : enText;
        }

        private IEnumerator WaitOrSkip(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration && !isSkipping)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
