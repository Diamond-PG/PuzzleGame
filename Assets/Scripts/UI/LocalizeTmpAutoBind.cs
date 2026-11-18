using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

[DisallowMultipleComponent]
public class LocalizeTmpAutoBind : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent localize;
    [SerializeField] private TMP_Text tmp;

    private void Reset()
    {
        localize = GetComponent<LocalizeStringEvent>();
        tmp = GetComponent<TMP_Text>();
    }

    private void Awake()
    {
        if (localize == null) localize = GetComponent<LocalizeStringEvent>();
        if (tmp == null) tmp = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (localize == null || tmp == null) return;

        // Гарантируем, что в рантайме событие пишет именно в этот TMP
        localize.OnUpdateString.RemoveListener(Apply);
        localize.OnUpdateString.AddListener(Apply);

        // Принудительно обновим сразу
        localize.RefreshString();
    }

    private void OnDisable()
    {
        if (localize == null) return;
        localize.OnUpdateString.RemoveListener(Apply);
    }

    private void Apply(string value)
    {
        if (tmp == null) return;
        tmp.SetText(value);
    }
}