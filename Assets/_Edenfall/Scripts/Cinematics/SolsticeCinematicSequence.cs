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
        [Header("Cinematic Cameras & First-Person POV Anchors")]
        [SerializeField] private CameraController2D cameraController;
        [SerializeField] private Transform povWideAnchor;
        [SerializeField] private Transform povCloseUpAnchor;
        [SerializeField] private Transform povIntimateAnchor;

        [Header("Character Rig (Heroine Focus)")]
        [SerializeField] private CharacterVisualRig aureliaRig;

        [Header("UI & Subtitles")]
        [SerializeField] private GameObject cinematicOverlayCanvas;
        [SerializeField] private Image fullScreenCGImage;
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
            Transform closeUpAnchor,
            Transform intimateAnchor,
            CharacterVisualRig aurelia,
            GameObject overlayCanvas,
            Image cgImg,
            Text subtitle,
            Text speaker,
            Button skipBtn,
            Sprite cg)
        {
            cameraController = camCtrl;
            povWideAnchor = wideAnchor;
            povCloseUpAnchor = closeUpAnchor;
            povIntimateAnchor = intimateAnchor;
            aureliaRig = aurelia;
            cinematicOverlayCanvas = overlayCanvas;
            fullScreenCGImage = cgImg;
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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.Cinematic);
            }

            if (cinematicOverlayCanvas != null)
            {
                cinematicOverlayCanvas.SetActive(true);
            }

            if (fullScreenCGImage != null)
            {
                fullScreenCGImage.gameObject.SetActive(false);
            }

            if (aureliaRig != null && !aureliaRig.gameObject.activeInHierarchy)
            {
                aureliaRig.gameObject.SetActive(true);
            }

            // Shot 1: First-Person Wide Establishing Shot (3.0s)
            // Player looks across the Dawn Sanctuary terrace towards Aurelia
            SetCameraAnchor(povWideAnchor, 5.0f, 2.5f);
            SetSubtitle("Aurelia", "Dawn Sanctuary... Trái tim của vương quốc từng rực sáng dưới hào quang Tinh Tú.", 
                "Dawn Sanctuary... The radiant heart of a realm once bathed in celestial luminescence.");
            
            if (aureliaRig != null)
            {
                aureliaRig.SetFacing(false);
                aureliaRig.SetExpression(CharacterEmotion.Melancholy);
            }

            yield return WaitOrSkip(3.2f);
            if (isSkipping) goto FinishCinematic;

            // Shot 2: First-Person Close-Up (3.5s)
            // Aurelia turns directly towards player (the camera)
            SetCameraAnchor(povCloseUpAnchor, 3.8f, 3.5f);
            if (aureliaRig != null)
            {
                aureliaRig.SetFacing(true);
                aureliaRig.SetExpression(CharacterEmotion.Happy);
                aureliaRig.SetTalking(true);
            }

            SetSubtitle("Aurelia", "Ngươi đã tới, Echo. Lời tiên tri của Mặt Trời Vỡ Vụn cuối cùng đã tìm thấy người kế thừa.",
                "You have arrived, Echo. The prophecy of the Shattered Sun has at last found its rightful heir.");

            yield return WaitOrSkip(3.6f);
            if (aureliaRig != null) aureliaRig.SetTalking(false);
            if (isSkipping) goto FinishCinematic;

            // Shot 3: Intimate POV Approach (3.8s)
            // Aurelia steps closer, gazing deeply into the player's eyes
            SetCameraAnchor(povIntimateAnchor, 3.0f, 4.0f);
            if (aureliaRig != null)
            {
                aureliaRig.SetExpression(CharacterEmotion.Loving);
                aureliaRig.SetTalking(true);
            }

            SetSubtitle("Aurelia", "Dù thiên đàng này đã vỡ vụn... ta vẫn luôn muốn cùng ngươi thề nguyện dưới bầu trời này.",
                "Though this paradise lies fractured... I still long to share this sacred vow with you.");

            yield return WaitOrSkip(3.8f);
            if (aureliaRig != null) aureliaRig.SetTalking(false);
            if (isSkipping) goto FinishCinematic;

            // Shot 4: Event CG Climactic Reveal
            if (fullScreenCGImage != null && cgSprite != null)
            {
                fullScreenCGImage.sprite = cgSprite;
                fullScreenCGImage.gameObject.SetActive(true);
            }

            SetSubtitle("Aurelia", "Hãy để lời thề này được khắc ghi mãi mãi giữa các vì sao.",
                "Let this sacred vow be etched forever across the boundless stars.");

            yield return WaitOrSkip(3.5f);

        FinishCinematic:
            if (GalleryManager.Instance != null && !string.IsNullOrEmpty(sceneId))
            {
                GalleryManager.Instance.UnlockScene(sceneId);
            }

            if (fullScreenCGImage != null)
            {
                fullScreenCGImage.gameObject.SetActive(false);
            }

            if (aureliaRig != null && !aureliaRig.gameObject.activeInHierarchy)
            {
                aureliaRig.gameObject.SetActive(true);
            }

            // Restore POV Camera
            if (cameraController != null && povWideAnchor != null)
            {
                cameraController.SetTarget(povWideAnchor);
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
            if (cameraController != null && anchor != null)
            {
                cameraController.SetTarget(anchor);
                cameraController.SetSpeed(speed);
            }

            var cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = orthoSize;
            }
        }

        private void SetSubtitle(string speaker, string viText, string enText)
        {
            bool isVi = LocalizationManager.Instance == null || LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.Vietnamese;

            if (speakerText != null) speakerText.text = speaker;
            if (subtitleText != null) subtitleText.text = isVi ? viText : enText;
        }

        private IEnumerator WaitOrSkip(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds && !isSkipping)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
