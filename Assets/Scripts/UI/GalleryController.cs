
using Bakvand.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalleryController : MonoBehaviour
{
    [Header("Gallery Open / Close")]
    [SerializeField] private GameObject galleryPanel;
    [SerializeField] private GameObject menuButtonsRoot;

    [Header("Gallery Navigation")]
    [SerializeField] private GameObject galleryArtworkRoot;
    [SerializeField] private GameObject galleryLevelsView;
    [SerializeField] private GameObject galleryCollectionView;

    [Header("Collection Counter")]
    [SerializeField] private TMP_Text collectionPageCounterText;
    [SerializeField, Min(1)] private int collectionPictureCount = 10;

    [Header("Collection Navigation")]
    [SerializeField] private Button collectionPreviousButton;
    [SerializeField] private Button collectionNextButton;
    [Tooltip("Slot 1 uses Picture Id 1. Leave unprepared slots empty.")]
    [SerializeField] private CollectionPictureView[] collectionPictures;
    [SerializeField, Range(0f, 1f)] private float collectionDisabledBrightness = 0.65f;
    [SerializeField, Range(0f, 1f)] private float collectionDisabledOpacity = 0.86f;

    private int currentCollectionPicture = 1;

#if UNITY_EDITOR
    private GameObject collectionTestPage;
#endif

    private void Awake()
    {
        if (collectionPreviousButton != null)
            collectionPreviousButton.onClick.AddListener(PreviousCollectionPicture);
        if (collectionNextButton != null)
            collectionNextButton.onClick.AddListener(NextCollectionPicture);

#if UNITY_EDITOR
        if (GetCollectionView(1) is { } first)
            first.CollectionTestNavigationChanged += RefreshCollection;
#endif

        ShowSectionSelection();
    }

    // Opens the gallery at the section selection screen.
    public void OpenGallery()
    {
        ShowSectionSelection();

        if (galleryPanel != null)
            galleryPanel.SetActive(true);

        if (menuButtonsRoot != null)
            menuButtonsRoot.SetActive(false);
    }

    // Closes the entire gallery and returns to the main menu.
    public void CloseGallery()
    {
        if (galleryPanel != null)
            galleryPanel.SetActive(false);

        if (menuButtonsRoot != null)
            menuButtonsRoot.SetActive(true);

        ShowSectionSelection();
    }

    // Opens the level selection screen.
    public void OpenLevelsView()
    {
        if (galleryLevelsView == null)
        {
            Debug.LogWarning(
                "GalleryController: Gallery Levels View is not assigned.");
            return;
        }

        if (galleryArtworkRoot != null)
            galleryArtworkRoot.SetActive(false);

        galleryLevelsView.SetActive(true);

        if (galleryCollectionView != null)
            galleryCollectionView.SetActive(false);
    }

    // Opens the collection screen.
    public void OpenCollectionView()
    {
        if (galleryCollectionView == null)
        {
            Debug.LogWarning(
                "GalleryController: Gallery Collection View is not assigned.");
            return;
        }

        if (galleryArtworkRoot != null)
            galleryArtworkRoot.SetActive(false);

        if (galleryLevelsView != null)
            galleryLevelsView.SetActive(false);

        RefreshCollection();
        galleryCollectionView.SetActive(true);
    }

    public void SetCollectionPicture(int pictureNumber)
    {
        if (!IsCollectionPictureAvailable(pictureNumber))
            return;

        currentCollectionPicture = pictureNumber;
        RefreshCollection();
    }

    private void PreviousCollectionPicture() => TurnCollectionPage(FindPreviousCollectionPicture(), collectionPreviousButton);
    private void NextCollectionPicture() => TurnCollectionPage(currentCollectionPicture + 1, collectionNextButton);

    private void TurnCollectionPage(int pictureNumber, Button button)
    {
        if (!button.isActiveAndEnabled || !button.IsInteractable() || !IsCollectionPictureAvailable(pictureNumber))
            return;

        button.GetComponent<UIClickSfx>()?.PlayClick();
        button.GetComponent<UIHapticsOnPress>()?.PlayHaptic();
        SetCollectionPicture(pictureNumber);
    }

    private int FindPreviousCollectionPicture()
    {
        for (int number = currentCollectionPicture - 1; number >= 1; number--)
            if (IsCollectionPictureAvailable(number))
                return number;
        return 0;
    }

    private CollectionPictureView GetCollectionView(int number)
    {
        if (collectionPictures == null || number < 1 || number > collectionPictures.Length)
            return null;

        var view = collectionPictures[number - 1];
        return view != null && view.PictureId == number && view.IsConfigured ? view : null;
    }

    private bool IsCollectionPictureAvailable(int number)
    {
        if (number < 1 || number > collectionPictureCount)
            return false;
        if (number == 1)
            return true;

#if UNITY_EDITOR
        if (GetCollectionView(1) is { CollectionTestModeEnabled: true } first)
            return number == 2 && first.CollectionTestSecondPictureUnlocked;
#endif
        var picture = GetCollectionView(number);
        return picture != null && picture.GetCollectedMask() != 0;
    }

    private void RefreshCollection()
    {
        if (!IsCollectionPictureAvailable(currentCollectionPicture))
            currentCollectionPicture = 1;

        var selected = GetCollectionView(currentCollectionPicture);
#if UNITY_EDITOR
        bool previewSecond = currentCollectionPicture == 2 &&
            GetCollectionView(1) is { CollectionTestModeEnabled: true };
        if (previewSecond)
            selected = null;
        ShowCollectionTestPage(previewSecond);
#endif

        if (collectionPictures != null)
            foreach (var picture in collectionPictures)
                if (picture != null && picture != selected)
                    picture.gameObject.SetActive(false);
        if (selected != null)
            selected.gameObject.SetActive(true);

        int previous = FindPreviousCollectionPicture();
        SetCollectionArrow(collectionPreviousButton, previous != 0, previous != 0);
        SetCollectionArrow(collectionNextButton, true, IsCollectionPictureAvailable(currentCollectionPicture + 1));
        if (collectionPageCounterText != null)
            collectionPageCounterText.SetText($"{currentCollectionPicture}/{Mathf.Max(1, collectionPictureCount)}");
    }

    private void SetCollectionArrow(Button button, bool visible, bool available)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(visible);
        button.interactable = available;
        var graphic = button.targetGraphic;
        graphic.raycastTarget = available;
        graphic.color = available ? Color.white : new Color(
            collectionDisabledBrightness, collectionDisabledBrightness, collectionDisabledBrightness,
            collectionDisabledOpacity);
        if (button.TryGetComponent<CanvasGroup>(out var group))
            group.blocksRaycasts = available;
        if (button.TryGetComponent<UIHapticsOnPress>(out var haptics))
            haptics.enabled = available;
    }

#if UNITY_EDITOR
    private void ShowCollectionTestPage(bool visible)
    {
        if (visible && collectionTestPage == null)
        {
            collectionTestPage = new GameObject("Collection Page 2 (Editor Preview)", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)collectionTestPage.transform;
            rect.SetParent(galleryCollectionView.transform, false);
            rect.sizeDelta = new Vector2(300f, 170f);
            var image = collectionTestPage.GetComponent<Image>();
            image.color = new Color(0.15f, 0.30f, 0.33f);
            image.raycastTarget = false;

            var label = Instantiate(collectionPageCounterText, rect);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.localScale = Vector3.one;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.SetText("1/4");
        }

        if (collectionTestPage != null)
            collectionTestPage.SetActive(visible);
    }
#endif

    // Returns from Levels or Collection to Section Selection.
    public void BackToSectionSelection()
    {
        ShowSectionSelection();
    }

    private void ShowSectionSelection()
    {
        if (galleryLevelsView != null)
            galleryLevelsView.SetActive(false);

        if (galleryCollectionView != null)
            galleryCollectionView.SetActive(false);

        if (galleryArtworkRoot != null)
            galleryArtworkRoot.SetActive(true);
    }

}
