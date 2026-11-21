
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GalleryController : MonoBehaviour
{
    [Header("Page Settings")]
    [SerializeField] private int currentPage = 1;
    [SerializeField] private int maxPages = 100;

    [Header("UI References")]
    [SerializeField] private TMP_Text pageCounterText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Puzzle Display")]
    [SerializeField] private Image puzzleDisplay;

    [Header("Gallery Images")]
    [SerializeField] private Sprite[] galleryImages;

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

    private int currentCollectionPicture = 1;

    private void Awake()
    {
        maxPages = Mathf.Max(1, maxPages);
        currentPage = Mathf.Clamp(currentPage, 1, maxPages);

        ShowSectionSelection();
        UpdateGalleryUI();
    }

    // Opens the gallery at the section selection screen.
    public void OpenGallery()
    {
        ShowSectionSelection();

        if (galleryPanel != null)
            galleryPanel.SetActive(true);

        if (menuButtonsRoot != null)
            menuButtonsRoot.SetActive(false);

        UpdateGalleryUI();
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

        galleryCollectionView.SetActive(true);

        SetCollectionPicture(currentCollectionPicture);
        UpdateGalleryUI();
    }

    public void SetCollectionPicture(int pictureNumber)
    {
        int pictureCount = Mathf.Max(1, collectionPictureCount);
        currentCollectionPicture = Mathf.Clamp(pictureNumber, 1, pictureCount);

        if (collectionPageCounterText != null)
            collectionPageCounterText.SetText($"{currentCollectionPicture}/{pictureCount}");
    }

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

    public void PreviousPage()
    {
        if (currentPage <= 1)
            return;

        currentPage--;
        UpdateGalleryUI();
    }

    public void NextPage()
    {
        if (currentPage >= maxPages)
            return;

        currentPage++;
        UpdateGalleryUI();
    }

    private void UpdateGalleryUI()
    {
        if (pageCounterText != null)
            pageCounterText.text = currentPage + " / —";

        if (previousButton != null)
            previousButton.interactable = currentPage > 1;

        if (nextButton != null)
            nextButton.interactable = currentPage < maxPages;

        UpdatePuzzleDisplay();
    }

    private void UpdatePuzzleDisplay()
    {
        if (puzzleDisplay == null)
            return;

        int imageIndex = currentPage - 1;

        if (galleryImages != null &&
            imageIndex >= 0 &&
            imageIndex < galleryImages.Length &&
            galleryImages[imageIndex] != null)
        {
            puzzleDisplay.sprite = galleryImages[imageIndex];
            puzzleDisplay.enabled = true;
            puzzleDisplay.preserveAspect = true;
        }
        else
        {
            puzzleDisplay.sprite = null;
            puzzleDisplay.enabled = false;
        }
    }
}
