using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Heart Images (ВАЖНО: слева направо!)")]
    [SerializeField] private Image[] heartImages;

    [Header("Hearts Container")]
    [SerializeField] private RectTransform heartsContainer;

    [Header("Bonus Hearts")]
    [SerializeField] private GameObject bonusHeartBadge;
    [SerializeField] private TMP_Text bonusHeartsText;

    [Header("Pickup Pop Animation")]
    [SerializeField] private bool useBonusBadgePop = true;

    [Tooltip("Насколько увеличивается рамка x1")]
    [SerializeField] private float badgePopScale = 1.15f;

    [Tooltip("Насколько увеличиваются обычные сердечки")]
    [SerializeField] private float heartsPopScale = 1.10f;

    [Tooltip("Общая длительность увеличения и возврата")]
    [SerializeField] private float popDuration = 0.16f;

    [Header("Blink Heart")]
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkInterval = 0.25f;

    private int shownHp;
    private int bonusHearts;

    private Coroutine blinkRoutine;
    private Coroutine pickupPopRoutine;

    private bool isBlinking;

    private Vector3 bonusBadgeOriginalScale = Vector3.one;
    private Vector3 heartsContainerOriginalScale = Vector3.one;

    public int BonusHearts => bonusHearts;

    private void Awake()
    {
        if (heartImages == null || heartImages.Length == 0)
            heartImages = GetComponentsInChildren<Image>(true);

        if (heartsContainer == null)
        {
            Transform containerTransform =
                transform.Find("HeartsContainer");

            if (containerTransform != null)
            {
                heartsContainer =
                    containerTransform.GetComponent<RectTransform>();
            }
        }

        if (bonusHeartBadge != null)
        {
            bonusBadgeOriginalScale =
                bonusHeartBadge.transform.localScale;
        }

        if (heartsContainer != null)
        {
            heartsContainerOriginalScale =
                heartsContainer.localScale;
        }

        shownHp =
            heartImages != null
                ? heartImages.Length
                : 0;

        Draw(shownHp);
        SetBonusHearts(0, false);
    }

    public void SetHearts(int hp)
    {
        if (heartImages == null)
            return;

        shownHp = Mathf.Clamp(
            hp,
            0,
            heartImages.Length
        );

        if (isBlinking)
            return;

        Draw(shownHp);
    }

    public void SetBonusHearts(int amount)
    {
        SetBonusHearts(amount, true);
    }

    private void SetBonusHearts(
        int amount,
        bool playPop
    )
    {
        int previousBonusHearts =
            bonusHearts;

        bonusHearts =
            Mathf.Max(0, amount);

        if (bonusHeartBadge != null)
        {
            bonusHeartBadge.SetActive(
                bonusHearts > 0
            );
        }

        if (bonusHeartsText != null)
        {
            bonusHeartsText.text =
                "x" + bonusHearts;
        }

        bool bonusWasAdded =
            bonusHearts > 0 &&
            bonusHearts > previousBonusHearts;

        if (playPop &&
            useBonusBadgePop &&
            bonusWasAdded)
        {
            PlayBonusPickupPop();
        }
    }

    public void AddBonusHeart()
    {
        SetBonusHearts(
            bonusHearts + 1,
            true
        );
    }

    public bool TryUseBonusHeart()
    {
        if (bonusHearts <= 0)
            return false;

        SetBonusHearts(
            bonusHearts - 1,
            false
        );

        return true;
    }

    public void PlayRegularHeartPickupPop()
    {
        PlayPickupPop(false);
    }

    private void PlayBonusPickupPop()
    {
        PlayPickupPop(true);
    }

    private void PlayPickupPop(
        bool includeBonusBadge
    )
    {
        if (heartsContainer == null &&
            (!includeBonusBadge ||
             bonusHeartBadge == null))
        {
            return;
        }

        if (pickupPopRoutine != null)
            StopCoroutine(pickupPopRoutine);

        ResetPopScales();

        pickupPopRoutine = StartCoroutine(
            PickupPopRoutine(includeBonusBadge)
        );
    }

    private IEnumerator PickupPopRoutine(
        bool includeBonusBadge
    )
    {
        float safeDuration =
            Mathf.Max(0.02f, popDuration);

        float halfDuration =
            safeDuration * 0.5f;

        Vector3 badgeBigScale =
            bonusBadgeOriginalScale *
            Mathf.Max(1f, badgePopScale);

        Vector3 heartsBigScale =
            heartsContainerOriginalScale *
            Mathf.Max(1f, heartsPopScale);

        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / halfDuration
            );

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            if (includeBonusBadge &&
                bonusHeartBadge != null)
            {
                bonusHeartBadge.transform.localScale =
                    Vector3.Lerp(
                        bonusBadgeOriginalScale,
                        badgeBigScale,
                        smoothT
                    );
            }

            if (heartsContainer != null)
            {
                heartsContainer.localScale =
                    Vector3.Lerp(
                        heartsContainerOriginalScale,
                        heartsBigScale,
                        smoothT
                    );
            }

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / halfDuration
            );

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            if (includeBonusBadge &&
                bonusHeartBadge != null)
            {
                bonusHeartBadge.transform.localScale =
                    Vector3.Lerp(
                        badgeBigScale,
                        bonusBadgeOriginalScale,
                        smoothT
                    );
            }

            if (heartsContainer != null)
            {
                heartsContainer.localScale =
                    Vector3.Lerp(
                        heartsBigScale,
                        heartsContainerOriginalScale,
                        smoothT
                    );
            }

            yield return null;
        }

        ResetPopScales();
        pickupPopRoutine = null;
    }

    private void ResetPopScales()
    {
        if (bonusHeartBadge != null)
        {
            bonusHeartBadge.transform.localScale =
                bonusBadgeOriginalScale;
        }

        if (heartsContainer != null)
        {
            heartsContainer.localScale =
                heartsContainerOriginalScale;
        }
    }

    public void BlinkAndHide(int lostIndex)
    {
        BlinkAndHideMultiple(lostIndex, 1);
    }

    public void BlinkAndHideMultiple(
        int firstLostIndex,
        int lostHeartCount
    )
    {
        if (heartImages == null ||
            heartImages.Length == 0)
        {
            return;
        }

        if (lostHeartCount <= 0)
            return;

        int safeFirstIndex =
            Mathf.Clamp(
                firstLostIndex,
                0,
                heartImages.Length - 1
            );

        int safeLostCount =
            Mathf.Clamp(
                lostHeartCount,
                1,
                heartImages.Length - safeFirstIndex
            );

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
            isBlinking = false;
        }

        blinkRoutine = StartCoroutine(
            BlinkAndHideMultipleRoutine(
                safeFirstIndex,
                safeLostCount
            )
        );
    }

    private IEnumerator BlinkAndHideMultipleRoutine(
        int firstIndex,
        int heartCount
    )
    {
        isBlinking = true;

        Draw(shownHp);

        for (int i = 0; i < heartCount; i++)
        {
            int index = firstIndex + i;

            if (index < 0 ||
                index >= heartImages.Length)
            {
                continue;
            }

            Image image = heartImages[index];

            if (image != null)
                SetAlpha(image, 1f);
        }

        for (int blink = 0;
             blink < blinkCount;
             blink++)
        {
            SetLostHeartsAlpha(
                firstIndex,
                heartCount,
                0f
            );

            yield return new WaitForSeconds(
                blinkInterval
            );

            SetLostHeartsAlpha(
                firstIndex,
                heartCount,
                1f
            );

            yield return new WaitForSeconds(
                blinkInterval
            );
        }

        Draw(shownHp);

        isBlinking = false;
        blinkRoutine = null;
    }

    private void SetLostHeartsAlpha(
        int firstIndex,
        int heartCount,
        float alpha
    )
    {
        for (int i = 0; i < heartCount; i++)
        {
            int index = firstIndex + i;

            if (index < 0 ||
                index >= heartImages.Length)
            {
                continue;
            }

            Image image = heartImages[index];

            if (image != null)
                SetAlpha(image, alpha);
        }
    }

    private void Draw(int hp)
    {
        if (heartImages == null)
            return;

        hp = Mathf.Clamp(
            hp,
            0,
            heartImages.Length
        );

        int hiddenHeartCount =
            heartImages.Length - hp;

        for (int i = 0;
             i < heartImages.Length;
             i++)
        {
            if (heartImages[i] == null)
                continue;

            bool shouldBeVisible =
                i >= hiddenHeartCount;

            SetAlpha(
                heartImages[i],
                shouldBeVisible ? 1f : 0f
            );
        }
    }

    private void SetAlpha(
        Image image,
        float alpha
    )
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (pickupPopRoutine != null)
        {
            StopCoroutine(pickupPopRoutine);
            pickupPopRoutine = null;
        }

        isBlinking = false;

        Draw(shownHp);
        ResetPopScales();
    }
}