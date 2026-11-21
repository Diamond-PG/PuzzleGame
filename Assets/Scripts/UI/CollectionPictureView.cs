using System.Collections;
using UnityEngine;

namespace Bakvand.UI
{
    public sealed class CollectionPictureView : MonoBehaviour
    {
        private const int PieceCount = 4;
        private const int CompleteMask = (1 << PieceCount) - 1;
        private const int CompletionPending = 1 << PieceCount;
        private static readonly int JoinedEdges = Shader.PropertyToID("_JoinedEdges");
        private static readonly int SeamOpacity = Shader.PropertyToID("_SeamOpacity");
        private static readonly int SeamThickness = Shader.PropertyToID("_SeamThickness");
        private static readonly int CompletionLight = Shader.PropertyToID("_CompletionLight");
        private static readonly int FinishingLight = Shader.PropertyToID("_FinishingLight");
        private static readonly int RevealTexture = Shader.PropertyToID("_RevealTexture");
        private static readonly int RevealPose = Shader.PropertyToID("_RevealPose");
        private static readonly int RevealLight = Shader.PropertyToID("_RevealLight");
        private const string RevealKeyword = "COLLECTION_PIECE_REVEAL";
        private const string CompletionKeyword = "COLLECTION_COMPLETION";

        [SerializeField, Min(1)] private int pictureId = 1;
        [SerializeField] private UnityEngine.UI.Image artwork;
        [SerializeField] private UnityEngine.UI.Image[] fragments = new UnityEngine.UI.Image[PieceCount];
        [SerializeField] private UnityEngine.UI.RawImage joinOverlay;

        [Header("Seams")]
        [SerializeField, Range(0.5f, 1.6f)] private float seamThickness = 1f;
        [SerializeField, Range(0f, 1f)] private float seamStrength = 0.74f;

        [Header("Piece Reveal")]
        [SerializeField, Min(0.01f)] private float pieceRevealSeconds = 1.1f;
        [SerializeField, Range(0.75f, 1f)] private float pieceStartScale = 0.92f;
        [SerializeField] private Color pieceRevealTint = new Color(1f, 0.84f, 0.60f);
        [SerializeField, Min(0.01f)] private float pieceFlashSeconds = 0.62f;
        [SerializeField, Range(0.06f, 0.35f)] private float pieceFlashRadius = 0.23f;
        [SerializeField, Range(0f, 0.7f)] private float pieceFlashBrightness = 0.32f;
        [SerializeField, Range(0f, 1f)] private float pieceEdgeGlow = 0.66f;

        [Header("Completion")]
        [SerializeField, Min(0.15f), Tooltip("Total time including the hold, merge and finishing sheen.")]
        private float completionSeconds = 2.65f;
        [SerializeField, Min(0f)] private float joinedHoldSeconds = 0.35f;
        [SerializeField, Range(0.25f, 3f)] private float waveSpeed = 0.95f;
        [SerializeField, Range(0f, 1f)] private float sweepStrength = 0.88f;
        [SerializeField, Range(0f, 0.65f)] private float completionFlashBrightness = 0.42f;
        [SerializeField, Min(0.01f)] private float mergeSeconds = 0.65f;
        [SerializeField, Range(0f, 0.5f)] private float finishSheen = 0.24f;

        [Header("Audio Output")]
        [SerializeField] private AudioSource uiAudioSource;
        [Header("Piece Appear Sound")]
        [SerializeField] private AudioClip pieceAppearClip;
        [SerializeField, Range(0f, 1f)] private float pieceAppearVolume = 1f;
        [Header("Puzzle Completed Sound")]
        [SerializeField] private AudioClip puzzleCompletedClip;
        [SerializeField, Range(0f, 1f)] private float puzzleCompletedVolume = 1f;

        private readonly Color[] originalPieceColors = new Color[PieceCount];
        private Color originalArtworkColor;
        private Material overlayMaterial;
        private Coroutine animation;
        private RectTransform revealingRect;
        private Vector3 revealRestingScale;
        private Vector2 revealRestingPosition;
        private int revealRestingSibling;
        private string PresentationKey => $"Collection_{pictureId}_PresentedMask";

        internal int PictureId => pictureId;
        internal bool IsConfigured => artwork != null && artwork.sprite != null && joinOverlay != null &&
            fragments != null && fragments.Length == PieceCount &&
            System.Array.TrueForAll(fragments, fragment => fragment != null && fragment.sprite != null);

        internal int GetCollectedMask()
        {
            int mask = 0;
            for (int index = 0; index < PieceCount; index++)
                if (PuzzlePiecePickup.IsPieceCollected(pictureId, index + 1))
                    mask |= 1 << index;
            return mask;
        }

#if UNITY_EDITOR
        [System.NonSerialized] private bool testMode;
        [System.NonSerialized] private int testPieces;
        [System.NonSerialized] private bool testFinished;

        public bool CollectionTestModeEnabled => testMode;
        public int CollectionTestPieceCount => testPieces;
        public bool CollectionTestIsAnimating => animation != null;
        public bool CollectionTestSecondPictureUnlocked { get; private set; }
        internal event System.Action CollectionTestNavigationChanged;

        public void SetCollectionTestMode(bool enabled)
        {
            if (!Application.isPlaying || enabled == testMode)
                return;

            StopAnimation();
            testMode = enabled;
            testPieces = 0;
            testFinished = false;
            CollectionTestSecondPictureUnlocked = false;
            if (isActiveAndEnabled)
                RefreshPicture();
            CollectionTestNavigationChanged?.Invoke();
        }

        public void SetCollectionTestSecondPictureUnlocked(bool unlocked)
        {
            if (!Application.isPlaying || !testMode)
                return;

            CollectionTestSecondPictureUnlocked = unlocked;
            CollectionTestNavigationChanged?.Invoke();
        }

        public void ResetCollectionTest()
        {
            ShowCollectionTestPieces(0);
            SetCollectionTestSecondPictureUnlocked(false);
        }

        public void ShowCollectionTestPieces(int count)
        {
            if (!Application.isPlaying || !testMode)
                return;

            StopAnimation();
            testPieces = Mathf.Clamp(count, 0, PieceCount);
            testFinished = false;
            if (isActiveAndEnabled)
                ShowPieces((1 << testPieces) - 1);
        }

        public void PlayCollectionTestNextPiece()
        {
            if (!Application.isPlaying || !testMode || !isActiveAndEnabled || animation != null || testPieces == PieceCount)
                return;

            animation = StartCoroutine(RevealNextTestPiece());
        }

        private IEnumerator RevealNextTestPiece()
        {
            yield return RevealPiece(testPieces, (1 << testPieces) - 1);
            testPieces++;
            ShowPieces((1 << testPieces) - 1);
            animation = null;
        }

        public void PlayCollectionTestCompletion()
        {
            if (!Application.isPlaying || !testMode || !isActiveAndEnabled || animation != null)
                return;

            ShowCollectionTestPieces(PieceCount);
            animation = StartCoroutine(CompleteTestPicture());
        }

        private IEnumerator CompleteTestPicture()
        {
            yield return AnimateCompletion();
            testFinished = true;
            animation = null;
        }
#endif

        private void Awake()
        {
            if (!IsConfigured)
            {
                enabled = false;
                return;
            }

            originalArtworkColor = artwork.color;
            for (int index = 0; index < PieceCount; index++)
                originalPieceColors[index] = fragments[index].color;

            overlayMaterial = new Material(joinOverlay.material);
            joinOverlay.material = overlayMaterial;
        }

        private void OnEnable() => RefreshPicture();

        private void OnDisable() => StopAnimation();

        private void OnDestroy()
        {
            if (overlayMaterial != null)
                Destroy(overlayMaterial);
        }

        private void StopAnimation()
        {
            if (animation != null)
            {
                StopCoroutine(animation);
                animation = null;
            }

            RestoreRevealTransform();
        }

        private void RefreshPicture()
        {
#if UNITY_EDITOR
            if (testMode)
            {
                if (testFinished)
                    ShowFinished();
                else
                    ShowPieces((1 << testPieces) - 1);
                return;
            }
#endif
            int collected = GetCollectedMask();
            int stored = GameInstallationSave.GetCollectionInt(PresentationKey);
            int presented = stored & collected;
            bool completionPending = presented == CompleteMask && (stored & CompletionPending) != 0;
            if (!completionPending && stored != presented)
                StorePresented(presented);

            if (presented == CompleteMask && !completionPending)
            {
                ShowFinished();
                return;
            }

            ShowPieces(presented);
            if (presented != collected || completionPending)
                animation = StartCoroutine(RevealCollected(presented, collected));
        }

        private IEnumerator RevealCollected(int presented, int collected)
        {
            for (int index = 0; index < PieceCount; index++)
            {
                int piece = 1 << index;
                if ((collected & piece) == 0 || (presented & piece) != 0)
                    continue;

                // A started reveal has been seen even if the player changes pages mid-effect.
                int next = presented | piece;
                StorePresented(next == CompleteMask ? next | CompletionPending : next);
                yield return RevealPiece(index, presented);
                presented |= piece;
                ShowPieces(presented);
            }

            if (presented == CompleteMask)
            {
                yield return AnimateCompletion(rememberPresentation: true);
            }

            animation = null;
        }

        private void StorePresented(int mask)
        {
            GameInstallationSave.SetCollectionInt(PresentationKey, mask);
        }

        private IEnumerator RevealPiece(int index, int presented)
        {
            var image = fragments[index];
            Color resting = originalPieceColors[index];
            Vector2 center = new Vector2(index % 2 == 0 ? 0.25f : 0.75f, index < 2 ? 0.75f : 0.25f);
            revealingRect = image.rectTransform;
            revealRestingScale = revealingRect.localScale;
            revealRestingPosition = revealingRect.anchoredPosition;
            revealRestingSibling = revealingRect.GetSiblingIndex();
            Vector2 centerOffset = Vector2.Scale(center - revealingRect.pivot, revealingRect.rect.size);
            centerOffset = revealingRect.localRotation * Vector2.Scale(centerOffset, revealRestingScale);
            revealingRect.SetAsFirstSibling();
            image.color = WithOpacity(resting, 0f);
            image.enabled = true;
            PlaySound(pieceAppearClip, pieceAppearVolume);

            float duration = Mathf.Max(0.01f, pieceRevealSeconds);
            float flashDuration = Mathf.Clamp(pieceFlashSeconds, 0.01f, duration);
            Vector4 previousEdges = GetJoinedEdges(presented);
            Vector4 newEdges = GetJoinedEdges(presented | (1 << index));
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float time = elapsed / duration;
                float progress = Mathf.SmoothStep(0f, 1f, time);
                float scale = Mathf.Lerp(pieceStartScale, 1f, progress);
                revealingRect.localScale = revealRestingScale * scale;
                revealingRect.anchoredPosition = revealRestingPosition + centerOffset * (1f - scale);
                Color fade = Color.Lerp(pieceRevealTint, resting, progress);
                fade.a = resting.a * progress;
                image.color = fade;

                float flashTime = Mathf.Clamp01(elapsed / flashDuration);
                float flash = pieceFlashBrightness * Mathf.SmoothStep(0f, 1f, flashTime / 0.16f)
                    * (1f - Mathf.SmoothStep(0f, 1f, flashTime));
                float rim = pieceEdgeGlow * Mathf.Sin(Mathf.PI * progress);
                Vector4 edges = Vector4.Lerp(previousEdges, newEdges, Mathf.SmoothStep(0f, 1f, (time - 0.82f) / 0.18f));
                var material = ApplyOverlay(edges, seamStrength, default, default, true);
                material.SetTexture(RevealTexture, image.sprite.texture);
                material.SetVector(RevealPose, new Vector4(center.x, center.y, 1f / scale));
                material.SetVector(RevealLight, new Vector4(flash, pieceFlashRadius, rim, progress));
                yield return null;
            }

            RestoreRevealTransform();
            image.color = resting;
        }

        private void RestoreRevealTransform()
        {
            if (revealingRect == null)
                return;

            revealingRect.localScale = revealRestingScale;
            revealingRect.anchoredPosition = revealRestingPosition;
            revealingRect.SetSiblingIndex(revealRestingSibling);
            revealingRect = null;
        }

        private IEnumerator AnimateCompletion(bool rememberPresentation = false)
        {
            float total = Mathf.Max(0.15f, completionSeconds);
            float hold = Mathf.Clamp(joinedHoldSeconds, 0f, total - 0.10f);
            if (hold > 0f)
                yield return new WaitForSecondsRealtime(hold);

            if (rememberPresentation)
                StorePresented(CompleteMask);
            PlaySound(puzzleCompletedClip, puzzleCompletedVolume);
            float duration = total - hold;
            float sheenDuration = duration * 0.19f;
            float mergeDuration = Mathf.Clamp(mergeSeconds, 0.01f, duration * 0.62f);
            float mergeStart = duration - mergeDuration - sheenDuration;
            float flashStart = mergeStart * 0.38f;
            float flashDuration = mergeStart - flashStart + mergeDuration * 0.82f;
            Vector4 joined = GetJoinedEdges(CompleteMask);
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float merge = Mathf.SmoothStep(0f, 1f, (elapsed - mergeStart) / mergeDuration);
                if (elapsed >= mergeStart)
                {
                    artwork.color = WithOpacity(originalArtworkColor, merge);
                    artwork.enabled = true;
                }

                float wave = Mathf.Clamp01(elapsed * waveSpeed);
                float waveCenter = Mathf.Lerp(-0.15f, 1.15f, wave);
                float flashTime = Mathf.Clamp01((elapsed - flashStart) / flashDuration);
                float flash = completionFlashBrightness * Mathf.SmoothStep(0f, 1f, flashTime / 0.3f)
                    * (1f - Mathf.SmoothStep(0f, 1f, (flashTime - 0.32f) / 0.68f));
                float radius = Mathf.Lerp(0.10f, 1.14f, Mathf.SmoothStep(0f, 1f, flashTime));
                float sheenTime = Mathf.Clamp01((elapsed - mergeStart - mergeDuration) / sheenDuration);
                float sheenCenter = Mathf.Lerp(-0.24f, 1.24f, sheenTime);
                float sheen = finishSheen * Mathf.Sin(Mathf.PI * sheenTime);

                ApplyOverlay(joined, seamStrength * (1f - merge),
                    new Vector4(waveCenter, sweepStrength * (1f - merge), radius, flash),
                    new Vector4(sheenCenter, sheen), completing: true);
                yield return null;
            }

            ShowFinished();
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (uiAudioSource != null && clip != null)
                uiAudioSource.PlayOneShot(clip, volume);
        }

        private static Color WithOpacity(Color color, float opacity)
        {
            color.a *= opacity;
            return color;
        }

        private void ShowPieces(int mask)
        {
            SetLayers(mask, false);
            Vector4 edges = GetJoinedEdges(mask);
            if (edges == Vector4.zero || seamStrength <= 0f)
                joinOverlay.enabled = false;
            else
                ApplyOverlay(edges, seamStrength, default, default);
        }

        private void ShowFinished()
        {
            SetLayers(0, true);
            joinOverlay.enabled = false;
        }

        private void SetLayers(int pieces, bool showArtwork)
        {
            artwork.color = originalArtworkColor;
            artwork.enabled = showArtwork;
            for (int index = 0; index < PieceCount; index++)
            {
                fragments[index].color = originalPieceColors[index];
                fragments[index].enabled = (pieces & (1 << index)) != 0;
            }
        }

        private static Vector4 GetJoinedEdges(int pieces)
        {
            // Texture channels are the joins TL-TR, TL-BL, TR-BR, BL-BR.
            return new Vector4(
                (pieces & 3) == 3 ? 1f : 0f,
                (pieces & 5) == 5 ? 1f : 0f,
                (pieces & 10) == 10 ? 1f : 0f,
                (pieces & 12) == 12 ? 1f : 0f);
        }

        private Material ApplyOverlay(Vector4 edges, float opacity, Vector4 completion, Vector4 sheen,
            bool reveal = false, bool completing = false)
        {
            joinOverlay.enabled = true;
            var material = joinOverlay.materialForRendering;
            SetEffectKeyword(material, RevealKeyword, reveal);
            SetEffectKeyword(material, CompletionKeyword, completing);
            material.SetVector(JoinedEdges, edges);
            material.SetFloat(SeamOpacity, opacity);
            material.SetFloat(SeamThickness, seamThickness);
            material.SetVector(CompletionLight, completion);
            material.SetVector(FinishingLight, sheen);
            return material;
        }

        private static void SetEffectKeyword(Material material, string keyword, bool enabled)
        {
            if (material.IsKeywordEnabled(keyword) == enabled)
                return;

            if (enabled)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }
    }
}
