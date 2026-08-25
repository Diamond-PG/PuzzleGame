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

    private void Awake()
    {
        currentPage = Mathf.Clamp(currentPage, 1, maxPages);
        UpdateGalleryUI();
    }

    public void OpenGallery()
    {
        if (galleryPanel != null)
            galleryPanel.SetActive(true);

        if (menuButtonsRoot != null)
            menuButtonsRoot.SetActive(false);

        UpdateGalleryUI();
    }

    public void CloseGallery()
    {
        if (galleryPanel != null)
            galleryPanel.SetActive(false);

        if (menuButtonsRoot != null)
            menuButtonsRoot.SetActive(true);
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