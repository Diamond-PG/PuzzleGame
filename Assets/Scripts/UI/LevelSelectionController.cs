using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LevelSelectionController : MonoBehaviour
{
    private const int PageSize = 10;

    [Serializable]
    private sealed class Cell
    {
        public Button button;
        public Image frame;
        public TMP_Text number;
        public Image[] stars;
        public GameObject lockOverlay;
        public UIHapticsOnPress haptics;
        public UIClickSfx sound;
    }

    [Header("View")]
    [SerializeField] private Cell[] cells;
    [SerializeField] private TMP_Text pageCounter;
    [SerializeField] private Button previousPage;
    [SerializeField] private Button nextPage;
    [SerializeField] private LevelHighlight levelHighlight;
    [SerializeField, Range(0, 1)] private float unavailableArrowAlpha = 0.5f;

    [Header("Levels (one full scene path per level, in order)")]
    [SerializeField] private string[] scenePaths;

    [Header("Stars")]
    [SerializeField] private Sprite starOutline;
    [SerializeField] private Sprite earnedStar;

    [Header("Locked style")]
    [SerializeField] private Material lockedImageMaterial;
    [SerializeField] private Material lockedNumberMaterial;
    [SerializeField] private Color lockedFrameColor;
    [SerializeField] private Color lockedNumberColor;
    [SerializeField] private Color lockedStarColor;

    [Header("Unlocked style")]
    [SerializeField] private Material unlockedImageMaterial;
    [SerializeField] private Material unlockedNumberMaterial;
    [SerializeField] private Color unlockedFrameColor = Color.white;
    [SerializeField] private Color unlockedNumberColor = Color.white;
    [SerializeField] private Color unlockedStarColor = Color.white;
    [SerializeField] private VertexGradient unlockedNumberGradient;

    private int page;

#if UNITY_EDITOR
    private readonly System.Collections.Generic.Dictionary<int, int> testStars = new();
    public bool LevelsTestModeEnabled { get; private set; }
    public bool LevelsTestGameStarted { get; private set; }
    public int LevelsTestLevelCount => Mathf.Max(PageSize, scenePaths.Length);

    public void SetLevelsTestMode(bool enabled)
    {
        if (!Application.isPlaying || LevelsTestModeEnabled == enabled)
            return;

        LevelsTestModeEnabled = enabled;
        ResetLevelsTest();
    }

    public void SetLevelsTestStars(int level, int stars)
    {
        if (!Application.isPlaying || !LevelsTestModeEnabled || level < 1 || level > LevelsTestLevelCount)
            return;

        testStars[level] = Mathf.Clamp(stars, 0, 3);
        if (isActiveAndEnabled)
            Refresh();
    }

    public void SetLevelsTestGameStarted(bool started)
    {
        if (!Application.isPlaying || !LevelsTestModeEnabled)
            return;

        LevelsTestGameStarted = started;
        if (isActiveAndEnabled)
            Refresh();
    }

    public void ResetLevelsTest()
    {
        if (!Application.isPlaying)
            return;

        testStars.Clear();
        LevelsTestGameStarted = false;
        if (isActiveAndEnabled)
            Refresh();
    }
#endif

    private void Awake()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            int slot = i;
            cells[i].button.onClick.AddListener(() => RequestLevel(slot));
        }

        previousPage.onClick.AddListener(PreviousPage);
        nextPage.onClick.AddListener(NextPage);
    }

    private void OnEnable()
    {
        LevelProgressSave.BestStarsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        LevelProgressSave.BestStarsChanged -= Refresh;
        if (levelHighlight != null)
            levelHighlight.Show(null);
    }

    private void Refresh()
    {
        bool started = HasProgress();
        int lastPage = GetLastExistingPage();
        page = Mathf.Min(page, lastPage);
        while (page > 0 && !CanAccessPage(page, started))
            page--;

        for (int i = 0; i < cells.Length; i++)
        {
            int level = page * PageSize + i + 1;
            bool unlocked = IsUnlocked(level, started);
            int stars = started ? GetStars(level) : 0;
            RenderCell(cells[i], level, unlocked, stars, SceneExists(level) &&
                LevelLaunchSession.SupportsReplay(level, scenePaths[level - 1]));
        }

        pageCounter.text = (page + 1).ToString(CultureInfo.InvariantCulture) + " / " +
            (lastPage == 0 ? "—" : (lastPage + 1).ToString(CultureInfo.InvariantCulture));

        previousPage.gameObject.SetActive(page > 0);
        ApplyArrow(previousPage, page > 0);
        ApplyArrow(nextPage, CanAccessPage(page + 1, started));

        if (levelHighlight != null)
        {
            int slot = FindHighlightSlot(started);
            levelHighlight.Show(slot >= 0 ? cells[slot].frame.rectTransform : null);
        }
    }

    private bool HasProgress()
    {
#if UNITY_EDITOR
        if (LevelsTestModeEnabled)
            return LevelsTestGameStarted;
#endif
        return GameInstallationSave.HasCurrentInstallSave();
    }

    private int GetStars(int level)
    {
#if UNITY_EDITOR
        if (LevelsTestModeEnabled)
            return testStars.TryGetValue(level, out int stars) ? stars : 0;
#endif
        return LevelProgressSave.GetBestStars(level);
    }

    private bool IsUnlocked(int level, bool started)
    {
        return LevelProgressSave.IsLevelUnlocked(level, started, GetStars);
    }

    private void RenderCell(Cell cell, int level, bool unlocked, int result, bool sceneExists)
    {
        cell.frame.material = unlocked ? unlockedImageMaterial : lockedImageMaterial;
        cell.frame.color = unlocked ? unlockedFrameColor : lockedFrameColor;
        cell.number.text = level.ToString(CultureInfo.InvariantCulture);
        cell.number.fontSharedMaterial = unlocked ? unlockedNumberMaterial : lockedNumberMaterial;
        cell.number.color = unlocked ? unlockedNumberColor : lockedNumberColor;
        cell.number.enableVertexGradient = unlocked;
        cell.number.colorGradient = unlockedNumberGradient;
        cell.number.fontStyle |= FontStyles.Bold;

        for (int i = 0; i < cell.stars.Length; i++)
        {
            var star = cell.stars[i];
            star.sprite = i < result ? earnedStar : starOutline;
            star.material = cell.frame.material;
            star.color = unlocked ? unlockedStarColor : lockedStarColor;
        }

        cell.lockOverlay.SetActive(!unlocked);
        bool canInteract = unlocked && sceneExists;
        cell.haptics.enabled = canInteract;
        cell.button.interactable = canInteract;
        cell.frame.raycastTarget = canInteract;
    }

    private void ApplyArrow(Button button, bool available)
    {
        button.interactable = available;
        var graphic = button.targetGraphic;
        var color = graphic.color;
        color.a = available ? 1f : unavailableArrowAlpha;
        graphic.color = color;
        graphic.raycastTarget = available;
        button.GetComponent<CanvasGroup>().blocksRaycasts = available;
        button.GetComponent<UIHapticsOnPress>().enabled = available;
    }

    private void PreviousPage()
    {
        TurnPage(previousPage, -1);
    }

    private void NextPage()
    {
        TurnPage(nextPage, 1);
    }

    private void TurnPage(Button arrow, int direction)
    {
        if (!arrow.isActiveAndEnabled || !arrow.IsInteractable())
            return;

        if (TryTurnPage(ref page, direction,
                candidate => CanAccessPage(candidate, HasProgress()),
                () =>
                {
                    arrow.GetComponent<UIClickSfx>().PlayClick();
                    arrow.GetComponent<UIHapticsOnPress>().PlayHaptic();
                }))
            Refresh();
    }

    internal static bool TryTurnPage(ref int page, int direction,
        Func<int, bool> canAccessNext, Action playFeedback)
    {
        if ((direction != -1 && direction != 1) || page + direction < 0 ||
            (direction == 1 && !canAccessNext(page + direction)))
            return false;

        page += direction;
        playFeedback();
        return true;
    }

    private void RequestLevel(int slot)
    {
#if UNITY_EDITOR
        if (LevelsTestModeEnabled)
            return;
#endif
        int level = page * PageSize + slot + 1;
        if (cells[slot].button.IsInteractable() && LevelLaunchSession.TryStartReplay(level, scenePaths))
            cells[slot].sound.PlayClick();
    }

    private int GetLastExistingPage()
    {
        for (int i = scenePaths.Length; i > 0; i--)
        {
            if (SceneExists(i))
                return (i - 1) / PageSize;
        }

        return 0;
    }

    private bool SceneExists(int level)
    {
        if (level < 1 || level > scenePaths.Length || string.IsNullOrWhiteSpace(scenePaths[level - 1]))
            return false;

        return LevelLaunchSession.GetLoadableSceneIndex(scenePaths[level - 1]) >= 0;
    }

    private int FindHighlightSlot(bool started)
    {
        for (int level = scenePaths.Length; level >= 1; level--)
        {
            if (!SceneExists(level) || !IsUnlocked(level, started))
                continue;

            int candidatePage = (level - 1) / PageSize;
            if (!CanAccessPage(candidatePage, started))
                continue;

            return candidatePage == page ? (level - 1) % PageSize : -1;
        }

        return -1;
    }

    private bool CanAccessPage(int requestedPage, bool started)
    {
        if (requestedPage == 0)
            return true;
        if (requestedPage < 0 || !IsUnlocked(requestedPage * PageSize + 1, started))
            return false;
        int firstLevel = requestedPage * PageSize + 1;
        for (int offset = 0; offset < PageSize; offset++)
        {
            if (SceneExists(firstLevel + offset))
                return true;
        }

        return false;
    }
}
