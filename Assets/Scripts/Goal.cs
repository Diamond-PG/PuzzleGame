using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("Links (можно не назначать вручную)")]
    [SerializeField] private LevelCompleteUI levelCompleteUI;
    [SerializeField] private LevelTimer levelTimer;

    [Header("Player Tag")]
    [SerializeField] private string playerTag = "Player";

    [Header("Freeze player on win (Variant B)")]
    [SerializeField] private bool freezePlayerOnWin = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool triggered;

    private void Awake()
    {
        // Unity 6: FindObjectOfType устарел -> FindFirstObjectByType
        if (levelTimer == null)
            levelTimer = Object.FindFirstObjectByType<LevelTimer>();

        if (levelCompleteUI == null)
            levelCompleteUI = Object.FindFirstObjectByType<LevelCompleteUI>();

        if (debugLogs)
            Debug.Log($"[GOAL] Awake. levelTimer={(levelTimer ? "OK" : "NULL")}, levelCompleteUI={(levelCompleteUI ? "OK" : "NULL")}", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        // Важно: часто other = collider дочернего объекта игрока.
        // Поэтому проверяем и сам тег, и тег root.
        bool isPlayer =
            other.CompareTag(playerTag) ||
            (other.transform.root != null && other.transform.root.CompareTag(playerTag));

        if (debugLogs)
            Debug.Log($"[GOAL] Trigger enter: other='{other.name}' tag='{other.tag}', rootTag='{other.transform.root.tag}', isPlayer={isPlayer}", this);

        if (!isPlayer) return;

        triggered = true;

        // 1) остановить таймер (если есть)
        if (levelTimer != null)
            levelTimer.StopTimer();

        // 2) остановить музыку (если есть)
        StopGameMusic();

        // 3) ВАРИАНТ B: заморозить игрока СРАЗУ при победе
        if (freezePlayerOnWin)
            FreezePlayer(other);

        // 4) показать окно победы
        if (levelCompleteUI != null)
            levelCompleteUI.ShowPanel();
        else
            Debug.LogWarning("[GOAL] levelCompleteUI == null, победа не может быть показана!", this);
    }

    private void FreezePlayer(Collider2D other)
    {
        // Пытаемся достать PlayerController максимально надежно:
        // - из объекта rigidbody (если collider на дочке)
        // - из root
        PlayerController pc = null;

        if (other.attachedRigidbody != null)
            pc = other.attachedRigidbody.GetComponent<PlayerController>();

        if (pc == null)
            pc = other.GetComponentInParent<PlayerController>();

        if (pc == null && other.transform.root != null)
            pc = other.transform.root.GetComponent<PlayerController>();

        if (pc != null)
        {
            pc.LockMovement(true);
            if (debugLogs) Debug.Log("[GOAL] Player movement LOCKED", this);
        }
        else
        {
            Debug.LogWarning("[GOAL] Не найден PlayerController — заморозка не сработала. Проверь: PlayerController висит на объекте Player (root).", this);
        }
    }

    private void StopGameMusic()
    {
        GameObject musicObj = GameObject.Find("GameMusic");
        if (musicObj == null)
        {
            if (debugLogs) Debug.Log("[GOAL] GameMusic not found (это не ошибка).", this);
            return;
        }

        AudioSource audio = musicObj.GetComponent<AudioSource>();
        if (audio == null)
        {
            if (debugLogs) Debug.Log("[GOAL] GameMusic найден, но AudioSource на нём нет.", this);
            return;
        }

        audio.Stop();
        if (debugLogs) Debug.Log("[GOAL] GameMusic stopped.", this);
    }
}