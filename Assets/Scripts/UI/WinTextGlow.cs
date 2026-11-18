using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class WinTextGlow : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float minPower = 0.3f;
    [SerializeField] private float maxPower = 0.8f;
    [SerializeField] private float speed = 1.5f;

    private Material _mat;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();

        // Делаем отдельный инстанс материала, чтобы анимация не трогала другие тексты
        _mat = new Material(text.fontMaterial);
        text.fontMaterial = _mat;
    }

    private void Update()
    {
        if (_mat == null) return;

        // t плавно ходит от 0 до 1 по синусоиде
        float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
        float power = Mathf.Lerp(minPower, maxPower, t);

        // Официальный ID параметра Glow Power из TextMeshPro
        _mat.SetFloat(ShaderUtilities.ID_GlowPower, power);
    }
}