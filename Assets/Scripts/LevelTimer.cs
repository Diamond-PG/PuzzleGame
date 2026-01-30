using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timeText;

    [Header("Countdown")]
    [SerializeField] private float startSeconds = 60f;

    [Header("Start behavior")]
    [SerializeField] private bool startOnlyAfterFirstMove = true;

    private float timeLeft;
    private bool running;
    private bool startedOnce;

    // Важно: если остановили таймер вручную (победа) — он больше никогда не должен перезапустить уровень
    private bool stoppedManually;

    private void Start()
    {
        timeLeft = startSeconds;

        // Если нужно стартовать только после первого движения — стоим.
        running = !startOnlyAfterFirstMove;

        UpdateUI();
    }

    private void Update()
    {
        // Сначала проверяем “победную” блокировку
        if (stoppedManually) return;

        if (!running) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateUI();
            OnTimeUp();
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timeText == null) return;

        int total = Mathf.CeilToInt(timeLeft);
        int minutes = total / 60;
        int seconds = total % 60;

        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    // === ВАЖНО: вызываем это при первом движении игрока ===
    public void NotifyPlayerMoved()
    {
        // Если уже победили — не даём таймеру “ожить”
        if (stoppedManually) return;

        if (!startOnlyAfterFirstMove) return;
        if (startedOnce) return;

        startedOnce = true;
        running = true;
    }

    // === ОСТАНОВИТЬ ТАЙМЕР (вызываем при победе / WinPanel) ===
    public void StopTimer()
    {
        stoppedManually = true;
        running = false;
        UpdateUI();
    }

    // Пауза/продолжение (если понадобится)
    public void PauseTimer(bool pause)
    {
        if (stoppedManually) return; // после Win не даём снова запустить
        running = !pause;
    }

    // Если захочешь перезапускать таймер вручную
    public void ResetTimer()
    {
        stoppedManually = false;
        startedOnce = false;
        timeLeft = startSeconds;
        running = !startOnlyAfterFirstMove;
        UpdateUI();
    }

    private void OnTimeUp()
    {
        // Пока делаем простое: перезапуск уровня
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}