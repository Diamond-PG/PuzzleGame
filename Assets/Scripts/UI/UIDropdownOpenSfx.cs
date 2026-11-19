using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Dropdown), typeof(UIClickSfx))]
public sealed class UIDropdownOpenSfx : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private UIClickSfx sound;
    private bool wasExpanded;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        sound = GetComponent<UIClickSfx>();
    }

    private void OnEnable()
    {
        wasExpanded = dropdown.IsExpanded;
    }

    private void LateUpdate()
    {
        bool expanded = dropdown.IsExpanded;
        if (expanded && !wasExpanded)
            sound.PlayClick();

        wasExpanded = expanded;
    }
}
