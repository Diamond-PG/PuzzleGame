using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Heart Images (ВАЖНО: слева направо!)")]
    [SerializeField] private Image[] heartImages;

    [Header("Bonus Hearts")]
    [SerializeField] private GameObject bonusHeartBadge;
    [SerializeField] private TMP_Text bonusHeartsText;

    [Header("Bonus Badge Pop Animation")]
    [SerializeField] private bool useBonusBadgePop = true;
    [SerializeField] private float popScale = 1.15f;
    [SerializeField] private float popDuration = 0.16f;

    [Header("Blink Heart")]
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float blinkInterval = 0.25f;

    private int shownHp;
    private int bonusHearts;
    private Coroutine blinkRoutine;
    private Coroutine bonusPopRoutine;
    private bool isBlinking;

    private Vector3 bonusBadgeOriginalScale = Vector3.one;

    private void Awake()
    {
        if (heartImages == null || heartImages.Length == 0)
            heartImages = GetComponentsInChildren<Image>(true);

        if (bonusHeartBadge != null)
            bonusBadgeOriginalScale = bonusHeartBadge.transform.localScale;

        shownHp = heartImages != null ? heartImages.Length : 0;

        Draw(shownHp);
        SetBonusHearts(0, false);
    }

    public void SetHearts(int hp)
    {
        if (isBlinking) return;

        shownHp = Mathf.Clamp(hp, 0, heartImages.Length);
        Draw(shownHp);
    }

    public void SetBonusHearts(int amount)
    {
        SetBonusHearts(amount, true);
    }

    private void SetBonusHearts(int amount, bool playPop)
    {
        int previousBonusHearts = bonusHearts;
        bonusHearts = Mathf.Max(0, amount);

        if (bonusHeartBadge != null)
            bonusHeartBadge.SetActive(bonusHearts > 0);

        if (bonusHeartsText != null)
            bonusHeartsText.text = "x" + bonusHearts;

        if (playPop && useBonusBadgePop && bonusHearts > 0 && bonusHearts > previousBonusHearts)
            PlayBonusBadgePop();
    }

    public void AddBonusHeart()
    {
        SetBonusHearts(bonusHearts + 1, true);
    }

    public bool TryUseBonusHeart()
    {
        if (bonusHearts <= 0)
            return false;

        SetBonusHearts(bonusHearts - 1, false);
        return true;
    }

    private void PlayBonusBadgePop()
    {
        if (bonusHeartBadge == null)
            return;

        if (bonusPopRoutine != null)
            StopCoroutine(bonusPopRoutine);

        bonusPopRoutine = StartCoroutine(BonusBadgePopRoutine());
    }

    private IEnumerator BonusBadgePopRoutine()
    {
        Transform badgeTransform = bonusHeartBadge.transform;

        badgeTransform.localScale = bonusBadgeOriginalScale;

        float halfDuration = popDuration * 0.5f;
        float timer = 0f;

        Vector3 bigScale = bonusBadgeOriginalScale * popScale;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            badgeTransform.localScale = Vector3.Lerp(bonusBadgeOriginalScale, bigScale, smoothT);
            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            badgeTransform.localScale = Vector3.Lerp(bigScale, bonusBadgeOriginalScale, smoothT);
            yield return null;
        }

        badgeTransform.localScale = bonusBadgeOriginalScale;
        bonusPopRoutine = null;
    }

    public void BlinkAndHide(int lostIndex)
    {
        if (heartImages == null || heartImages.Length == 0) return;
        if (lostIndex < 0 || lostIndex >= heartImages.Length) return;

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkAndHideRoutine(lostIndex));
    }

    private IEnumerator BlinkAndHideRoutine(int index)
    {
        isBlinking = true;

        Image img = heartImages[index];
        if (img == null)
        {
            isBlinking = false;
            yield break;
        }

        Draw(shownHp);
        SetAlpha(img, 1f);

        for (int i = 0; i < blinkCount; i++)
        {
            SetAlpha(img, 0f);
            yield return new WaitForSeconds(blinkInterval);

            SetAlpha(img, 1f);
            yield return new WaitForSeconds(blinkInterval);
        }

        SetAlpha(img, 0f);

        isBlinking = false;
        blinkRoutine = null;
    }

    private void Draw(int hp)
    {
        if (heartImages == null) return;

        hp = Mathf.Clamp(hp, 0, heartImages.Length);

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            SetAlpha(heartImages[i], i < hp ? 1f : 0f);
        }
    }

    private void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}